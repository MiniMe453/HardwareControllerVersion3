using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using System;
using Rover.OS;

public enum RoverControlMode {CAM, RVR};
public enum OSMode {Rover, Computer, Map};

public static class RoverOperatingSystem
{
    private static RoverControlMode m_roverControlMode = RoverControlMode.CAM;
    public static RoverControlMode RoverControlMode {get {return m_roverControlMode;}}
    private static OSMode m_osMode = OSMode.Rover;
    public static OSMode OSMode {get{return m_osMode;}}
    public static event Action<OSMode> EOnOSModeChanged;
    public static event Action<RoverControlMode> EOnRoverControlModeChanged;
    public static event Action<bool> EOnArduinoInputStateChanged;
    private static bool m_allowUserControl = true;
    public static bool AllowUserControl {get {return m_allowUserControl;}}
    private static bool m_arduinoInputEnabled = true;
    public static bool ArduinoInputEnabled {get{return m_arduinoInputEnabled;}}
    private static int m_readyLedPin;
    private static int m_transmitLedPin;

    //LED Pins
    public static void InitOS()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnInputDatabasedInit;
    }

    private static void OnInputDatabasedInit()
    {//Transmit LED ReadyTransmit LED
        ArduinoInputDatabase.GetInputFromName("RVR Button").EOnButtonPressed += OnRVRButtonPressed;
        ArduinoInputDatabase.GetInputFromName("CAM Button").EOnButtonPressed += OnCAMButtonPressed;
        m_transmitLedPin = ArduinoInputDatabase.GetOutputIndexFromName("Transmit LED");
        m_readyLedPin = ArduinoInputDatabase.GetOutputIndexFromName("ReadyTransmit LED");
        ThreeWaySwitch.EOnCurrentSelectionChanged += OnNewControlModeSelected;

        OnNewControlModeSelected(ThreeWaySwitch.CurrentValue);
        OnRVRButtonPressed(0);

        LEDManager.SetLEDMode(new int[] {m_readyLedPin, m_transmitLedPin}, new int[] {1,0});
    }

    public static void SetRoverControlMode(RoverControlMode newMode)
    {
        if(newMode == m_roverControlMode)
            return;

        m_roverControlMode = newMode;

        EOnRoverControlModeChanged?.Invoke(newMode);
    }

    static void OnRVRButtonPressed(int pin)
    {
        SetRoverControlMode(RoverControlMode.RVR);

        int[] ledPinIndex = new int[] {ArduinoInputDatabase.GetOutputIndexFromName("rvr button led"), ArduinoInputDatabase.GetOutputIndexFromName("cam led button")};
        int[] ledPinValues = new int[] {1,0};
        LEDManager.SetLEDMode(ledPinIndex, ledPinValues);

        if(OSMode != OSMode.Rover)
            return;

        if(AppDatabase.CurrentlyLoadedApp != null)
        {
            AppDatabase.CloseApp(AppDatabase.CurrentlyLoadedApp.AppID);
        }
    }

    static void OnCAMButtonPressed(int pin)
    {
        SetRoverControlMode(RoverControlMode.CAM);

        int[] ledPinIndex = new int[] {ArduinoInputDatabase.GetOutputIndexFromName("rvr button led"), ArduinoInputDatabase.GetOutputIndexFromName("cam led button")};
        int[] ledPinValues = new int[] {0,1};
        LEDManager.SetLEDMode(ledPinIndex, ledPinValues);

        if(OSMode != OSMode.Rover)
            return;

        if(AppDatabase.CurrentlyLoadedApp != null)
        {
            AppDatabase.CloseApp(AppDatabase.CurrentlyLoadedApp.AppID);
        }
    }

    static void OnNewControlModeSelected(int newSelection)
    {
        if(m_osMode == (OSMode)newSelection)
            return;

        m_osMode = (OSMode)newSelection;

        EOnOSModeChanged?.Invoke(m_osMode);
    }

    public static void SetUserControl(bool userControl)
    {
        m_allowUserControl = userControl;
    }

    public static void SetArduinoEnabled(bool newEnabled)
    {
        m_arduinoInputEnabled = newEnabled;

        LEDManager.SetLEDMode(new int[] {m_readyLedPin, m_transmitLedPin}, newEnabled? new int[] {1,0} : new int[] {0,1});

        EOnArduinoInputStateChanged?.Invoke(newEnabled);
    }

    public static void SetOSMode(OSMode newMode)
    {
        m_osMode = newMode;
        EOnOSModeChanged?.Invoke(m_osMode);
    }
}
