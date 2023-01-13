using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using Rover.Settings;
using UnityTimer;

public class System_WARN : MonoBehaviour
{
    private int tempWarnPin;
    private int radWarnPin;
    private int rdioWarnPin;
    private int anglWarnPin;

    //Proximity Sensor Variables
    private int[] m_ledPins = new int[4];
    private int[] m_ledPinStates = { 0, 0, 0, 0 };
    private bool m_stateModified = false;
    private int m_prevQuadrant = 0;
    private Timer m_proximityTimer;
    private List<GameObject> objectsInRange = new List<GameObject>();

    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
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
        radWarnPin = ArduinoInputDatabase.GetOutputIndexFromName("Radio Warning Light");
        rdioWarnPin = ArduinoInputDatabase.GetOutputIndexFromName("Radio Warning Light");
        anglWarnPin = ArduinoInputDatabase.GetOutputIndexFromName("Angle Warning Light");

        m_ledPins[0] = ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor front LED");
        m_ledPins[1] = ArduinoInputDatabase.GetOutputIndexFromName("ProximitySensor right LED");
        m_ledPins[2] = ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor Back");
        m_ledPins[3] = ArduinoInputDatabase.GetOutputIndexFromName("Proximity Sensor Left");

        m_proximityTimer = Timer.Register(GameSettings.PROXIMITY_CHECK_DELAY, () => CheckRoverProximity(), isLooped: true);
    }

    void Update()
    {
        CheckAngle();
    }

    void CheckAngle()
    {
        if((Mathf.Abs(SystemPitchRoll.Pitch) > GameSettings.PITCH_DANGER_ZONE || Mathf.Abs(SystemPitchRoll.Roll) > GameSettings.ROLL_DANGER_ZONE) && !LEDManager.GetLEDState(anglWarnPin))
            LEDManager.SetLEDMode(anglWarnPin, 1);
        else if((Mathf.Abs(SystemPitchRoll.Pitch) < GameSettings.PITCH_DANGER_ZONE || Mathf.Abs(SystemPitchRoll.Roll) < GameSettings.ROLL_DANGER_ZONE) && LEDManager.GetLEDState(anglWarnPin))
            LEDManager.SetLEDMode(anglWarnPin, 0);
    }

    void CheckRoverProximity()
    {
        float angle = 0f;
        bool sensorActivated = false;

        Collider[] objectsInSphere = Physics.OverlapSphere(transform.position, GameSettings.PROXIMITY_CHECK_DELAY);

        if (objectsInSphere.Length == 0)
        return;
        
        sensorActivated = true;

        foreach (Collider obj in objectsInSphere)
        {
            Vector2 playerPos = new Vector2(transform.forward.x, transform.forward.z);
            Vector2 objPos = new Vector2(obj.transform.position.x, obj.transform.position.z);

            float angle_a = Mathf.Atan2(playerPos.y, playerPos.x);
            float angle_b = Mathf.Atan2(objPos.y, objPos.x);

            angle = angle_b - angle_a;

            if (angle < Mathf.PI / 4 && angle > -Mathf.PI / 4 && m_ledPinStates[0] != 1)
                SetLEDPinStates(0, 1);
            if (angle > Mathf.PI / 4 && angle < (Mathf.PI / 4) * 3 && m_ledPinStates[1] != 1)
                SetLEDPinStates(1, 1);
            if (angle > (Mathf.PI / 4) * 3 || angle < (-Mathf.PI / 4) * 3 && m_ledPinStates[2] != 1)
                SetLEDPinStates(2, 1);
            if (angle > (-Mathf.PI / 4) * 3 && angle < -Mathf.PI / 4 && m_ledPinStates[3] != 1)
                SetLEDPinStates(3, 1);
        }

        if (sensorActivated && m_stateModified)
        {
            Debug.Log("QuadrantChanged");
            m_stateModified = false;
            LEDManager.SetLEDMode(m_ledPins, m_ledPinStates);
        }
        else if (m_stateModified && !sensorActivated)
        {
            Debug.Log("Proximity Sensor: Reset Pin States");
            m_stateModified = false;
            m_ledPinStates = new int[] { 0, 0, 0, 0 };
            LEDManager.SetLEDMode(m_ledPins, m_ledPinStates);
        }
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
        }

    }
}
