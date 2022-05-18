using System;
using System.Collections;
using UnityEngine;

[Serializable] // be able to save it
public class TileHolder : MonoBehaviour
{
    // a script that can be placed on an object to hold tile info
    public Tile tile;
    SpriteRenderer spRenderer;

    private void Start()
    {
        WaitForFrameUpdate();
    }

    public void WaitForFrameUpdate()
    {
        // wait until it is active
        StartCoroutine(waitForFrame());
    }

    IEnumerator waitForFrame()
    {
        // set the sprite's color
        spRenderer = GetComponentInChildren<SpriteRenderer>();
        spRenderer.color = new Color(tile.colorR, tile.colorG, tile.colorB, tile.colorA);

        // wait until snapped to grid
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForEndOfFrame();

        // set tile info
        tile.tilePosX = transform.position.x;
        tile.tilePosY = transform.position.y;

        // remove old tiles underneath this
        foreach(Transform tr in transform.parent)
        {
            if(tr.position.x == this.transform.position.x && tr.position.y == this.transform.position.y)
            {
                if(tr != this.transform)
                {
                    if (ProjectPanel.instance.selectedObjects.Contains(tr.GetComponent<TileHolder>()))
                    {
                        ProjectPanel.instance.selectedObjects.Remove(tr.GetComponent<TileHolder>());
                    }
                    Destroy(tr.gameObject);
                }
            }
        }

        // destroy tile if out bounds
        if (tile.tilePosX < -100f || tile.tilePosX > 100 || tile.tilePosY < -100 || tile.tilePosY > 100)
        {
            if(ProjectPanel.instance.selectedObjects.Contains(this))
            {
                ProjectPanel.instance.selectedObjects.Remove(this);
            }
            Destroy(this.gameObject);
        }
    }

    public void SetSelected(bool _true)
    {
        // set the visual on top
        int _order = 1;
        if(_true)
        {
            _order = 2;
        }

        transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = _order;
        transform.GetChild(1).gameObject.SetActive(_true);
    }
}
