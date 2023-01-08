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
    public RectTransform radioScanTransform;
    public GameObject radioScanListEntry;
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

        if(System_RDIO.PrevScanResults.Count == 0)
        {
            GameObject entry = Instantiate(radioScanListEntry, radioScanTransform);
            entry.GetComponent<TextMeshProUGUI>().text = "NO SCAN PERFORMED";
            GameObject entry2 = Instantiate(radioScanListEntry, radioScanTransform);
            entry2.GetComponent<TextMeshProUGUI>().text = "USE SYS.RDIO.SCAN";
            GameObject entry3 = Instantiate(radioScanListEntry, radioScanTransform);
            entry3.GetComponent<TextMeshProUGUI>().text = "TO PERFORM A SCAN";
            return;
        }

        if(radioScanTransform.childCount > 0)
        {
            for(int i = 0; i < radioScanTransform.childCount; i++)
            {
                DestroyImmediate(radioScanTransform.GetChild(i));
            }
        }

        foreach(Struct_RadioScan result in System_RDIO.PrevScanResults)
        {
            GameObject entry = Instantiate(radioScanListEntry, radioScanTransform);
            entry.GetComponent<TextMeshProUGUI>().text = $"  {result.radioType.ToString().PadLeft(3)}    {result.frequency.ToString("000.0")}  {result.strength.ToString().PadRight(3)}%";
        }
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
