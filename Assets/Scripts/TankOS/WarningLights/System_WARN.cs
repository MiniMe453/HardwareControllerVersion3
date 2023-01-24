using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using Rover.Settings;
using UnityTimer;
using Rover.Interface;
using System;
public class System_WARN : MonoBehaviour
{
    private int tempWarnPin;
    private int radWarnPin;
    private int rdioWarnPin;
    private int anglWarnPin;
    private bool m_magWarningShown = false;

    //Proximity Sensor Variables
    private int[] m_ledPins = new int[4];
    private int[] m_ledPinStates = { 0, 0, 0, 0 };
    private bool m_stateModified = false;
    private int m_prevQuadrant = 0;
    private Timer m_proximityTimer;
    private List<GameObject> objectsInRange = new List<GameObject>();
    private MessageBox m_angleMessageBox;
    private MessageBox m_currentMessageBox;
    public static event Action<int> EOnProximitySensorModified;
    public static event Action<bool> EOnRadiationWarningLight;
    public static event Action<bool> EOnTemperatureWarningLight;
    public static event Action<bool> EOnMagWarningLight;
    public static event Action<bool> EOnAngleWarningLight;

    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
        
        m_proximityTimer = Timer.Register(GameSettings.PROXIMITY_CHECK_DELAY, () => CheckRoverProximity(), isLooped: true);
    }


    /**
    Temp Warning Light
    Radio Warning Light
    Angle Warning Light
    Proximity Sensor Back
    Proximity Sensor Left
    Radiation Warning Light
    Proximity Sensor front LED
    ProximitySensor right LED
    **/
    void OnDatabaseInit()
    {
        tempWarnPin = ArduinoInputDatabase.GetOutputIndexFromName("Temp Warning Light");
        radWarnPin = ArduinoInputDatabase.GetOutputIndexFromName("Radiation Warning Light");
        rdioWarnPin = ArduinoInputDatabase.GetOutputIndexFromName("Radio Warning Light");
        anglWarnPin = ArduinoInputDatabase.GetOutputIndexFromName("Angle Warning Light");

        m_ledPins[0] = ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor front LED");
        m_ledPins[3] = ArduinoInputDatabase.GetOutputIndexFromName("ProximitySensor right LED");
        m_ledPins[2] = ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor Back");
        m_ledPins[1] = ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor Left");

        m_proximityTimer = Timer.Register(GameSettings.PROXIMITY_CHECK_DELAY, () => CheckRoverProximity(), isLooped: true);
    }

    void Update()
    {
        if(!Uduino.UduinoManager.Instance.isConnected())
            return;

        CheckAngle();
        CheckMagnetic();
        CheckRadiation();
        CheckTemperature();
    }

    void CheckMagnetic()
    {
        float magVal = MagneticSim.ReadMagneticFromLocation(transform.position);

        if(magVal > GameSettings.MAG_MAX_VALUE && !m_magWarningShown)
        {
            m_magWarningShown = true;

            if(m_currentMessageBox)
            {
                m_currentMessageBox.HideMessageBox();
                m_currentMessageBox = null;    
            }

            m_currentMessageBox = UIManager.ShowMessageBox("WRNG: HIGH MAGNETIC FIELDS", Color.red, 2f);
            EOnMagWarningLight?.Invoke(true);
        }
        else if (magVal < GameSettings.MAG_MAX_VALUE && m_magWarningShown)
        {
            m_magWarningShown = false;
            EOnMagWarningLight?.Invoke(false);
        }
    }

    void CheckTemperature()
    {
        float tempVal = TemperatureSim.ReadTemperatureFromLocation(transform.position);

        if(tempVal > GameSettings.TEMP_MAX_VALUE && !LEDManager.GetLEDState(tempWarnPin))
        {
            if(!m_currentMessageBox)
            {
                m_currentMessageBox = UIManager.ShowMessageBox("WRNG: HIGH TEMPERATURE", Color.red, 2f);
            }

            LEDManager.SetLEDMode(tempWarnPin, 1);
            EOnTemperatureWarningLight?.Invoke(true);
        }
        else if (tempVal < GameSettings.TEMP_MAX_VALUE && LEDManager.GetLEDState(tempWarnPin))
        {
            LEDManager.SetLEDMode(tempWarnPin, 0);
            EOnTemperatureWarningLight?.Invoke(false);
        }
        
    }

    void CheckRadiation()
    {
        float radVal = RadiationSim.ReadRadiationFromLocation(transform.position);

        if(radVal > GameSettings.RAD_MAX_VALUE && !LEDManager.GetLEDState(radWarnPin))
        {
            if(!m_currentMessageBox)
            {
                m_currentMessageBox = UIManager.ShowMessageBox("WRNG: HIGH RADIATION", Color.red, 2f);
            }

            LEDManager.SetLEDMode(radWarnPin, 1);
            EOnRadiationWarningLight?.Invoke(true);
        }
        else if (radVal < GameSettings.RAD_MAX_VALUE && LEDManager.GetLEDState(radWarnPin))
        {
            LEDManager.SetLEDMode(radWarnPin, 0);
            EOnRadiationWarningLight?.Invoke(false);
        }
    }

    void CheckAngle()
    {
        if((Mathf.Abs(SystemPitchRoll.Pitch) > GameSettings.PITCH_DANGER_ZONE || Mathf.Abs(SystemPitchRoll.Roll) > GameSettings.ROLL_DANGER_ZONE) && !LEDManager.GetLEDState(anglWarnPin))
        { 
            LEDManager.SetLEDMode(anglWarnPin, 1);
            m_angleMessageBox = UIManager.ShowMessageBox("WRNG: HIGH PITCH OR ROLL", Color.red, -1f);
            EOnAngleWarningLight?.Invoke(true);    
        }
        else if((Mathf.Abs(SystemPitchRoll.Pitch) < GameSettings.PITCH_DANGER_ZONE && Mathf.Abs(SystemPitchRoll.Roll) < GameSettings.ROLL_DANGER_ZONE) && LEDManager.GetLEDState(anglWarnPin))
        {    
            LEDManager.SetLEDMode(anglWarnPin, 0);
            EOnAngleWarningLight?.Invoke(false);
        }

            if(m_angleMessageBox)
            {
                m_angleMessageBox.HideMessageBox();
            }
    }

    void CheckRoverProximity()
    {
        float angle = 0f;
        bool sensorActivated = false;

        Collider[] objectsInSphere = Physics.OverlapSphere(transform.position, GameSettings.PROXIMITY_CHECK_RADIUS, ~LayerMask.GetMask(new string[] {"Terrain", "Rover"}));

        if (objectsInSphere.Length == 0)
        {
            ResetProximitySensorPins();
            return;
        }
        
        sensorActivated = true;

        foreach (Collider obj in objectsInSphere)
        {
            angle = Vector3.SignedAngle(transform.forward, (obj.transform.position - transform.position).normalized, Vector3.up);

            if ((angle <= -135f || angle >= 135f ) && m_ledPinStates[0] != 1)
                SetLEDPinStates(0, 1);
            if (angle >= 45f && angle <= 135f && m_ledPinStates[1] != 1)
                SetLEDPinStates(1, 1);
            if (angle >= -45f && angle <= 45f && m_ledPinStates[2] != 1)
                SetLEDPinStates(2, 1);
            if (angle < -45f && angle > -135f && m_ledPinStates[3] != 1)
                SetLEDPinStates(3, 1);
        }

        if (sensorActivated && m_stateModified)
        {
            m_stateModified = false;
            LEDManager.SetLEDMode(m_ledPins, m_ledPinStates);
        }
    }

    void ResetProximitySensorPins()
    {
        if(!m_stateModified)
            return;

        m_stateModified = false;
        m_prevQuadrant = -1;
        m_ledPinStates = new int[] { 0, 0, 0, 0 };
        LEDManager.SetLEDMode(m_ledPins, m_ledPinStates); 
        EOnProximitySensorModified?.Invoke(-1);
    }

    private void SetLEDPinStates(int index, int value)
    {
        if(index != m_prevQuadrant)
        {
            for(int i = 0;i < m_ledPinStates.Length;i++)
            {
                m_ledPinStates[i] = i == index? 1 : 0;
            }

            m_prevQuadrant = index;
            m_stateModified = true;  

            EOnProximitySensorModified?.Invoke(index);
        }
    }
}
