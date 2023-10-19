using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Rover.Arduino;
using Rover.Settings;
public class ResetJoystickHorizCenterValue : MonoBehaviour
{
    InputActionMap inputActions = new InputActionMap();
    // Start is called before the first frame update

    float currentJoystickReading = 0;
    void Start()
    {
        inputActions.AddAction("ResetJoystick", binding: "<Keyboard>/r");
        inputActions["ResetJoystick"].performed += OnKeyboardRPressed;
        inputActions.Enable();

        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;

    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("Joystick X").EOnValueChanged += OnHorizontalAxis;
        Debug.Log("Event was registerd");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnKeyboardRPressed(InputAction.CallbackContext context)
    {
        GameSettings.HORIZONTAL_CENTER_VAL = (int)currentJoystickReading;
    }

    void OnHorizontalAxis(float value, int pin)
    {
        currentJoystickReading = value;
    }
}
