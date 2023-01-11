using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Rover.OS;
using UnityEngine.InputSystem;

public class System_Nav_Interface : MonoBehaviourApplication
{
    public RenderCameraToTexture mapCamera;
    private Vector2 m_currentMoveDir;
    public Canvas canvas;
    public float cameraMoveSpeed;
    public TextMeshProUGUI cursorElevationText;
    public TextMeshProUGUI cursorGPSCoordText;
    private Vector2 m_cursorGPSCoords;


    protected override void Init()
    {
        applicationInputs.AddAction("goup", binding:"<Keyboard>/upArrow");
        applicationInputs.AddAction("godown", binding:"<Keyboard>/downArrow");
        applicationInputs.AddAction("goleft", binding:"<Keyboard>/leftArrow");
        applicationInputs.AddAction("goright", binding:"<Keyboard>/rightArrow");

        applicationInputs["goup"].performed += NavigateUp;
        applicationInputs["godown"].performed += NavigateDown;
        applicationInputs["goleft"].performed += NavigateLeft;
        applicationInputs["goright"].performed += NavigateRight;
        applicationInputs["goup"].canceled += NavigateUp;
        applicationInputs["godown"].canceled += NavigateDown;
        applicationInputs["goleft"].canceled += NavigateLeft;
        applicationInputs["goright"].canceled += NavigateRight;


        //AppDatabase.LoadApp(AppID);
    }
    protected override void OnAppLoaded()
    {
        Debug.LogError("App loaded");
        UIManager.AddToViewport(canvas, 50);
        mapCamera.enableRendering = true;
        RoverOperatingSystem.SetUserControl(false);
    }

    protected override void OnAppQuit()
    {   
        UIManager.RemoveFromViewport(canvas);
        mapCamera.enableRendering = false;
    }

    void NavigateUp(InputAction.CallbackContext context)
    {
        if(context.performed)
            m_currentMoveDir.x = 1f;
        else if (context.canceled)
            m_currentMoveDir.x = 0f;
    }

    void NavigateDown(InputAction.CallbackContext context)
    {
        if(context.performed)
            m_currentMoveDir.x = -1f;
        else if (context.canceled)
            m_currentMoveDir.x = 0f;
    }

    void NavigateLeft(InputAction.CallbackContext context)
    {
        if(context.performed)
            m_currentMoveDir.y = -1f;
        else if (context.canceled)
            m_currentMoveDir.y = 0f;
    }

    void NavigateRight(InputAction.CallbackContext context)
    {
        if(context.performed)
            m_currentMoveDir.y = 1f;
        else if (context.canceled)
            m_currentMoveDir.y = 0f;
    }

    void Update()
    {
        if(!AppIsLoaded)
            return;

        mapCamera.transform.localPosition += new Vector3(m_currentMoveDir.y, 0, m_currentMoveDir.x) * cameraMoveSpeed * Time.deltaTime;

        RaycastHit hit;

        if(Physics.Raycast(mapCamera.transform.position, Vector3.down, out hit, 1000f))
        {
            cursorElevationText.text = Mathf.FloorToInt(hit.point.y).ToString() + "m";
        }

        m_cursorGPSCoords = System_GPS.WorldPosToGPSCoords(mapCamera.transform.position);

        cursorGPSCoordText.text = m_cursorGPSCoords.x.ToString("00.00") + ":" + m_cursorGPSCoords.y.ToString("00.00");
    }
}
