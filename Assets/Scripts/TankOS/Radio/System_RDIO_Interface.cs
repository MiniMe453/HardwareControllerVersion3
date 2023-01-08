using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.OS;
using UnityEngine.UI;
using TMPro;
using UnityTimer;
using Rover.Settings;

public class System_RDIO_Interface : MonoBehaviourApplication
{
    private AudioAnalyzer m_radioSignalAnalyzer;
    public Canvas canvas;
    public List<RawImage> chartNameBackgrounds;
    public List<TextMeshProUGUI> chartNameTMPros;
    public List<TextMeshProUGUI> bandNameTMPros;
    public TextMeshProUGUI signalStrengthText;
    public TextMeshProUGUI signalFrequencyText;
    private static int m_signalStrength;
    public static int SignalStrength {get {return m_signalStrength;}}
    private Timer updateSignalStrengthTimer;
    
    protected override void Init()
    {
        System_RDIO.EOnNewRadioTypeSelected += OnNewRadioModeSelected;
        System_RDIO.EOnRadioFrequencyUpdated += OnNewRadioFrequencySelected;
    }
    protected override void OnAppLoaded()
    {
        UIManager.AddToViewport(canvas, 100);
        updateSignalStrengthTimer = Timer.Register(GameSettings.RADIO_FREQ_CHART_UPDATE_TIMER, () => UpdateSignalStrengthText(), isLooped: true);
    }

    protected override void OnAppQuit()
    {
        UIManager.RemoveFromViewport(canvas);
        updateSignalStrengthTimer.Cancel();
    }

    void OnNewRadioModeSelected(int newMode)
    {
        for(int i = 0; i < 3; i++)
        {
            chartNameBackgrounds[i].color = i == newMode? Color.white : Color.black;
            chartNameTMPros[i].color = i == newMode? Color.black : Color.white;
            bandNameTMPros[i].color = i == newMode? Color.white : Color.gray;
        }
    }

    void OnNewRadioFrequencySelected(float newFreq)
    {
        signalFrequencyText.text = newFreq.ToString("000.0");
    }

    public static void SetSignalStrength(float signalStrength)
    {
        m_signalStrength = Mathf.CeilToInt(signalStrength * 100f);
    }

    void UpdateSignalStrengthText()
    {
        signalStrengthText.text = m_signalStrength.ToString() + "%";
    }
}
