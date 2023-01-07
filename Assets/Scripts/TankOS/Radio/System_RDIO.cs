using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using System;
using Uduino;

public class System_RDIO : MonoBehaviour
{
    private static RadioReceiverData m_receiverData;
    public static RadioReceiverData ReceiverData {get {return m_receiverData;}}
    private float m_freqPercentage;
    private float m_frequency;
    public float Frequency {get {return m_frequency;}}
    private int m_selectedRadioType;
    private int[] m_radioLEDPinIndexes;
    public static event Action<int> EOnNewRadioTypeSelected;
    public static event Action<float> EOnRadioFrequencyUpdated;

    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
        m_receiverData = GetComponent<RadioReceiver>().RecieverData;
    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("Rotary Encoder Counter").EOnValueChanged += OnRadioEncoderValueChanged;
        ArduinoInputDatabase.GetInputFromName("Rotary Encoder Button").EOnButtonPressed += OnRadioButtonPressed;
        m_radioLEDPinIndexes = new int[] {
            ArduinoInputDatabase.GetOutputIndexFromName("Radio LED 1"),
            ArduinoInputDatabase.GetOutputIndexFromName("Radio LED 2"),
            ArduinoInputDatabase.GetOutputIndexFromName("Radio LED 3")
        };
    }

    void OnRadioEncoderValueChanged(float value, int pin)
    {
        m_freqPercentage = value/1024f;

        m_frequency = ((ReceiverData.frequencyMax - ReceiverData.frequencyMin) * m_freqPercentage) + ReceiverData.frequencyMin;
        ReceiverData.Frequency = m_frequency;

        string strFreq = "";
        float tempValue = Mathf.FloorToInt(ReceiverData.Frequency * 10f);
        strFreq = tempValue.ToString();

       // Debug.LogError(ReceiverData.Frequency);
         
        if(strFreq.Length < 4)
            for(int i = 0; i < 4 - strFreq.Length; i++)
            {
                strFreq = "0" + strFreq;
            }

        Debug.LogError(strFreq);

        object[] data = new object[5];
        data[4] = 0;

        for(int i = 0; i < 4; i++)
        {
            data[i] = Int32.Parse(strFreq[i].ToString());
        }

        UduinoManager.Instance.sendCommand("writeTM1", data);
        EOnRadioFrequencyUpdated?.Invoke(ReceiverData.Frequency);
    }

    void OnRadioButtonPressed(int pin)
    {
        m_selectedRadioType++;

        if(m_selectedRadioType > 2)
            m_selectedRadioType = 0;

        Debug.LogError(m_selectedRadioType);

        int[] ledValues = new int[3];

        for(int i = 0; i < 3; i++)
        {
            ledValues[i] = i == m_selectedRadioType? 1 : 0;
        }

        LEDManager.SetLEDMode(m_radioLEDPinIndexes, ledValues);

        ReceiverData.RadioType = (RadioManager.ERadioTypes)m_selectedRadioType;
        // OnRadioEncoderValueChanged(m_freqPercentage * 1024f, 0);

        // EOnNewRadioTypeSelected?.Invoke(m_selectedRadioType);

        Debug.Log(ReceiverData.RadioType);
    }
}
