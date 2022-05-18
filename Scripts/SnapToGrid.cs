using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    // the size of the grid
    public float grid = 5f;
    float x = 0f;
    float y = 0f;

    void Update()
    {
        // if >0, so no div by 0 errors
        if (grid > 0)
        {
            float reciprocalGrid = 1f / grid;

            // round to that value
            x = Mathf.Round(transform.position.x * reciprocalGrid) / reciprocalGrid;
            y = Mathf.Round(transform.position.y * reciprocalGrid) / reciprocalGrid;

            // go to the rounded position
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
