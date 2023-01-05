using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.UI;

public class MapModeManager : MonoBehaviour
{

    public Canvas mapModeCanvas;

    void OnEnable()
    {
        RoverOperatingSystem.EOnOSModeChanged += OnNewOSMode;
    }

    void OnNewOSMode(OSMode newMode)
    {
        switch(newMode)
        {
            case OSMode.Rover:
                UIManager.RemoveFromViewport(mapModeCanvas);
                break;
            case OSMode.Computer:
                UIManager.RemoveFromViewport(mapModeCanvas);
                break;
            case OSMode.Map:
                UIManager.AddToViewport(mapModeCanvas, 100);
                RoverOperatingSystem.SetUserControl(false);
                break;
        }
    }
}
