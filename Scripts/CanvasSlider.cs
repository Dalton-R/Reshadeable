using System.Collections;
using UnityEngine;

public class CanvasSlider : MonoBehaviour
{
    public static CanvasSlider instance;

    float SlideTime = 12f;

    public Transform canvasHolder;

    GameObject currentCanvas;
    GameObject oldCanvas;
    Vector3 oldCanvasDesiredPos;

    Coroutine currentSetActiveRoutine = null;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // set the first canvas as active
        if (canvasHolder.childCount > 0)
        {
            currentCanvas = canvasHolder.GetChild(0).gameObject;
            currentCanvas.SetActive(true);
            CanvasOpen(currentCanvas.name);

            CurrentOnlyActive();
        }
    }

    public void CurrentOnlyActive()
    {
        // set all canvases other than opened to inactive
        foreach (Transform tr in canvasHolder)
        {
            if (tr.gameObject != currentCanvas)
            {
                tr.gameObject.SetActive(false);
            }
        }
    }

    public void SlideCanvasFromRight(string _name)
    {
        slideCanvas(_name, new Vector3(Screen.width * 2f, 0f, 0f));
    }

    public void SlideCanvasFromLeft(string _name)
    {
        slideCanvas(_name, new Vector3(-Screen.width * 2f, 0f, 0f));
    }

    public void SlideCanvasFromTop(string _name)
    {
        slideCanvas(_name, new Vector3(0f, Screen.height * 2f, 0f));
    }

    public void SlideCanvasFromBottom(string _name)
    {
        slideCanvas(_name, new Vector3(0f, -Screen.height * 2f, 0f));
    }

    void slideCanvas(string _name, Vector3 _startPos)
    {
        oldCanvas = currentCanvas;
        oldCanvasDesiredPos = -_startPos;

        // find and move the canvas
        for (int i = 0; i < canvasHolder.childCount; i++)
        {
            if (canvasHolder.GetChild(i).name == _name)
            {
                canvasHolder.GetChild(i).transform.localPosition = _startPos;
                if (currentSetActiveRoutine != null)
                {
                    StopCoroutine(currentSetActiveRoutine);
                }
                currentSetActiveRoutine = StartCoroutine(waitToSetActive(canvasHolder.GetChild(i).gameObject));
                canvasHolder.GetChild(i).gameObject.GetComponent<RectTransform>().SetAsLastSibling();
                CanvasOpen(_name);
                break;
            }
        }
    }

    void CanvasOpen(string _name)
    {
        // any special scripts for opening a canvas

        MenuManager manager = MenuManager.instance;

        if (_name == "CreateProject")
        {
            manager.NewCurrentProject();
            manager.createNameInputField.text = manager.currentProject.SaveName;
        }
        if(_name == "LoadingArea")
        {
            ProjectPanel.instance.OnOpen();
        }

        // Stop Coroutines
        if (MenuManager.instance.spawningProjectsRoutine != null && _name != "ProjectsList")
        {
            MenuManager.instance.StopCoroutine(MenuManager.instance.spawningProjectsRoutine);
        }
    }

    IEnumerator waitToSetActive(GameObject gm)
    {
        gm.SetActive(true);
        currentCanvas = gm;

        yield return new WaitForSeconds(0.7f);

        CurrentOnlyActive();
    }

    private void Update()
    {
        // move the canvases smoothly
        if (currentCanvas != null)
        {
            currentCanvas.transform.localPosition = Vector3.Lerp(currentCanvas.transform.localPosition, Vector3.zero, SlideTime * Time.deltaTime);
        }
        if(oldCanvas != null)
        {
            oldCanvas.transform.localPosition = Vector3.Lerp(oldCanvas.transform.localPosition, oldCanvasDesiredPos, SlideTime * Time.deltaTime);
        }
    }
}
