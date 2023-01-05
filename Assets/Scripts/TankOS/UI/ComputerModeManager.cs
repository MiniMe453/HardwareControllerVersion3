using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.UI;

public class ComputerModeManager : MonoBehaviour
{

    public Canvas computerModeMainCanvas;

    void OnEnable()
    {
        RoverOperatingSystem.EOnOSModeChanged += OnNewOSMode;
    }

    void OnNewOSMode(OSMode newMode)
    {
        switch(newMode)
        {
            case OSMode.Rover:
                UIManager.RemoveFromViewport(computerModeMainCanvas);
                break;
            case OSMode.Computer:
                UIManager.AddToViewport(computerModeMainCanvas, 100);
                RoverOperatingSystem.SetUserControl(false);
                break;
            case OSMode.Map:
                UIManager.RemoveFromViewport(computerModeMainCanvas);
                break;
        }
    }
}
