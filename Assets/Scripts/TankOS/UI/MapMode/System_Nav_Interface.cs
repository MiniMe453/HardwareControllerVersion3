using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Rover.OS;
using UnityEngine.InputSystem;
using Rover.Interface;
using Rover.Settings;
using Rover.Arduino;

public class System_Nav_Interface : MonoBehaviourApplication
{
    public RenderCameraToTexture mapCamera;
    private Vector2 m_currentMoveDir;
    public Canvas canvas;
    [Header("Cursor Variables")]
    public float cameraMoveSpeed;
    public TextMeshProUGUI cursorElevationText;
    public TextMeshProUGUI cursorGPSCoordText;
    private Vector2 m_cursorGPSCoords;
    private bool m_cursorConnectedToRover = true;

    [Header("Rover Positioning Variables")]
    public TextMeshProUGUI roverGPSCoordText;
    public TextMeshProUGUI roverHeadingText;
    public TextMeshProUGUI roverElevationText;
    [Header("MapMarkers")]
    public RectTransform mapMarkerTransform;
    public GameObject mapMarkerTransformEntry;

    private MessageBox m_brakeWarningBox;
    private float m_horizontalAxis;
    private float m_verticalAxis;


    protected override void Init()
    {
        applicationInputs.AddAction("goup", binding:"<Keyboard>/upArrow");
        applicationInputs.AddAction("godown", binding:"<Keyboard>/downArrow");
        applicationInputs.AddAction("goleft", binding:"<Keyboard>/leftArrow");
        applicationInputs.AddAction("goright", binding:"<Keyboard>/rightArrow");
        applicationInputs.AddAction("resetLoc", binding:"<Keyboard>/r");
        applicationInputs.AddAction("markLoc", binding:"<Keyboard>/m");


        applicationInputs["goup"].performed += NavigateUp;
        applicationInputs["godown"].performed += NavigateDown;
        applicationInputs["goleft"].performed += NavigateLeft;
        applicationInputs["goright"].performed += NavigateRight;
        applicationInputs["goup"].canceled += NavigateUp;
        applicationInputs["godown"].canceled += NavigateDown;
        applicationInputs["goleft"].canceled += NavigateLeft;
        applicationInputs["goright"].canceled += NavigateRight;
        applicationInputs["resetLoc"].performed += ResetMapCameraPos;
        applicationInputs["markLoc"].performed += MarkMapLocation;

        System_MTR.EOnBrakeModeChanged += OnBrakeStateChanged;

        ArduinoInputDatabase.GetInputFromName("Joystick X").EOnValueChanged += OnHorizontalAxis;
        ArduinoInputDatabase.GetInputFromName("Joystick Y").EOnValueChanged += OnVerticalAxis;


        //AppDatabase.LoadApp(AppID);
    }
    protected override void OnAppLoaded()
    {
        UIManager.AddToViewport(canvas, 50);
        mapCamera.enableRendering = true;
        RoverOperatingSystem.SetUserControl(true);
        m_cursorConnectedToRover = true;
    }

    protected override void OnAppQuit()
    {   
        UIManager.RemoveFromViewport(canvas);
        mapCamera.enableRendering = false;
    }

    void OnVerticalAxis(float value, int pin)
    {
        m_verticalAxis = (value - GameSettings.VERTICAL_CENTER_VAL) / GameSettings.VERTICAL_CENTER_VAL;

        if(Mathf.Abs(m_verticalAxis) < GameSettings.JOYSTICK_DEADZONE)
            m_verticalAxis = 0;

        m_currentMoveDir = new Vector2(m_horizontalAxis, m_verticalAxis);
    }

    void OnHorizontalAxis(float value, int pin)
    {
        m_horizontalAxis = (value - GameSettings.HORIZONTAL_CENTER_VAL)/GameSettings.HORIZONTAL_CENTER_VAL;

        if(Mathf.Abs(m_horizontalAxis) < GameSettings.JOYSTICK_DEADZONE)
            m_horizontalAxis = 0;

        m_currentMoveDir = new Vector2(m_horizontalAxis, m_verticalAxis);
    }

