using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.UI;

public class MapModeManager : MonoBehaviour
{

    public GameObject mapModeGameObject;

    void OnEnable()
    {
        RoverOperatingSystem.EOnOSModeChanged += OnNewOSMode;
    }

    void OnNewOSMode(OSMode newMode)
    {
        switch(newMode)
        {
            case OSMode.Rover:
                mapModeGameObject.SetActive(false);
                break;
            case OSMode.Computer:
                mapModeGameObject.SetActive(false);
                break;
            case OSMode.Map:
                mapModeGameObject.SetActive(true);
                RoverOperatingSystem.SetUserControl(false);
                break;
        }
    }
}
