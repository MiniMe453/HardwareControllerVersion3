using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.UI;

public class ComputerModeManager : MonoBehaviour
{

    public HomeScreen homeScreen;

    void OnEnable()
    {
        RoverOperatingSystem.EOnOSModeChanged += OnNewOSMode;
    }

    void OnNewOSMode(OSMode newMode)
    {
        switch(newMode)
        {
            case OSMode.Rover:
                homeScreen.RemoveHomeScreen();
                break;
            case OSMode.Computer:
                homeScreen.LoadHomeScreen();
                RoverOperatingSystem.SetUserControl(false);
                break;
            case OSMode.Map:
                homeScreen.RemoveHomeScreen();
                break;
        }
    }
}
