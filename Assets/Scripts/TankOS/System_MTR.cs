using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using MilkShake;
using System;
using UnityTimer;
using Rover.Settings;

public class System_MTR : MonoBehaviour
{
    Rigidbody m_rigidbody => GetComponent<Rigidbody>();
    float m_horizontalAxis;
    float m_throttleAxis;
    static bool m_brakeActive = true;
    public static bool IsBrakeActive {get {return m_brakeActive;}}
    public static event Action<bool> EOnBrakeModeChanged;
    int m_brakeLEDpin;

    public float turnSpeed;
    public float speed;
    public float maxSpeed = 1f;

//Public variables for other scripts
    public static float RoverVelocity;
    public static event Action<float> EOnRoverVelocityUpdate;

    public Shaker mainShaker;
    ShakeInstance m_offroadShakeInstance;
    public ShakePreset offRoadShakePreset;
    float m_shakeStrength = 0.15f;
    public GameObject navCamera;


    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;

        Timer.Register(0.25f, () => OnRoverVelocityUpdate());
    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("Joystick X").EOnValueChanged += OnHorizontalAxis;
        ArduinoInputDatabase.GetInputFromName("Push Potentiometer").EOnValueChanged += OnThrottleAxis;
        ArduinoInputDatabase.GetInputFromName("Brake Switch").EOnButtonPressed += OnBrakeSwitchPressed;
        ArduinoInputDatabase.GetInputFromName("Brake Switch").EOnButtonReleased += OnBrakeSwitchReleased;
        RoverOperatingSystem.EOnRoverControlModeChanged += OnRoverControlModeChanged;
        RoverOperatingSystem.EOnOSModeChanged += OnOSModeChanged;

        m_brakeLEDpin = ArduinoInputDatabase.GetOutputIndexFromName("brake light led");
    }

    void OnHorizontalAxis(float value, int pin)
    {
        m_horizontalAxis = (value - GameSettings.HORIZONTAL_CENTER_VAL)/GameSettings.HORIZONTAL_CENTER_VAL;

        if(Mathf.Abs(m_horizontalAxis) < GameSettings.JOYSTICK_DEADZONE)
            m_horizontalAxis = 0;
    }

    void OnThrottleAxis(float value, int pin)
    {
        m_throttleAxis = (value - 512)/512;

        if(Mathf.Abs(m_throttleAxis) < GameSettings.JOYSTICK_DEADZONE)
            m_throttleAxis = 0;
    }

    void OnBrakeSwitchPressed(int pin)
    {
        m_brakeActive = true;
        LEDManager.SetLEDMode(m_brakeLEDpin, 1);

        EOnBrakeModeChanged?.Invoke(m_brakeActive);
    }

    void OnBrakeSwitchReleased(int pin)
    {
        m_brakeActive = false;
        LEDManager.SetLEDMode(m_brakeLEDpin, 0);

        EOnBrakeModeChanged?.Invoke(m_brakeActive);
    }

    void OnRoverControlModeChanged(RoverControlMode newMode)
    {
        switch(newMode)
        {
            case RoverControlMode.CAM:
                navCamera.SetActive(false);
                break;
            case RoverControlMode.RVR:
                navCamera.SetActive(true);
                break;
        }
    }

    void OnOSModeChanged(OSMode newMode)
    {
        if(newMode == OSMode.Rover)
            RoverOperatingSystem.SetUserControl(true);
    }

    void Update()
    {
        if(RoverOperatingSystem.OSMode != OSMode.Rover)
            return;

        Quaternion wantedRotation = transform.rotation * Quaternion.Euler(Vector3.up * turnSpeed * m_horizontalAxis);
            m_rigidbody.MoveRotation(wantedRotation);
        
        if(RoverOperatingSystem.RoverControlMode != RoverControlMode.RVR)
            return;

        Vector3 wantedPosition = transform.position + (transform.forward * maxSpeed * m_throttleAxis * (m_brakeActive? 0f : 1f));
        m_rigidbody.MovePosition(wantedPosition);

        // if(m_offroadShakeInstance == null)
        // {
        //     m_offroadShakeInstance = mainShaker.Shake(offRoadShakePreset);
        //     return;
        // }

        // m_offroadShakeInstance.StrengthScale = RoverVelocity / maxSpeed;
        // Debug.LogError(RoverVelocity / maxSpeed);
    }

    void OnRoverVelocityUpdate()
    {
        RoverVelocity = maxSpeed * m_throttleAxis * (m_brakeActive? 0f : 1f);
        EOnRoverVelocityUpdate?.Invoke(RoverVelocity);
    }
}
