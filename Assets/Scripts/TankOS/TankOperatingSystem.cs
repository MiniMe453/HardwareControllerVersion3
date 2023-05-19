using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using System;
using Rover.OS;

public enum RoverControlMode { CAM, RVR };
public enum OSMode { Rover, Computer, Map };

public static class RoverOperatingSystem
{
    private static RoverControlMode m_roverControlMode = RoverControlMode.CAM;
    public static RoverControlMode RoverControlMode { get { return m_roverControlMode; } }
    private static OSMode m_osMode = OSMode.Rover;
    public static OSMode OSMode { get { return m_osMode; } }
    public static event Action<OSMode> EOnOSModeChanged;
    public static event Action<RoverControlMode> EOnRoverControlModeChanged;
    public static event Action<bool> EOnArduinoInputStateChanged;
    private static bool m_allowUserControl = true;
    public static bool AllowUserControl { get { return m_allowUserControl; } }
    private static bool m_arduinoInputEnabled = true;
    public static bool ArduinoInputEnabled { get { return m_arduinoInputEnabled; } }
    private static int m_readyLedPin;
    private static int m_transmitLedPin;

    //LED Pins
    public static void InitOS()
    {
        GameInitializer.EOnGameInitialized += OnInputDatabasedInit;
    }

    private static void OnInputDatabasedInit()
    {

        if (InputTypeManager.UseKeyboardInput)
            AssignKeyboardInputs();
        else
            AssignArduinoInputs();

        OnNewControlModeSelected(ThreeWaySwitch.CurrentValue);
        OnRVRButtonPressed(0);


    }

    static void AssignArduinoInputs()
    {
        ArduinoInputDatabase.GetInputFromName("RVR Button").EOnButtonPressed += OnRVRButtonPressed;
        ArduinoInputDatabase.GetInputFromName("CAM Button").EOnButtonPressed += OnCAMButtonPressed;
        m_transmitLedPin = ArduinoInputDatabase.GetOutputIndexFromName("Transmit LED");
        m_readyLedPin = ArduinoInputDatabase.GetOutputIndexFromName("ReadyTransmit LED");
        ThreeWaySwitch.EOnCurrentSelectionChanged += OnNewControlModeSelected;
        LEDManager.SetLEDMode(new int[] { m_readyLedPin, m_transmitLedPin }, new int[] { 1, 0 });
    }

    static void AssignKeyboardInputs()
    {
        InputTypeManager.InputActions["CtrlRvrMode"].performed += (context) => { OnNewControlModeSelected(0); };
        InputTypeManager.InputActions["CtrlCmpMode"].performed += (context) => { OnNewControlModeSelected(1); };
        InputTypeManager.InputActions["CtrlMapMode"].performed += (context) => { OnNewControlModeSelected(2); };
        InputTypeManager.InputActions["RVRMode"].performed += (context) => { SetRoverControlMode(RoverControlMode.RVR); };
        InputTypeManager.InputActions["CAMMode"].performed += (context) => { SetRoverControlMode(RoverControlMode.CAM); };
    }

    public static void SetRoverControlMode(RoverControlMode newMode)
    {
        if (newMode == m_roverControlMode)
            return;

        m_roverControlMode = newMode;

        EOnRoverControlModeChanged?.Invoke(newMode);

        if (OSMode != OSMode.Rover)
            return;

        if (AppDatabase.CurrentlyLoadedApp != null)
        {
            AppDatabase.CloseApp(AppDatabase.CurrentlyLoadedApp.AppID);
        }
    }

    static void OnRVRButtonPressed(int pin)
    {
        SetRoverControlMode(RoverControlMode.RVR);

        int[] ledPinIndex = new int[] { ArduinoInputDatabase.GetOutputIndexFromName("rvr button led"), ArduinoInputDatabase.GetOutputIndexFromName("cam led button") };
        int[] ledPinValues = new int[] { 1, 0 };
        LEDManager.SetLEDMode(ledPinIndex, ledPinValues);
    }

    static void OnCAMButtonPressed(int pin)
    {
        SetRoverControlMode(RoverControlMode.CAM);

        int[] ledPinIndex = new int[] { ArduinoInputDatabase.GetOutputIndexFromName("rvr button led"), ArduinoInputDatabase.GetOutputIndexFromName("cam led button") };
        int[] ledPinValues = new int[] { 0, 1 };
        LEDManager.SetLEDMode(ledPinIndex, ledPinValues);
    }

    static void OnNewControlModeSelected(int newSelection)
    {
        if (m_osMode == (OSMode)newSelection)
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

        LEDManager.SetLEDMode(new int[] { m_readyLedPin, m_transmitLedPin }, newEnabled ? new int[] { 1, 0 } : new int[] { 0, 1 });

        EOnArduinoInputStateChanged?.Invoke(newEnabled);
    }

    public static void SetOSMode(OSMode newMode)
    {
        m_osMode = newMode;
        EOnOSModeChanged?.Invoke(m_osMode);
    }
}
