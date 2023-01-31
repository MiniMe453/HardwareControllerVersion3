using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.InputSystem;
using System;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        ArduinoInputDatabase.InitializeDatabase();
        
        // QualitySettings.vSyncCount = 0;  // VSync must be disabled
        // Application.targetFrameRate = 30;

        RoverOperatingSystem.InitOS();
    }

}
