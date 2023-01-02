using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using System;
using TMPro;
using UnityTimer;
using Uduino;

public class InputTester : MonoBehaviour
{
    int currentRadioLight;
    int station1LightPin;
    int station2LightPin;
    int station3LightPin;
    int rotaryCountInt;
    public TextMeshProUGUI rotaryCounter;
    public TextMeshProUGUI currentRadioLightText;
    public TextMeshProUGUI LIDARButton;
    public TextMeshProUGUI TakePhotoButton;
    public TextMeshProUGUI PushPotentiometer;
    public TextMeshProUGUI JoystickX;
    public TextMeshProUGUI JoystickY;
    public TextMeshProUGUI printStopButton;
    public TextMeshProUGUI threeWaySwitchText;
    int cam1ledPin;
    int cam2ledPin;
    int cam3ledPin;
    int cam4ledPin;
    int brakeSwitchPin;
    int RVRModeLedPin;
    int CAMModeLedPin;
    int LCDCounter;
    int PrintStartLedPin;
    int[] warningLightIndexes;
    int warningLightIndex = 0;
    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("Rotary Encoder Button").EOnButtonPressed += OnRadioSwitchPressed;
        ArduinoInputDatabase.GetInputFromName("Rotary Encoder Counter").EOnValueChanged += OnRotaryEncoderUpdate;
        ArduinoInputDatabase.GetInputFromName("CAM 1 Button").EOnButtonPressed += OnCam1Pressed;
        ArduinoInputDatabase.GetInputFromName("CAM 2 Button").EOnButtonPressed += OnCam2Pressed;
        ArduinoInputDatabase.GetInputFromName("CAM 3 Button").EOnButtonPressed += OnCam3Pressed;
        ArduinoInputDatabase.GetInputFromName("CAM 4 Button").EOnButtonPressed += OnCam4Pressed;
        ArduinoInputDatabase.GetInputFromName("CAM Take Photo Button").EOnButtonPressed += TakePhotoButtonPressed;
        ArduinoInputDatabase.GetInputFromName("CAM Take Photo Button").EOnButtonReleased += TakePhotoButtonRelease;
        ArduinoInputDatabase.GetInputFromName("LIDAR Button").EOnButtonPressed += LIDARButtonPressed;
        ArduinoInputDatabase.GetInputFromName("LIDAR Button").EOnButtonReleased += LIDARButtonReleased;
        ArduinoInputDatabase.GetInputFromName("Push Potentiometer").EOnValueChanged += PushPotienometerChanged;
        ArduinoInputDatabase.GetInputFromName("Joystick X").EOnValueChanged += JoystickXChanged;
        ArduinoInputDatabase.GetInputFromName("Joystick Y").EOnValueChanged += JoystickYChanged;
        ArduinoInputDatabase.GetInputFromName("Brake Switch").EOnButtonPressed += BrakeSwitchOn;
        ArduinoInputDatabase.GetInputFromName("Brake Switch").EOnButtonReleased += BrakeSwitchOff;
        ArduinoInputDatabase.GetInputFromName("RVR Button").EOnButtonPressed += OnRVRButtonPressed;
        ArduinoInputDatabase.GetInputFromName("CAM Button").EOnButtonPressed += OnCamButtonPressed;
        ArduinoInputDatabase.GetInputFromName("Print Button 01").EOnButtonPressed += OnPrintStartButtonPressed;
        ArduinoInputDatabase.GetInputFromName("Print Button 02").EOnButtonPressed += OnPrintStopButtonPressed;
        ArduinoInputDatabase.GetInputFromName("Print Button 02").EOnButtonReleased += OnPrintStopButtonReleased;
        ThreeWaySwitch.EOnCurrentSelectionChanged += OnThreeWaySwitchChanged;

        station1LightPin = ArduinoInputDatabase.GetOutputIndexFromName("Radio Station LED 1");
        station2LightPin = ArduinoInputDatabase.GetOutputIndexFromName("radio station 2 led");
        station3LightPin = ArduinoInputDatabase.GetOutputIndexFromName("radio station 3 led");
        cam1ledPin = ArduinoInputDatabase.GetOutputIndexFromName("CAM 1 led pin");
        cam2ledPin = ArduinoInputDatabase.GetOutputIndexFromName("cam 2 led pin");
        cam3ledPin = ArduinoInputDatabase.GetOutputIndexFromName("cam 3 led pin");
        cam4ledPin = ArduinoInputDatabase.GetOutputIndexFromName("cam 4 led pin");
        brakeSwitchPin = ArduinoInputDatabase.GetOutputIndexFromName("brake light led");
        RVRModeLedPin = ArduinoInputDatabase.GetOutputIndexFromName("rvr button led");
        CAMModeLedPin = ArduinoInputDatabase.GetOutputIndexFromName("cam led button");
        PrintStartLedPin = ArduinoInputDatabase.GetOutputIndexFromName("print led button");

        warningLightIndexes = new int[]
        {
            ArduinoInputDatabase.GetOutputIndexFromName("Temp Warning Light"),
            ArduinoInputDatabase.GetOutputIndexFromName("Radio Warning Light"),
            ArduinoInputDatabase.GetOutputIndexFromName("Angle Warning Light"),
            ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor Back"),
            ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor Left"),
            ArduinoInputDatabase.GetOutputIndexFromName("Radiation Warning Light"),
            ArduinoInputDatabase.GetOutputIndexFromName("ProximitySensor right LED"),
            ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor front LED"),
            ArduinoInputDatabase.GetOutputIndexFromName("ReadyTransmit LED"),
            ArduinoInputDatabase.GetOutputIndexFromName("Transmit LED")
        };

