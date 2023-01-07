using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;

public class System_RDIO : MonoBehaviour
{
    private static RadioReceiverData m_receiverData;
    public static RadioReceiverData ReceiverData {get {return m_receiverData;}}
    private float m_freqPercentage;
    private float m_frequency;
    public float Frequency {get {return m_frequency;}}

    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
        m_receiverData = GetComponent<RadioReceiver>().RecieverData;
    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("Rotary Encoder Counter").EOnValueChanged += OnRadioEncoderValueChanged;
    }

    void OnRadioEncoderValueChanged(float value, int pin)
    {
        m_freqPercentage = value/512f;

        m_frequency = ((ReceiverData.frequencyMax - ReceiverData.frequencyMin) * m_freqPercentage) + ReceiverData.frequencyMin;
        ReceiverData.Frequency = m_frequency;

        Debug.LogError(ReceiverData.Frequency);
    }
}