    void NavigateUp(InputAction.CallbackContext context)
    {
        m_cursorConnectedToRover = false;
        ShowBrakeWarningMessage();

        if(context.performed)
            m_currentMoveDir.x = 1f;
        else if (context.canceled)
            m_currentMoveDir.x = 0f;
    }

    void NavigateDown(InputAction.CallbackContext context)
    {
        m_cursorConnectedToRover = false;
        ShowBrakeWarningMessage();

        if(context.performed)
            m_currentMoveDir.x = -1f;
        else if (context.canceled)
            m_currentMoveDir.x = 0f;
    }

    void NavigateLeft(InputAction.CallbackContext context)
    {
        m_cursorConnectedToRover = false;
        ShowBrakeWarningMessage();

        if(context.performed)
            m_currentMoveDir.y = -1f;
        else if (context.canceled)
            m_currentMoveDir.y = 0f;
    }

    void NavigateRight(InputAction.CallbackContext context)
    {
        m_cursorConnectedToRover = false;
        ShowBrakeWarningMessage();

        if(context.performed)
            m_currentMoveDir.y = 1f;
        else if (context.canceled)
            m_currentMoveDir.y = 0f;
    }

    void ShowBrakeWarningMessage()
    {
        if(System_MTR.IsBrakeActive)
            return;

        m_brakeWarningBox = UIManager.ShowMessageBox("WARNING: Brake disabled", Color.red, -1f);
    }

    void OnBrakeStateChanged(bool newState)
    {
        if(newState && m_brakeWarningBox != null)
        {
            m_brakeWarningBox.HideMessageBox();
            m_brakeWarningBox = null;
            return;
        }

        if(m_cursorConnectedToRover)
            return;

        ShowBrakeWarningMessage();
            
    }

    void ResetMapCameraPos(InputAction.CallbackContext context)
    {
        mapCamera.transform.position = new Vector3(System_GPS.WorldSpacePos.x, mapCamera.transform.position.y, System_GPS.WorldSpacePos.z);
        m_cursorConnectedToRover = true;
    }

    void MarkMapLocation(InputAction.CallbackContext context)
    {
        GameObject location = Instantiate(mapMarkerTransformEntry, mapMarkerTransform);
        location.GetComponent<TextMeshProUGUI>().text = System_GPS.GPSCoordsToString(System_GPS.WorldPosToGPSCoords(mapCamera.transform.position));
    }

    void Update()
    {
        if(!AppIsLoaded)
            return;

        if(m_cursorConnectedToRover)
            mapCamera.transform.position = new Vector3(System_GPS.WorldSpacePos.x, mapCamera.transform.position.y, System_GPS.WorldSpacePos.z);
        else
            mapCamera.transform.position += new Vector3(m_currentMoveDir.y, 0, m_currentMoveDir.x) * cameraMoveSpeed * Time.deltaTime;

        RaycastHit hit;

        if(Physics.Raycast(mapCamera.transform.position, Vector3.down, out hit, 1000f, LayerMask.GetMask(new string[] {"Terrain"})))
        {
            cursorElevationText.text = Mathf.FloorToInt(System_GPS.ElevationAtWorldPos(hit.point)).ToString() + "m";
        }

        m_cursorGPSCoords = System_GPS.WorldPosToGPSCoords(mapCamera.transform.position);

        cursorGPSCoordText.text = System_GPS.GPSCoordsToString(m_cursorGPSCoords);

        roverGPSCoordText.text = System_GPS.GPSCoordsToString(System_GPS.GPSCoordinates);
        roverHeadingText.text = System_GPS.Heading.ToString("000.0");
        roverElevationText.text = System_GPS.Elevation.ToString("00.0") + "m";
    }
}
