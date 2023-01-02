using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.UI;

public class ComputerModeManager : MonoBehaviour
{

    public GameObject computerModeGameObject;

    void OnEnable()
    {
        RoverOperatingSystem.EOnOSModeChanged += OnNewOSMode;
    }

    void OnNewOSMode(OSMode newMode)
    {
        switch(newMode)
        {
            case OSMode.Rover:
                computerModeGameObject.SetActive(false);
                break;
            case OSMode.Computer:
                computerModeGameObject.SetActive(true);
                RoverOperatingSystem.SetUserControl(false);
                break;
            case OSMode.Map:
                computerModeGameObject.SetActive(false);
                break;
        }
    }
}
