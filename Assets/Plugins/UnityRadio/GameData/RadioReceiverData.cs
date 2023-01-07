using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "Radio Receiver", menuName = "Radio/Radio Receiver")]
public class RadioReceiverData : ScriptableObject
{
    private RadioManager.ERadioTypes m_RadioType;
    public RadioManager.ERadioTypes RadioType
    {
        get { return m_RadioType; }
        set
        {
            m_RadioType = value;

            switch (m_RadioType)
            {
                case RadioManager.ERadioTypes.AM:
                    SetFrequencyData(1.7f, 0.54f, 0.01f);
                    break;
                case RadioManager.ERadioTypes.FM:
                    SetFrequencyData(108f, 87.8f, 0.2f);
                    break;
                case RadioManager.ERadioTypes.Other:
                    SetFrequencyData(0.54f, 108f, 0.2f);
                    break;
            }
        }
    }
    public float sensitivity = -80f;

    private RadioManager.EAntennaTypes m_AntennaType;

    public RadioManager.EAntennaTypes AntennaType
    {
        get { return m_AntennaType; }
        set
        {
            m_AntennaType = value;

            switch (m_AntennaType)
            {
                case RadioManager.EAntennaTypes.Directional:
                    sensitivity = -80;
                    break;
                case RadioManager.EAntennaTypes.Omnidirectional:
                    sensitivity = -70;
                    break;
                case RadioManager.EAntennaTypes.BeamAntenna:
                    sensitivity = -90;
                    break;
            }
        }
    }
    private float m_Frequency = 100f;
    public float Frequency
    {
        get { return m_Frequency; }
        set
        {
            m_Frequency = value;
            if (m_Frequency >= frequencyMax)
            {
                m_Frequency = frequencyMax;
            }
            else if (m_Frequency <= frequencyMin)
            {
                m_Frequency = frequencyMin;
            }
        }
    }
    public float frequencyMax;
    public float frequencyMin;
    public float frequencyBand;

    public AudioMixerGroup mixerGroup;

    private float volume;

    public float Volume
    {
        get { return volume; }
        set
        {
            volume = Mathf.Clamp01(value);
            mixerGroup.audioMixer.SetFloat("Volume", -Mathf.Pow(1 - volume, 6) * 80);
        }
    }

    public float signalStrength;
    private void SetFrequencyData(float max, float min, float band)
    {
        m_Frequency = (((m_Frequency - frequencyMin) / (frequencyMax - frequencyMin)) * (max - min)) + min;



        frequencyMax = max;
        frequencyMin = min;
        frequencyBand = band;
    }

    public string FrequencyData()
    {
        return frequencyMax + " | " + frequencyMin;
    }
}
