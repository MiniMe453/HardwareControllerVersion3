using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using System;

public enum RoverControlMode {CAM, RVR};

public static class RoverOperatingSystem
{
    public static RoverControlMode roverControlMode = RoverControlMode.RVR;
    public static event Action<RoverControlMode> EOnRoverControlModeChanged;

    //LED Pins
    public static void InitOS()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnInputDatabasedInit;
    }

    private static void OnInputDatabasedInit()
    {
        ArduinoInputDatabase.GetInputFromName("RVR Button").EOnButtonPressed += OnRVRButtonPressed;
        ArduinoInputDatabase.GetInputFromName("CAM Button").EOnButtonPressed += OnCAMButtonPressed;
    }

    public static void SetRoverControlMode(RoverControlMode newMode)
    {
        if(newMode == roverControlMode)
            return;

        roverControlMode = newMode;

        EOnRoverControlModeChanged?.Invoke(newMode);
    }

    static void OnRVRButtonPressed(int pin)
    {
        SetRoverControlMode(RoverControlMode.RVR);

        int[] ledPinIndex = new int[] {ArduinoInputDatabase.GetOutputIndexFromName("rvr button led"), ArduinoInputDatabase.GetOutputIndexFromName("cam led button")};
        int[] ledPinValues = new int[] {1,0};
        LEDManager.SetLEDMode(ledPinIndex, ledPinValues);
    }

    static void OnCAMButtonPressed(int pin)
    {
        SetRoverControlMode(RoverControlMode.CAM);

        int[] ledPinIndex = new int[] {ArduinoInputDatabase.GetOutputIndexFromName("rvr button led"), ArduinoInputDatabase.GetOutputIndexFromName("cam led button")};
        int[] ledPinValues = new int[] {0,1};
        LEDManager.SetLEDMode(ledPinIndex, ledPinValues);
    }
}
