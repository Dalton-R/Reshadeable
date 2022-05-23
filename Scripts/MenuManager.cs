using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [HideInInspector]
    public Project currentProject = null;

    [Header("View Projects")]
    public Transform projectListContent;
    public GameObject projectListPrefab;
    [HideInInspector]
    public Coroutine spawningProjectsRoutine = null;

    [Header("Create Project")]
    public InputField createNameInputField;

    private void Awake()
    {
        instance = this;
    }

    public void NewCurrentProject()
    {
        // get a new instance of a project
        currentProject = new Project();
    }

    public void CreateProject(InputField _field)
    {
        // if the project name is too short
        if (ReduceWhitespace(_field.text).Length < 3)
        {
            DialogUI.Instance.SetTitle("Error!").SetMessage("You need to have at least 3 characters in the name!").AcceptOnly(true).Show();
            return;
        }

        // if we already have a project with that name
        if (ProjectExists(_field.text))
        {
            DialogUI.Instance.SetTitle("Error!").SetMessage("You already have a project with that name!").AcceptOnly(true).Show();
            return;
        }

        if (currentProject != null)
        {
            currentProject.SaveName = _field.text;
        }

        CanvasSlider.instance.SlideCanvasFromRight("LoadingArea");
    }

    public void DeleteProject()
    {
        // delete the project popup
        DialogUI.Instance.SetTitle("Are you sure?").SetMessage("Are you sure you want to delete this project?\nIt cannot be undone!").AcceptOnly(false).OnAccept(delegate { deleteCurrentProject(); }).Show();
    }

    void deleteCurrentProject()
    {
        // delete the curreent project
        SaveManager.instance.DeleteProject(currentProject);
        currentProject = null;
        SpawnProjects();
        ProjectPanel.instance.LeaveProjectPanel(false);
    }

    bool ProjectExists(string _name)
    {
        // see if we already have a project that exists

        foreach (Project lang in GameVariables.SavedProjects)
        {
            if (lang.SaveName.ToLower() == _name.ToLower())
            {
                return true;
            }
        }

        return false;
    }

    string ReduceWhitespace(string text)
    {
        // remove white space from the string

        string newText = "";
        bool inWhitespace = false;
        int posStart = 0;
        int pos = 0;
        for (pos = 0; pos < text.Length; ++pos)
        {
            char cc = text[pos];
            if (Char.IsWhiteSpace(cc))
            {
                if (!inWhitespace)
                {
                    if (pos > posStart) newText += text.Substring(posStart, pos - posStart);
                    inWhitespace = true;
                }
                posStart = pos + 1;
            }
            else
            {
                if (inWhitespace)
                {
                    inWhitespace = false;
                    posStart = pos;
                }
            }
        }

        if (pos > posStart) newText += text.Substring(posStart, pos - posStart);

        return (newText);
    }

    public void SpawnProjects()
    {
        // remove old list of projects
        foreach (Transform tr in projectListContent)
        {
            Destroy(tr.gameObject);
        }

        // if it was in the middle of spawning old projects, stop it
        if (spawningProjectsRoutine != null)
        {
            StopCoroutine(spawningProjectsRoutine);
        }
        // start the spawn projects routine
        spawningProjectsRoutine = StartCoroutine(doSpawnProjects());
    }

    IEnumerator doSpawnProjects()
    {
        foreach (Project lang in GameVariables.SavedProjects)
        {
            // create the project button
            GameObject gm = Instantiate(projectListPrefab, projectListContent);

            // when the project button is clicked
            gm.GetComponent<Button>().onClick.AddListener(delegate {
                currentProject = lang;
                CanvasSlider.instance.SlideCanvasFromRight("LoadingArea");
            });
            gm.transform.GetChild(0).GetComponent<Text>().text = lang.SaveName;

            // wait for a little bit, so it doesnt crash with a lot of projects
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void QuitGame()
    {
        // quit the game if dialog accepted
        DialogUI.Instance.SetTitle("Are you sure?").SetMessage("Are you sure you want to quit?").AcceptOnly(false).OnAccept(delegate { Application.Quit(); }).Show();
    }
}
