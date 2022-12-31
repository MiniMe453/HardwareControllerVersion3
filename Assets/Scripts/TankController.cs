using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;

public class TankController : MonoBehaviour
{
    Rigidbody m_rigidbody => GetComponent<Rigidbody>();
    float m_horizontalAxis;
    float m_throttleAxis;
    bool m_brakeActive = true;
    int m_brakeLEDpin;

    public float turnSpeed;
    public float speed;
    public float maxSpeed = 1f;

    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("Joystick X").EOnValueChanged += OnHorizontalAxis;
        ArduinoInputDatabase.GetInputFromName("Push Potentiometer").EOnValueChanged += OnThrottleAxis;
        ArduinoInputDatabase.GetInputFromName("Brake Switch").EOnButtonPressed += OnBrakeSwitchFlipped;
        ArduinoInputDatabase.GetInputFromName("Brake Switch").EOnButtonReleased += OnBrakeSwitchFlipped;
        m_brakeLEDpin = ArduinoInputDatabase.GetOutputIndexFromName("brake light led");
    }

    void OnHorizontalAxis(float value, int pin)
    {
        m_horizontalAxis = (value - 508)/508;

        if(Mathf.Abs(m_horizontalAxis) < 0.05)
            m_horizontalAxis = 0;
    }

    void OnThrottleAxis(float value, int pin)
    {
        m_throttleAxis = (value - 512)/512;
        // m_rigidbody.MovePosition(transform.position + (transform.forward * maxSpeed * value));
        // Debug.LogError(m_rigidbody.velocity.magnitude + " " + value);
    }

    void OnBrakeSwitchFlipped(int pin)
    {
        m_brakeActive = !m_brakeActive;
        LEDManager.SetLEDMode(m_brakeLEDpin, m_brakeActive? 1 : 0);
    }

    void Update()
    {
        Vector3 wantedPosition = transform.position + (transform.forward * maxSpeed * m_throttleAxis * (m_brakeActive? 0f : 1f));
        m_rigidbody.MovePosition(wantedPosition);

        Quaternion wantedRotation = transform.rotation * Quaternion.Euler(Vector3.up * turnSpeed * m_horizontalAxis);
        m_rigidbody.MoveRotation(wantedRotation);

    }
}
