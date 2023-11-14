using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using MilkShake;
using System;
using UnityTimer;
using Rover.Settings;

public class System_MTR : MonoBehaviour, IInputTypes
{
    Rigidbody m_rigidbody => GetComponent<Rigidbody>();
    static float m_horizontalAxis;
    public static float HorizontalAxis { get { return m_horizontalAxis; } }
    static float m_throttleAxis;
    public static float ThrottleAxis { get { return m_throttleAxis; } }
    public static event Action<float> EOnThrottleAxisChanged;
    private float m_currentSpeed;
    private float m_accelerationSpeed = 0.5f;
    private float m_brakeSpeed = 1.5f;
    static bool m_brakeActive = false;
    public static bool IsBrakeActive { get { return m_brakeActive; } }
    public static event Action<bool> EOnBrakeModeChanged;
    int m_brakeLEDpin;

    public float turnSpeed;
    public float speed;
    public float maxSpeed = 1f;

    //Public variables for other scripts
    public static float RoverVelocity;
    private static float m_oldRoverVelocity;
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
        GameInitializer.EOnGameInitialized += OnDatabaseInit;

        m_originalTransform = transform;
    }

    void OnDatabaseInit()
    {
        if (RoverInputManager.UseKeyboardInput)
            AssignKeyboardEvents();
        else
            AssignArduinoEvents();

        RoverOperatingSystem.EOnRoverControlModeChanged += OnRoverControlModeChanged;
        RoverOperatingSystem.EOnOSModeChanged += OnOSModeChanged;

        EOnThrottleAxisChanged?.Invoke(m_throttleAxis);

        // Timer.Register(0.25f, () => OnRoverVelocityUpdate(), isLooped: true);
    }

    public void AssignKeyboardEvents()
    {
        KeyboardAxisManager.EOnXAxis += OnHorizontalAxisKeyboard;
        KeyboardAxisManager.EOnThrottleAxis += OnThrottleAxisKeyboard;
        RoverInputManager.InputActions["Brake"].performed += (x) => { OnBrakeSwitchPressed(-1); };
        OnBrakeSwitchPressed(-1);
    }

    public void AssignArduinoEvents()
    {
        m_brakeLEDpin = ArduinoInputDatabase.GetOutputIndexFromName("brake light led");

        ArduinoInputDatabase.GetInputFromName("Joystick X").EOnValueChanged += OnHorizontalAxisArduino;
        ArduinoInputDatabase.GetInputFromName("Push Potentiometer").EOnValueChanged += OnThrottleAxisArduino;
        ArduinoInputDatabase.GetInputFromName("Brake Switch").EOnButtonPressed += OnBrakeSwitchPressed;
        ArduinoInputDatabase.GetInputFromName("Brake Switch").EOnButtonReleased += OnBrakeSwitchPressed;
    }

    void OnHorizontalAxisArduino(float value, int pin)
    {
        m_horizontalAxis = (value - GameSettings.HORIZONTAL_CENTER_VAL) / GameSettings.HORIZONTAL_CENTER_VAL;

        if (Mathf.Abs(m_horizontalAxis) < GameSettings.JOYSTICK_DEADZONE)
            m_horizontalAxis = 0;
        
        m_horizontalAxis = Math.Clamp(m_horizontalAxis, -1, 1);
    }

    void OnThrottleAxisArduino(float value, int pin)
    {
        m_throttleAxis = (value - 512) / ((value - 512 > 0) ? 1024 : 512);

        if (Mathf.Abs(m_throttleAxis) < GameSettings.JOYSTICK_DEADZONE)
            m_throttleAxis = 0;
    }

    void OnHorizontalAxisKeyboard(float value)
    {
        m_horizontalAxis = value;
    }

    void OnThrottleAxisKeyboard(float value)
    {
        Debug.Log(value);
        m_throttleAxis = value;

        EOnThrottleAxisChanged?.Invoke(value);
    }

    void OnBrakeSwitchPressed(int pin)
    {
        m_brakeActive = !m_brakeActive;

        LEDManager.SetLEDMode(m_brakeLEDpin, m_brakeActive ? 1 : 0);

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
        switch (newMode)
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
        if (newMode == OSMode.Rover)
            RoverOperatingSystem.SetUserControl(true);
    }

    void FixedUpdate()
    {
        if (RoverOperatingSystem.OSMode == OSMode.Computer)
            return;

        if (RoverOperatingSystem.OSMode == OSMode.Map && (m_brakeActive || RoverVelocity == 0f))
            return;

        if (Mathf.Abs(SystemPitchRoll.Pitch) > 90 || Mathf.Abs(SystemPitchRoll.Roll) > 90)
            return;

        Quaternion wantedRotation = transform.rotation * Quaternion.Euler(Vector3.up * turnSpeed * m_horizontalAxis * Time.deltaTime);
        m_rigidbody.MoveRotation(wantedRotation);


        if(!m_brakeActive && RoverOperatingSystem.RoverControlMode == RoverControlMode.RVR)
            m_currentSpeed += m_accelerationSpeed * Time.deltaTime * m_throttleAxis;
        else
            m_currentSpeed -= m_brakeSpeed * Time.deltaTime * Mathf.Sign(m_currentSpeed);
        

        if(Mathf.Abs(m_currentSpeed) > 1f)
            m_currentSpeed = m_throttleAxis;
        else if (Mathf.Abs(m_currentSpeed) < GameSettings.JOYSTICK_DEADZONE / 2f && m_brakeActive)
            m_currentSpeed = 0f;

        Vector3 wantedPosition = transform.position + (transform.forward * maxSpeed * m_currentSpeed * Time.fixedDeltaTime);
        m_rigidbody.MovePosition(wantedPosition);

        motorSoundEffect.volume = Mathf.Clamp01(Mathf.Abs(m_currentSpeed) + Mathf.Abs(m_horizontalAxis));

        RoverVelocity = maxSpeed * (m_currentSpeed * -1f) * 2.5f;
        
        if(m_oldRoverVelocity != RoverVelocity)
        {
            m_oldRoverVelocity = RoverVelocity;
            EOnRoverVelocityUpdate?.Invoke(RoverVelocity);
        }
    }
}
