using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.UI;
using Rover.OS;

public class MapModeManager : MonoBehaviour
{

    public MonoBehaviourApplication navInterface;

    void OnEnable()
    {
        RoverOperatingSystem.EOnOSModeChanged += OnNewOSMode;
    }

    void OnNewOSMode(OSMode newMode)
    {
        switch(newMode)
        {
            case OSMode.Rover:
                // UIManager.RemoveFromViewport(mapModeCanvas);
                break;
            case OSMode.Computer:
                // UIManager.RemoveFromViewport(mapModeCanvas);
                break;
            case OSMode.Map:
                AppDatabase.LoadApp(navInterface.AppID);
                break;
        }
    }
}
