using System.Collections;
using UnityEngine;

public class ScrollViewLOD : MonoBehaviour
{
    // a script to set objects inactive in scroll views to avoid lag

    public GameObject[] itemsToLoad;

    RectTransform holder;

    float minXValue = 0;
    float maxXValue = 0;
    float thisWidth = 0;
    float scrollX;

    float minYValue = 0;
    float maxYValue = 0;
    float thisHeight = 0;
    float scrollY;

    bool hidden = false;

    bool canAffect = false;

    public bool doX;
    public bool doY;

    float OFFSET = 0f;

    private void Start()
    {
        StartCoroutine(waitForStart());
    }

    public IEnumerator waitForStart()
    {
        // wait until object is in the list
        yield return new WaitForSeconds(0.01f);
        Initialize();
    }

    public void Initialize()
    {
        holder = transform.parent.GetComponent<RectTransform>();

        OFFSET = -holder.parent.transform.parent.GetComponent<RectTransform>().rect.height;

        // if the scroll view supports x scrolling
        if (doX)
        {
            thisWidth = GetComponent<RectTransform>().rect.width;
            float posX = GetComponent<RectTransform>().localPosition.x;
            minXValue = posX + thisWidth;
            maxXValue = posX - holder.parent.transform.parent.GetComponent<RectTransform>().rect.width - thisWidth;
        }

        // if the scroll view supports y scrolling
        if (doY)
        {
            thisHeight = GetComponent<RectTransform>().rect.height;
            float posY = GetComponent<RectTransform>().localPosition.y;
            minYValue = posY + thisHeight;
            maxYValue = posY - holder.parent.transform.parent.GetComponent<RectTransform>().rect.height - thisHeight;
        }

        canAffect = true;
    }

    private void Update()
    {
        if (holder == null || !canAffect)
            return;

        if (doX)
        {
            scrollX = -holder.localPosition.x + OFFSET;

            if (scrollX > minXValue)
            {
                if (!hidden)
                {
                    hidden = true;
                    SetObj();
                }
            }
            else if (scrollX < maxXValue)
            {
                if (!hidden)
                {
                    hidden = true;
                    SetObj();
                }
            }
            else
            {
                if (hidden)
                {
                    hidden = false;
                    SetObj();
                }
            }
        }

        if (doY)
        {
            scrollY = -holder.localPosition.y + OFFSET;

            if (scrollY > minYValue)
            {
                if (!hidden)
                {
                    hidden = true;
                    SetObj();
                }
            }
            else if (scrollY < maxYValue)
            {
                if (!hidden)
                {
                    hidden = true;
                    SetObj();
                }
            }
            else
            {
                if (hidden)
                {
                    hidden = false;
                    SetObj();
                }
            }
        }
    }

    void SetObj()
    {
        foreach (GameObject gm in itemsToLoad)
        {
            gm.SetActive(!hidden);
        }
    }
}
