using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoomAndMoveFromTouch : MonoBehaviour
{
    public static CameraZoomAndMoveFromTouch instance;

    public float zoomOutMin = 15;
    public float zoomOutMax = 70;

    public float maxXMove = 1450;
    public float maxYMove = 1450;

    private Vector3 touchStart;
    public Transform cameraPos;

    public float cameraMoveModifier = 2f;
    public float cameraZoomModifier = 0.01f;
    public float moveBeforeMove = 5f;
    [HideInInspector]
    public bool _doMove = false;
    bool zooming = false;
    bool lastTouching = false;
    bool canPlace = false;
    [HideInInspector]
    public bool isMovingTiles = false;

    [HideInInspector]
    public bool wasTeardropping = false;
    [HideInInspector]
    public bool wasSelecting = false;

    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        bool _overObject = false;

        // if we clicked on a ui panel
        if (EventSystem.current.IsPointerOverGameObject())
        {
            _overObject = true;
        }
        // if we clicked on an invisible cube
        RaycastHit hit;
        Vector2 _mouseSpawn = GetComponent<Camera>().ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        if (Physics.Raycast(new Vector3(_mouseSpawn.x, _mouseSpawn.y, transform.position.z), Vector3.forward, out hit, 20f))
        {
            if (hit.collider.CompareTag("Untagged"))
            {
                _overObject = true;
            }
        }

        // get mouse position
        Vector3 _mouse = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0f);

        // if start touch
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = _mouse;
            _doMove = false;
            canPlace = true;
        }

        // stop the camera movement if over a ui panel
        if (Input.GetMouseButton(0))
        {
            if (_overObject)
            {
                lastTouching = false;
                //return;
                canPlace = false;
            }
        }

        if (Input.GetMouseButton(0) && lastTouching == false)
        {
            touchStart = _mouse;
            _doMove = false;
            lastTouching = true;
        }

        // if 2 fingers, we are zooming
        if (Input.touchCount == 2)
        {
            zooming = true;
        }

        if (zooming)
        {
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                Zoom(difference * cameraZoomModifier);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 direction = touchStart - _mouse;

            // move only after the threshold
            if (direction.magnitude > moveBeforeMove && !wasSelecting && !isMovingTiles && !_overObject)
            {
                _doMove = true;
            }

            // move the camera
            if (_doMove)
            {
                direction *= cameraMoveModifier;
                cameraPos.transform.position += direction;

                // dont go past boundaries
                if (cameraPos.transform.localPosition.x > maxXMove)
                {
                    cameraPos.transform.localPosition = new Vector3(maxXMove, cameraPos.transform.localPosition.y, cameraPos.transform.localPosition.z);
                }
                if (cameraPos.transform.localPosition.x < -maxXMove)
                {
                    cameraPos.transform.localPosition = new Vector3(-maxXMove, cameraPos.transform.localPosition.y, cameraPos.transform.localPosition.z);
                }
                if (cameraPos.transform.localPosition.y > maxYMove)
                {
                    cameraPos.transform.localPosition = new Vector3(cameraPos.transform.localPosition.x, maxYMove, cameraPos.transform.localPosition.z);
                }
                if (cameraPos.transform.localPosition.y < -maxYMove)
                {
                    cameraPos.transform.localPosition = new Vector3(cameraPos.transform.localPosition.x, -maxYMove, cameraPos.transform.localPosition.z);
                }

                touchStart = _mouse;
            }
        }

        // if we are able to place/remove a tile
        if(Input.GetMouseButtonUp(0) && !zooming && !_doMove)
        {
            if (ProjectPanel.instance.inPanel && !wasTeardropping && !wasSelecting && !isMovingTiles && !_overObject && canPlace)
            {
                Vector2 _spawn = GetComponent<Camera>().ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                ProjectPanel.instance.MousePress(_spawn);
            }

            wasTeardropping = false;
            wasSelecting = false;
            isMovingTiles = false;
        }

        // if we let go after zooming
        if (Input.touchCount == 0 && zooming)
        {
            zooming = false;
        }

        // zoom for PC
        Zoom(Input.GetAxis("Mouse ScrollWheel") * 50);
    }

    void Zoom(float increment)
    {
        // set the camera orthographic size for zoom
        GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize - increment, zoomOutMin, zoomOutMax);
    }
}
