using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public static string projectsSavePath;
    public static string imagesSavePath;
    public static string seperatorChar;
    static string fileExtention;

    private void Awake()
    {
        instance = this;

        // set file paths
        seperatorChar = Path.DirectorySeparatorChar.ToString();
        projectsSavePath = Application.persistentDataPath + seperatorChar + "Projects";
        imagesSavePath = Application.persistentDataPath + seperatorChar + "Images";
        fileExtention = ".rsh";

        // if the directories dont exist, create them
        if (!Directory.Exists(projectsSavePath))
        {
            Directory.CreateDirectory(projectsSavePath);
        }
        if (!Directory.Exists(imagesSavePath))
        {
            Directory.CreateDirectory(imagesSavePath);
        }
    }

    public void GetSavedProjects()
    {
        try
        {
            // if the directory doesnt exist, create it
            if (!Directory.Exists(projectsSavePath))
            {
                Directory.CreateDirectory(projectsSavePath);
            }

            // get the available file names
            string[] saveFileNames = Directory.GetFiles(projectsSavePath);

            // clear the old list
            GameVariables.SavedProjects.Clear();

            // foreach file in the names
            foreach (string _str in saveFileNames)
            {
                if (File.Exists(_str))
                {
                    // get the file data
                    FileStream file = File.Open(_str, FileMode.Open);
                    BinaryFormatter bf = new BinaryFormatter();
                    Project data = (Project)bf.Deserialize(file);
                    file.Close();

                    // add the data to the list
                    GameVariables.SavedProjects.Add(data);
                }
            }

            // spawn the project buttons list
            MenuManager.instance.SpawnProjects();
        }
        catch
        {
            
        }
    }

    public void GetSavedImages()
    {
        try
        {
            // if the directory doesnt exist, create it
            if (!Directory.Exists(imagesSavePath))
            {
                Directory.CreateDirectory(imagesSavePath);
            }

            // get the available file names
            string[] saveFileNames = Directory.GetFiles(imagesSavePath);

            // clear the old list
            GameVariables.SavedImages.Clear();

            // foreach file in the names
            foreach (string _str in saveFileNames)
            {
                if (File.Exists(_str))
                {
                    // add the file name to the list
                    GameVariables.SavedImages.Add(_str);
                }
            }
        }
        catch
        {
            
        }
    }

    public void SaveProject(Project _lng)
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Open(projectsSavePath + seperatorChar + _lng.SaveName + fileExtention, FileMode.OpenOrCreate);
            //FileStream file = File.Open(projectsSavePath + seperatorChar + _lng.SaveName + fileExtention, FileMode.Create);

            // set the data to a new project
            Project data = new Project();
            data.SaveName = _lng.SaveName;
            data.Tiles = _lng.Tiles;

            // set the project to a file
            bf.Serialize(file, data);
            file.Close();
        }
        catch
        {
            
        }
    }

    public void DeleteProject(Project data)
    {
        File.Delete(projectsSavePath + seperatorChar + data.SaveName + fileExtention);
        GetSavedProjects();
    }

    public void CaptureScreenshot(float x, float y, float width, float height, string _fileName, List<GameObject> objectsToDisable, List<GameObject> objects1, List<GameObject> objects2, bool multiple, int number)
    {
        StartCoroutine(captureScreenshot(x, y, width, height, _fileName, objectsToDisable, objects1, objects2, multiple, number));
    }

    IEnumerator captureScreenshot(float x, float y, float width, float height, string _fileName, List<GameObject> objectsToDisable, List<GameObject> objects1, List<GameObject> objects2, bool multiple, int number)
    {
        // remove UI and other stuff out of image
        foreach(GameObject gm in objectsToDisable)
        {
            gm.SetActive(false);
        }

        // set the non-shadeable objects invisible (if any)
        foreach (GameObject gm in objects2)
        {
            gm.SetActive(false);
        }

        // take screenshot
        yield return new WaitForEndOfFrame();
        string fileName1 = _fileName + number.ToString();
        zzTransparencyCapture.captureScreenshot(x, y, width, height, fileName1);

        // if it was a split image
        string fileName2 = "";
        if (multiple)
        {
            number++;

            // set the non-shadeable objects visible
            foreach (GameObject gm in objects2)
            {
                gm.SetActive(true);
            }
            foreach (GameObject gm in objects1)
            {
                gm.SetActive(false);
            }

            // take screenshot
            yield return new WaitForEndOfFrame();
            fileName2 = _fileName + number.ToString();
            zzTransparencyCapture.captureScreenshot(x, y, width, height, fileName2);

            // bring back the shadeable objects
            foreach (GameObject gm in objects1)
            {
                gm.SetActive(true);
            }
        }

        // bring back the UI stuff
        foreach (GameObject gm in objectsToDisable)
        {
            gm.SetActive(true);
        }

        // bring back the camera
        GetSavedImages();
        ProjectPanel.instance.mainCamera.transform.GetComponent<CameraZoomAndMoveFromTouch>().enabled = true;
        ProjectPanel.instance.SetCameraToSettings();

        // if android, share image, if PC, give message to open folder
#if UNITY_ANDROID
        ShareScreenshot(fileName1, fileName2);
#else
        DialogUI.Instance.SetTitle("Success!").SetMessage("Successfully saved your screenshot files!").AcceptOnly(true).Show();
#endif
    }

    public void OpenImagesFolder()
    {
        // for PC
        Application.OpenURL(imagesSavePath);
    }

    public void ShareProject(Project data)
    {
        // share the project
        new NativeShare().AddFile(imagesSavePath + Path.DirectorySeparatorChar + data.SaveName + fileExtention)
        .SetSubject($"Language: {data.SaveName}").SetText("").SetUrl("")
        .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
        .Share();
    }

    public void ShareScreenshot(string imageName1, string imageName2)
    {
        // share the screenshot(s)
        NativeShare nshare = new NativeShare();
        nshare.AddFile(imagesSavePath + Path.DirectorySeparatorChar + imageName1 + ".png");

        // if it was a split image
        if(!string.IsNullOrEmpty(imageName2))
        {
            nshare.AddFile(imagesSavePath + Path.DirectorySeparatorChar + imageName2 + ".png");
        }
        nshare.SetSubject($"Reshadeable image: {MenuManager.instance.currentProject.SaveName}").SetText("").SetUrl("");
        nshare.SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget));
        nshare.Share();
    }
}
