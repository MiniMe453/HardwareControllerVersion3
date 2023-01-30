using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{

    public AudioMixer RadioStaticMixer;
    public AudioMixer RadioStationMixer;
    public AudioMixer WorldSoundsMixer;
    private int m_micLedPin;
    private bool m_isMuted = false;
    private bool m_isMicOn = true;

    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
        
        WorldSoundsMixer.SetFloat("Volume", -80f);
        RadioStaticMixer.SetFloat("Volume", -80f);
        RadioStationMixer.SetFloat("Volume", -80f);
    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("Mic Button").EOnButtonPressed += OnMicButtonPressed;
        ArduinoInputDatabase.GetInputFromName("Mute Button").EOnButtonPressed += OnMuteButtonPressed;
        m_micLedPin = ArduinoInputDatabase.GetOutputIndexFromName("Mic Led Button");

        OnMicButtonPressed(0);
    }

    void OnMicButtonPressed(int pin)
    {
        Debug.LogError("Button pressed");
        m_isMicOn = !m_isMicOn;

        WorldSoundsMixer.SetFloat("Volume", m_isMicOn? 0f : -80f);
        RadioStaticMixer.SetFloat("Volume", m_isMicOn? -80f : 0f);
        RadioStationMixer.SetFloat("Volume", m_isMicOn? -80f : 0f);

        LEDManager.SetLEDMode(m_micLedPin, m_isMicOn? 1 : 0);
    }

    void OnMuteButtonPressed(int pin)
    {
        m_isMuted = !m_isMuted;

        WorldSoundsMixer.SetFloat("Volume", m_isMuted? -80f : (m_isMicOn? 0f : -80f));
        RadioStaticMixer.SetFloat("Volume", m_isMuted? -80f : (m_isMicOn? -80f : 0f));
        RadioStationMixer.SetFloat("Volume",m_isMuted? -80f : ( m_isMicOn? -80f : 0f));
    }
}
