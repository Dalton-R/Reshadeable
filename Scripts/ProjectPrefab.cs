using UnityEngine;

// a prefab to store a project
[CreateAssetMenu(menuName = "New Project")]
public class ProjectPrefab : ScriptableObject
{
    // project holder
    public Project project;
}
