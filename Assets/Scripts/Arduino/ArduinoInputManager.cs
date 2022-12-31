using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;

public class ArduinoInputManager : MonoBehaviour
{
    int cam3LEDPin;
    bool cam3Activated = false;
    void Awake()
    {
        ArduinoInputDatabase.InitializeDatabase();
        //ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseSetup;
    }
    
    void OnDatabaseSetup()
    {
        Debug.LogError("Input manager in secen set up properly");
        ArduinoInputDatabase.GetInputFromName("RVR Button").EOnButtonPressed += OnTestButtonPressed;
        ArduinoInputDatabase.GetInputFromName("Joystick X").EOnValueChanged += OnTestAnalogValueChanged;
        cam3LEDPin = ArduinoInputDatabase.GetOutputIndexFromName("cam 3 led pin");
    }
    void OnTestAnalogValueChanged(float value, int pin)
    {
        Debug.Log(value);
    }

    void OnTestButtonPressed(int pin)
    {
        Debug.Log("Button pressed!");

        cam3Activated = !cam3Activated;
        LEDManager.SetLEDMode(cam3LEDPin, cam3Activated? 1 : 0);
    }
}
