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
    static float m_horizontalAxis;
    public static float HorizontalAxis {get {return m_horizontalAxis;}}
    static float m_throttleAxis;
    public static float ThrottleAxis {get {return m_throttleAxis;}}
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
    public AudioSource motorSoundEffect;

    private Transform m_originalTransform;


    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;

        m_originalTransform = transform;
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
        Timer.Register(0.25f, () => OnRoverVelocityUpdate(), isLooped: true);
    }

    void OnHorizontalAxis(float value, int pin)
    {
        m_horizontalAxis = (value - GameSettings.HORIZONTAL_CENTER_VAL)/GameSettings.HORIZONTAL_CENTER_VAL;

        if(Mathf.Abs(m_horizontalAxis) < GameSettings.JOYSTICK_DEADZONE)
            m_horizontalAxis = 0;
    }

    void OnThrottleAxis(float value, int pin)
    {
        m_throttleAxis = (value - 512)/((value - 512 > 0)? 1024 : 512);

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
        if(RoverOperatingSystem.OSMode == OSMode.Computer)
            return;

        if(RoverOperatingSystem.OSMode == OSMode.Map && (m_brakeActive || RoverVelocity == 0f))
            return;

        Quaternion wantedRotation = transform.rotation * Quaternion.Euler(Vector3.up * turnSpeed * m_horizontalAxis * Time.deltaTime);
            m_rigidbody.MoveRotation(wantedRotation);
        
        if(RoverOperatingSystem.RoverControlMode != RoverControlMode.RVR)
            return;

        Vector3 wantedPosition = transform.position + (transform.forward * maxSpeed * m_throttleAxis * (m_brakeActive? 0f : 1f) * Time.deltaTime);
        m_rigidbody.MovePosition(wantedPosition);

        motorSoundEffect.volume =  Mathf.Clamp01((Mathf.Abs(m_throttleAxis) + Mathf.Abs(m_horizontalAxis)) * (m_brakeActive? 0f: 1f));

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
        RoverVelocity = maxSpeed * (m_throttleAxis * -1f) * (m_brakeActive? 0f : 1f) * 10f;
        EOnRoverVelocityUpdate?.Invoke(RoverVelocity);
    }
}
