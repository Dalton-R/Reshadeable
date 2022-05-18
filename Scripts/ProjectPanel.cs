using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ProjectPanel : MonoBehaviour
{
    public static ProjectPanel instance;

    public bool inPanel;

    public Transform projectArea;
    public Transform tileContainer;
    public List<GameObject> objectsToSetActiveWhenLoad = new List<GameObject>();
    public Animator mainPanel;
    public Animator sidePanel;
    public Animator bgPanelAnimator;
    public Animator imageInfoAnimator;
    public List<GameObject> objectsToDisableForCapture = new List<GameObject>();

    public GameObject tilePrefab;
    Tile tileSettings = new Tile();
    public Button opacityColorButton;
    public Button tileColorButton;
    public GameObject reshadeYesButton;
    public GameObject reshadeNoButton;

    public GameObject editShadeArea;
    public Slider editShadeSlider;
    public Slider editShadeAlphaSlider;

    public GameObject editColorArea;
    public Slider editColorRSlider;
    public Slider editColorGSlider;
    public Slider editColorBSlider;
    public Slider editColorASlider;

    [HideInInspector]
    public bool teardropping = false;

    [HideInInspector]
    public bool selecting = false;
    [HideInInspector]
    public bool hasSelected = false;

    bool _adding = true;
    public Button addButton;
    public Button deleteButton;

    // CAMERA SETTINGS
    public Camera mainCamera;
    public Transform camTargetPos;
    float camSize = 5f;
    Vector3 camPos = Vector2.zero;

    public GameObject singleImageInfoClose;
    public GameObject bothImageInfoClose;

    [HideInInspector]
    public List<TileHolder> selectedObjects = new List<TileHolder>();

    private void Awake()
    {
        instance = this;
    }

    public void OnOpen()
    {
        // when we open the panel, set everything up

        CreateCanvas();

        tileSettings = new Tile();
        SetTileSettingsVisuals();
        SetTileReshadeable(tileSettings.tileShadeable);
        SetAdd(true);
        selectedObjects.Clear();
        hasSelected = false;
    }

    public void ClearCanvas()
    {
        // clear all old tiles
        foreach(Transform tr in tileContainer)
        {
            Destroy(tr.gameObject);
        }
    }

    public void CreateCanvas()
    {
        ClearCanvas();

        StartCoroutine(spawnTiles());
    }

    IEnumerator spawnTiles()
    {
        bgPanelAnimator.Play("BGCanvasDarkFadeIn");

        float _time = 0f;

        //spawn the tiles
        foreach (Tile tile in MenuManager.instance.currentProject.Tiles)
        {
            TileHolder til = Instantiate(tilePrefab, new Vector2(tile.tilePosX, tile.tilePosY), Quaternion.identity, tileContainer).GetComponent<TileHolder>();
            til.tile = tile;
            yield return new WaitForSeconds(0.001f);
            _time += 0.001f;
        }

        // allow time for the loading canvas to fade in/out
        float _maxTime = 1f;
        if(_time < _maxTime)
        {
            float _timeToWait = _maxTime - _time;
            yield return new WaitForSeconds(_timeToWait);
        }

        CanvasSlider.instance.SlideCanvasFromRight("ProjectArea");
        mainPanel.Play("ProjectAreaFadeIn");
        yield return new WaitForSeconds(0.1f);
        foreach(GameObject gm in objectsToSetActiveWhenLoad)
        {
            gm.SetActive(true);
        }

        CenterCamera(false);

        // if this, so it doesn't set to 0
        if(MenuManager.instance.currentProject.Tiles.Count == 0)
        {
            mainCamera.orthographicSize = 35f;
        }

        SaveCameraSettings();
        inPanel = true;
    }

    public void MousePress(Vector2 pos)
    {
        if (teardropping)
            return;

        if (selecting)
            return;

        if(hasSelected)
        {
            ClearSelected();
            return;
        }

        if (imageInfoAnimator.GetComponent<CanvasGroup>().alpha > 0.1f)
            return;

        if (_adding)
        {
            // add a tile if within bounds
            if (!(pos.x < -100.49f || pos.x > 100.49f || pos.y < -100.49f || pos.y > 100.49f))
            {
                TileHolder til = Instantiate(tilePrefab, pos, Quaternion.identity, tileContainer).GetComponent<TileHolder>();
                til.tile = SetTileSettings(tileSettings);
            }
        }
        else
        {
            // remove a tile where pressed
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(pos.x, pos.y, mainCamera.transform.position.z), Vector3.forward, out hit, 20f))
            {
                if (hit.collider.CompareTag("Tile"))
                {
                    Destroy(hit.transform.gameObject);
                }
            }
        }
    }

    public void ClearSelected()
    {
        // set the tiles to not selected
        foreach (Transform tr in tileContainer)
        {
            tr.GetComponent<TileHolder>().SetSelected(false);
        }
        hasSelected = false;
        //selectedObjects.Clear();
    }

    public void ButtonClicked(string _name)
    {
        if(_name == "Add")
        {
            SetAdd(true);
        }
        if(_name == "Delete")
        {
            SetAdd(false);
        }
        if(_name == "Screenshot")
        {
            DialogUI.Instance.SetTitle("Are you sure?").SetMessage("Are you sure you want to export an image?").AcceptOnly(false).OnAccept(delegate { ScreenShot(false); }).Show();
        }
        if (_name == "ScreenshotSplit")
        {
            DialogUI.Instance.SetTitle("Are you sure?").SetMessage("Are you sure you want to export the images?").AcceptOnly(false).OnAccept(delegate { ScreenShot(true); }).Show();
        }
        if (_name == "Teardrop")
        {
            StartCoroutine(teardrop());
        }
        if(_name == "Select")
        {
            StartCoroutine(select());
        }
        if(_name == "EditOpen")
        {
            sidePanel.Play("EditOpen");
        }
        if (_name == "EditClose")
        {
            sidePanel.Play("EditClose");
            editShadeArea.SetActive(false);
            editColorArea.SetActive(false);
        }
        if(_name == "Shade")
        {
            bool _active = editShadeArea.activeInHierarchy;
            if(!_active)
            {
                List<float> averageFloats = new List<float>();
                averageFloats.Add(tileSettings.colorR);
                averageFloats.Add(tileSettings.colorG);
                averageFloats.Add(tileSettings.colorB);
                editShadeSlider.value = GetAverage(averageFloats);
                editShadeAlphaSlider.value = tileSettings.colorA;
            }
            editShadeArea.SetActive(!_active);
        }
        if(_name == "Color")
        {
            bool _active = editColorArea.activeInHierarchy;
            if (!_active)
            {
                editColorRSlider.value = tileSettings.colorR;
                editColorGSlider.value = tileSettings.colorG;
                editColorBSlider.value = tileSettings.colorB;
                editColorASlider.value = tileSettings.colorA;
            }
            editColorArea.SetActive(!_active);
        }
        if(_name == "ImageInfo")
        {
            imageInfoAnimator.Play("ImageInfoOpen");

            // if android, it will auto share
#if UNITY_ANDROID
            singleImageInfoClose.SetActive(true);
            bothImageInfoClose.SetActive(false);
#else
            singleImageInfoClose.SetActive(false);
            bothImageInfoClose.SetActive(true);
#endif
        }
    }

    float GetAverage(List<float> floats)
    {
        //get an average of list of numbers
        float _numFloats = (float)floats.Count;

        float _total = 0;
        foreach(float flot in floats)
        {
            _total += flot;
        }

        return _total / _numFloats;
    }

    public void SetAdd(bool _true)
    {
        // set if we are adding/removing a tile
        _adding = _true;
        addButton.interactable = !_true;
        deleteButton.interactable = _true;
    }

    public void SetTileReshadeable(bool _true)
    {
        // set the tile if we can reshade it or not
        tileSettings.tileShadeable = _true;
        reshadeNoButton.SetActive(!_true);
        reshadeYesButton.SetActive(_true);

        if(_true)
        {
            List<float> averageFloats = new List<float>();
            averageFloats.Add(tileSettings.colorR);
            averageFloats.Add(tileSettings.colorG);
            averageFloats.Add(tileSettings.colorB);
            SetTileSettingsColorShade(GetAverage(averageFloats));
        }

        editShadeArea.SetActive(false);
        editColorArea.SetActive(false);

        SetTileSettingsVisuals();
    }

    public void SetTileSettingsVisuals()
    {
        Color _tileColor = new Color(tileSettings.colorR, tileSettings.colorG, tileSettings.colorB, tileSettings.colorA);

        opacityColorButton.GetComponent<Image>().color = _tileColor;
        tileColorButton.GetComponent<Image>().color = _tileColor;
    }

    public void SetTileSettingsColorShade(float _value)
    {
        tileSettings.colorR = _value;
        tileSettings.colorG = _value;
        tileSettings.colorB = _value;
        SetTileSettingsVisuals();
    }

    public void SetTileSettingsColorR(float _value)
    {
        tileSettings.colorR = _value;
        SetTileSettingsVisuals();
    }
    public void SetTileSettingsColorG(float _value)
    {
        tileSettings.colorG = _value;
        SetTileSettingsVisuals();
    }
    public void SetTileSettingsColorB(float _value)
    {
        tileSettings.colorB = _value;
        SetTileSettingsVisuals();
    }
    public void SetTileSettingsColorA(float _value)
    {
        tileSettings.colorA = _value;
        SetTileSettingsVisuals();
    }

    public void ScreenShot(bool _multiple)
    {
        mainCamera.transform.GetComponent<CameraZoomAndMoveFromTouch>().enabled = false;
        SaveCameraSettings();
        CenterCamera(true);

        // get the next available number to save an image
        SaveManager.instance.GetSavedImages();
        int _number = 0;
        foreach(string str in GameVariables.SavedImages)
        {
            string _path = SaveManager.imagesSavePath + Path.DirectorySeparatorChar + MenuManager.instance.currentProject.SaveName;
            if (str.Length >= _path.Length)
            {
                string _substr = str.Substring(0, _path.Length);
                if (_substr == _path)
                {
                    int _num = -1;
                    string _intStr = str.Remove(str.Length - 4);
                    _intStr = _intStr.Substring(_substr.Length);
                    if(int.TryParse(_intStr, out _num))
                    {
                        if(_num > _number)
                        {
                            _number = _num;
                        }
                    }
                }
            }
        }
        _number++;

        // if there is at least 1 tile
        if (tileContainer.childCount > 0)
        {
            // calculate where to take image
            Bounds bounds = GetBounds();

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            // change to box size
            min.z = 0f;
            max.z = 0f;
            min.x -= 0.5f;
            min.y -= 0.5f;
            max.x += 0.5f;
            max.y += 0.5f;

            Vector3 screenMin = mainCamera.WorldToScreenPoint(min);
            Vector3 screenMax = mainCamera.WorldToScreenPoint(max);
            float screenWidth = screenMax.x - screenMin.x;
            float screenHeight = screenMax.y - screenMin.y;

            Vector3 screenCenter = mainCamera.WorldToScreenPoint(min);

            bool _canDo = false;
            List<GameObject> objects1 = new List<GameObject>();
            List<GameObject> objects2 = new List<GameObject>();

            // if we are splitting to 2 images
            if(_multiple)
            {
                bool _hasShadeable = false;
                bool _hasNoShadeable = false;

                // see if we have both shadeable, and non-shadeable tiles
                foreach(Transform tr in tileContainer)
                {
                    Tile _til = tr.GetComponent<TileHolder>().tile;
                    if (_til.tileShadeable)
                    {
                        _hasShadeable = true;
                        objects1.Add(tr.gameObject);
                    }
                    else
                    {
                        _hasNoShadeable = true;
                        objects2.Add(tr.gameObject);
                    }
                }

                // if we have both
                if(_hasShadeable && _hasNoShadeable)
                {
                    _canDo = true;
                }
            }
            else
            {
                foreach (Transform tr in tileContainer)
                {
                    objects1.Add(tr.gameObject);
                }

                _canDo = true;
            }

            if (_canDo)
            {
                SaveManager.instance.CaptureScreenshot(screenCenter.x, screenCenter.y, screenWidth, screenHeight, MenuManager.instance.currentProject.SaveName, objectsToDisableForCapture, objects1, objects2, _multiple, _number);
            }
            else
            {
                mainCamera.transform.GetComponent<CameraZoomAndMoveFromTouch>().enabled = true;
                DialogUI.Instance.SetTitle("Error!").SetMessage("You don't have enough required tiles!").AcceptOnly(true).Show();
            }
        }
        else
        {
            mainCamera.transform.GetComponent<CameraZoomAndMoveFromTouch>().enabled = true;
            DialogUI.Instance.SetTitle("Error!").SetMessage("You don't have enough required tiles!").AcceptOnly(true).Show();
        }
    }

    public void SaveCameraSettings()
    {
        camSize = mainCamera.orthographicSize;
        camPos = mainCamera.transform.position;
    }

    public void SetCameraToSettings()
    {
        mainCamera.orthographicSize = camSize;
        Vector3 _targetPos = new Vector3(camPos.x, camPos.y, mainCamera.transform.position.z);
        camTargetPos.position = _targetPos;
        mainCamera.transform.position = _targetPos;
    }

    public void CenterCamera(bool _screenShot)
    {
        // center the camera to the tiles
        Vector3 _centerPoint = GetCenterPoint();
        Vector3 _desiredPos = new Vector3(_centerPoint.x, _centerPoint.y, mainCamera.transform.position.z);
        camTargetPos.position = _desiredPos;
        mainCamera.transform.position = _desiredPos;
        Bounds bounds = GetBounds();

        // if screenshot, make sure we have enough space, or error
        if(_screenShot)
        {
            mainCamera.orthographicSize = bounds.size.x + 15f;
        }
        else
        {
            mainCamera.orthographicSize = bounds.size.x + 1f;
        }

        if (mainCamera.orthographicSize < CameraZoomAndMoveFromTouch.instance.zoomOutMin)
        {
            mainCamera.orthographicSize = CameraZoomAndMoveFromTouch.instance.zoomOutMin;
        }
    }

    Vector3 GetCenterPoint()
    {
        // get the center point of the tiles
        if(tileContainer.childCount == 0)
        {
            return Vector3.zero;
        }
        if(tileContainer.childCount == 1)
        {
            return tileContainer.GetChild(0).position;
        }

        var bounds = GetBounds();
        return bounds.center;
    }

    Bounds GetBounds()
    {
        // get the bounding area of the tiles
        List<Transform> tiles = new List<Transform>();
        foreach (Transform tr in tileContainer)
        {
            tiles.Add(tr);
        }

        if(tiles.Count == 0)
        {
            Bounds _bounds = new Bounds();
            Vector3 _size = Vector3.zero;
            _size.x = CameraZoomAndMoveFromTouch.instance.zoomOutMin;
            _bounds.size = _size;
            _bounds.center = Vector3.zero;
            return _bounds;
        }

        var bounds = new Bounds(tiles[0].position, Vector3.zero);
        for (int i = 0; i < tiles.Count; i++)
        {
            bounds.Encapsulate(tiles[i].position);
        }

        return bounds;
    }

    IEnumerator teardrop()
    {
        projectArea.gameObject.SetActive(false);
        teardropping = true;
        CameraZoomAndMoveFromTouch.instance.wasTeardropping = true;

        // wait until the user presses down
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // see if on object
        RaycastHit hit;
        if(Physics.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, out hit, 20f))
        {
            if(hit.collider.CompareTag("Tile"))
            {
                tileSettings = SetTileSettings(hit.collider.GetComponent<TileHolder>().tile);
                SetTileSettingsVisuals();
                SetTileReshadeable(tileSettings.tileShadeable);
            }
        }
        teardropping = false;
        projectArea.gameObject.SetActive(true);
    }

    IEnumerator select()
    {
        ClearSelected();
        selectedObjects.Clear();

        projectArea.gameObject.SetActive(false);
        selecting = true;
        CameraZoomAndMoveFromTouch.instance.wasSelecting = true;

        // wait until the user presses down
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        Vector3 _startPos = roundVector3(mainCamera.ScreenToWorldPoint(Input.mousePosition), 1f);
        // wait until the user presses up
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
        Vector3 _endPos = roundVector3(mainCamera.ScreenToWorldPoint(Input.mousePosition), 1f);

        // get the minimum and maximum positions
        float _minX = 0f;
        float _minY = 0f;
        float _maxX = 0f;
        float _maxY = 0f;
        if (_startPos.x > _endPos.x)
        {
            _minX = _endPos.x;
            _maxX = _startPos.x;
        }
        else
        {
            _minX = _startPos.x;
            _maxX = _endPos.x;
        }
        if (_startPos.y > _endPos.y)
        {
            _minY = _endPos.y;
            _maxY = _startPos.y;
        }
        else
        {
            _minY = _startPos.y;
            _maxY = _endPos.y;
        }

        // get the selected objects within the bounds
        foreach (Transform tr in tileContainer)
        {
            if(tr.position.x >= _minX && tr.position.x <= _maxX && tr.position.y >= _minY && tr.position.y <= _maxY)
            {
                TileHolder _tilHolder = tr.GetComponent<TileHolder>();
                _tilHolder.SetSelected(true);
                selectedObjects.Add(_tilHolder);
            }
        }

        // if we have selected at least 1 object
        if (selectedObjects.Count > 0)
        {
            hasSelected = true;
            selecting = false;
            projectArea.gameObject.SetActive(true);

            do
            {
                if (selectedObjects.Count == 0)
                {
                    hasSelected = false;
                }

                // wait until the user presses down
                yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
                Vector3 _mouseStart = mainCamera.ScreenToWorldPoint(Input.mousePosition);

                // see if on object
                RaycastHit hit;
                if (Physics.Raycast(_mouseStart, Vector3.forward, out hit, 20f) && hasSelected)
                {
                    if (hit.collider.CompareTag("Tile"))
                    {
                        if (selectedObjects.Contains(hit.collider.GetComponent<TileHolder>()))
                        {
                            CameraZoomAndMoveFromTouch.instance.isMovingTiles = true;
                            do
                            {
                                // get the position change per tile
                                Vector3 _mouseChange = mainCamera.ScreenToWorldPoint(Input.mousePosition) - _mouseStart;
                                foreach (TileHolder til in selectedObjects)
                                {
                                    // set the tiles to the new position
                                    Vector3 _newPos = new Vector3(til.tile.tilePosX + _mouseChange.x, til.tile.tilePosY + _mouseChange.y, 0f);
                                    til.transform.position = _newPos;
                                }
                                yield return new WaitForFixedUpdate();
                            } while (Input.GetMouseButton(0));

                            // update the tile's settings, and remove underneath tiles
                            foreach (TileHolder til in selectedObjects)
                            {
                                til.WaitForFrameUpdate();
                            }
                        }
                    }
                }
            } while (hasSelected);
        }
    }

    // basically the snap to grid script
    Vector3 roundVector3(Vector3 _pos, float gridSize)
    {
        Vector3 _retVec = Vector3.zero;

        if (gridSize > 0)
        {
            float reciprocalGrid = 1f / gridSize;

            // round to that value
            float x = Mathf.Round(_pos.x * reciprocalGrid) / reciprocalGrid;
            float y = Mathf.Round(_pos.y * reciprocalGrid) / reciprocalGrid;

            // go to the rounded position
            _retVec = new Vector3(x, y, transform.position.z);
        }

        return _retVec;
    }

    public Tile SetTileSettings(Tile _tile)
    {
        Tile _til = new Tile();
        _til.colorR = _tile.colorR;
        _til.colorG = _tile.colorG;
        _til.colorB = _tile.colorB;
        _til.colorA = _tile.colorA;
        _til.tileShadeable = _tile.tileShadeable;
        _til.tilePosX = _tile.tilePosX;
        _til.tilePosY = _tile.tilePosY;
        return _til;
    }

    public void LeaveProjectPanel(bool _save)
    {
        // if we want to save the project to a file
        if (_save)
        {
            Project _saveProj = new Project();
            _saveProj.SaveName = MenuManager.instance.currentProject.SaveName;

            foreach (Transform tr in tileContainer)
            {
                Tile _refTile = tr.GetComponent<TileHolder>().tile;
                Tile _newTile = SetTileSettings(_refTile);
                _saveProj.Tiles.Add(_newTile);
            }
            SaveManager.instance.SaveProject(_saveProj);
        }

        foreach (GameObject gm in objectsToSetActiveWhenLoad)
        {
            gm.SetActive(false);
        }
        inPanel = false;
        mainPanel.Play("ProjectAreaFadeOut");
        bgPanelAnimator.Play("BGCanvasDarkFadeOut");

        // if the side/image info panels were open, close them
        if (sidePanel.GetComponent<CanvasGroup>().alpha > 0.9f)
        {
            sidePanel.Play("EditClose");
        }
        if(imageInfoAnimator.GetComponent<CanvasGroup>().alpha > 0.9f)
        {
            imageInfoAnimator.Play("ImageInfoClose");
        }

        // open the menu
        CanvasSlider.instance.SlideCanvasFromLeft("Menu");
    }
}