        Timer.Register(1f, () => SendTestLCDCommand(), isLooped: true);
        Timer.Register(0.9f, () => SendTestWriteTM(), isLooped: true);
        Timer.Register(0.8f, () => CycleWarningLights(), isLooped: true);

//        Debug.LogError(PrintStartLedPin);
    }

    void OnThreeWaySwitchChanged(int newSelection)
    {
        string text = "";

        switch(newSelection)
        {
            case 0:
                text = $"{newSelection}: MODE 1";
                break;
            case 1:
                text = $"{newSelection}: MODE 2";
                break;
            case 2:
                text = $"{newSelection}: MODE 3";
                break;
        }

        threeWaySwitchText.text = text;
    }

    void CycleWarningLights()
    {
        warningLightIndex++;

        if(warningLightIndex == warningLightIndexes.Length)
            warningLightIndex = 0;

        int[] values = new int[warningLightIndexes.Length];
        values[warningLightIndex] = 1;

        values[warningLightIndexes.Length - 1] = warningLightIndex % 2 == 0? 0 : 1;
        values[warningLightIndexes.Length - 2] = warningLightIndex % 2 == 0? 1 : 0;

        LEDManager.SetLEDMode(warningLightIndexes, values);
    }

    void SendTestWriteTM()
    {
        object[] displayData = new object[5];

        if(LCDCounter % 2 == 0)
        {
            displayData[4] = "0";
        }
        else 
        {
            displayData[4] = "1";
        }

        for(int i = 0; i < 4; i++)
        {
            displayData[i] = (4096 - LCDCounter).ToString("0000")[i];
        }

        UduinoManager.Instance.sendCommand("writeTM2", displayData);
    }

    void OnPrintStartButtonPressed(int pin)
    {
        Debug.LogError("button pressed");
        ToggleLEDState(PrintStartLedPin);
    }

    void OnPrintStopButtonPressed(int pin)
    {
        // ToggleLEDState(PrintStopLedPin);
        printStopButton.text = "PRESSED";
    }

    void OnPrintStopButtonReleased(int pin)
    {
        printStopButton.text = "RELEASED";
    }

    void SendTestLCDCommand()
    {
        object[] lcdData = new object[2];

        lcdData[0] = System.DateTime.Now.ToLongTimeString().Split(' ')[0];
        LCDCounter++;

        if(LCDCounter > 4096)
            LCDCounter = 0;

        lcdData[1] = LCDCounter.ToString("0000");

        UduinoManager.Instance.sendCommand("lcd", lcdData);
    }

    void OnRadioSwitchPressed(int pin)
    {
        currentRadioLight++;

        if(currentRadioLight == 3)
            currentRadioLight = 0;

        LEDManager.SetLEDMode(new int[] {station1LightPin, station2LightPin, station3LightPin}, new int[] {
            Convert.ToInt32(currentRadioLight == 0), Convert.ToInt32(currentRadioLight == 1), Convert.ToInt32(currentRadioLight == 2)
        });

        currentRadioLightText.text = currentRadioLight.ToString();
    }

    void OnRotaryEncoderUpdate(float value, int pin)
    {
        rotaryCounter.text = value.ToString("000");

        rotaryCountInt = Mathf.FloorToInt(value);

        object[] displayData = new object[5];

        displayData[4] = "0";

        for(int i = 0; i< 4; i++)
        {
            displayData[i] = rotaryCountInt.ToString("0000")[i];
        }

        UduinoManager.Instance.sendCommand("writeTM1", displayData);
    }

    void OnCam1Pressed(int pin)
    {
        Debug.LogError("Cam 1 button pressed " + cam1ledPin.ToString());
        ToggleLEDState(cam1ledPin);
    }

    void OnCam2Pressed(int pin)
    {
        ToggleLEDState(cam2ledPin);
    }
    
    void OnCam3Pressed(int pin)
    {
        ToggleLEDState(cam3ledPin);
    }
    
    void OnCam4Pressed(int pin)
    {
        ToggleLEDState(cam4ledPin);
    }

    void ToggleLEDState(int ledPin)
    {
        if(LEDManager.GetLEDState(ledPin))
        {
            LEDManager.SetLEDMode(ledPin, 0);
        }
        else
        {
            LEDManager.SetLEDMode(ledPin, 1);
        }
    }

    void LIDARButtonPressed(int pin)
    {
        LIDARButton.text = "PRESSED";
    }

    void LIDARButtonReleased(int pin)
    {
        LIDARButton.text = "RELEASED";
    }

    void TakePhotoButtonPressed(int pin)
    {
        TakePhotoButton.text = "PRESSED";
    }

    void TakePhotoButtonRelease(int pin)
    {
        TakePhotoButton.text = "RELEASED";  
    }

    void PushPotienometerChanged(float value, int pin)
    {
        PushPotentiometer.text = value.ToString("0000");
    }

    void JoystickXChanged(float value, int pin)
    {
        JoystickX.text = value.ToString("0000");
    }

    void JoystickYChanged(float value, int pin)
    {
        JoystickY.text = value.ToString("0000");
    }

    void BrakeSwitchOn(int pin)
    {
        LEDManager.SetLEDMode(brakeSwitchPin, 1);
    }

    void BrakeSwitchOff(int pin)
    {
        LEDManager.SetLEDMode(brakeSwitchPin, 0);
    }

    void OnRVRButtonPressed(int pin)
    {
        ToggleLEDState(RVRModeLedPin);
    }

    void OnCamButtonPressed(int pin)
    {
        ToggleLEDState(CAMModeLedPin);
    }
}
