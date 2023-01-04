using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        ArduinoInputDatabase.InitializeDatabase();
        
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 30;

        RoverOperatingSystem.InitOS();
    }
}
