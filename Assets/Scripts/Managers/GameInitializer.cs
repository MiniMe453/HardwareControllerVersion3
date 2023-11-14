using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.InputSystem;
using System;

public class GameInitializer : MonoBehaviour
{
    public static Action EOnGameInitialized;

    public void InitializeGame(bool usingKeyboard)
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.SetResolution(800, 600, FullScreenMode.FullScreenWindow);

        RoverOperatingSystem.InitOS();

        if (!usingKeyboard)
        {
            ArduinoInputDatabase.InitializeDatabase();
            return;
        }

        EOnGameInitialized?.Invoke();

        RoverInputManager.InputActions.Enable();
        // QualitySettings.vSyncCount = 0;  // VSync must be disabled
        // Application.targetFrameRate = 60;
    }

}
