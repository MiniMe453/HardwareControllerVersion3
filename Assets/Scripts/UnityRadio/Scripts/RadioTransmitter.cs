using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioTransmitter : MonoBehaviour
{
    [SerializeField]
    private RadioTransmitterData m_RadioTransmitterData;

    [SerializeField]
    private RadioManager m_RadioManager;

    [SerializeField]
    private Transform m_TransmitterTransform;

    [SerializeField]
    private AudioSource m_TransmitterSource;
    [SerializeField]
    private AudioLowPassFilter m_LowPassFilter;
    private AudioAnalyzer audioAnalyzer;
    public float SignalStrength;
    public RadioTransmitterData TransmitterData {get{return m_RadioTransmitterData;}}

    void Start()
    {
        audioAnalyzer = new AudioAnalyzer(1024, m_TransmitterSource);

        if (m_TransmitterSource.outputAudioMixerGroup != m_RadioTransmitterData.mixerGroup)
        {
            m_TransmitterSource.outputAudioMixerGroup = m_RadioTransmitterData.mixerGroup;
        }

        m_RadioManager.RegisterTransmitter(this);

        m_TransmitterSource.volume = 0f;
    }

    void OnApplicationQuit()
    {
        m_RadioManager.DeregisterTransmitter(this);
    }

    public float GetPt()
    {
        return 10 * Mathf.Log10(m_RadioTransmitterData.watts) + 30;
    }

    public float GetGt()
    {
        return m_RadioTransmitterData.antennaGain;
    }

    public float GetCableLoss()
    {
        return m_RadioTransmitterData.cableLoss;
    }

    public float GetFrequency()
    {
        return m_RadioTransmitterData.frequency;
    }

    public Vector3 GetTransmitterLocation()
    {
        return m_TransmitterTransform.position;
    }

    public void SetSignalStrength(float signalVolume)
    {
        if(m_TransmitterSource == null)
            return;

        m_TransmitterSource.volume = signalVolume;
        m_LowPassFilter.cutoffFrequency = ((signalVolume * signalVolume) * 4) * 2000;
        SignalStrength = signalVolume;
    }

    public float GetAverageAmplitude()
    {
        if(audioAnalyzer == null)
            return 0f;

        return audioAnalyzer.AverageAmplitude();
    }
}
