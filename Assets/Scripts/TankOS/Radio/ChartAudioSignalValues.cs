using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AwesomeCharts;
using UnityTimer;
using Rover.OS;
using Rover.Settings;


public class ChartAudioSignalValues : MonoBehaviour
{
    public MonoBehaviourApplication mainApplication;
    private LineDataSet m_dataSet = new LineDataSet();
    private LineChart m_chart => GetComponent<LineChart>();
    private Timer updateDataSetTimer;
    public RadioManager.ERadioTypes radioType;
    public static float maxMagnitude = 10f;
    private static float m_sampleAmplitude = 1;
    public int numberOfEntries = 25;

    
    void OnEnable()
    {
        m_chart.AxisConfig.VerticalAxisConfig.Bounds.Max = maxMagnitude + 1f;
        m_chart.AxisConfig.VerticalAxisConfig.Bounds.Min = -(maxMagnitude + 1f);

        List<LineEntry> lineEntries = new List<LineEntry>();

        for(int i = 0; i <= numberOfEntries; i++)
        {
            LineEntry newEntry = new LineEntry();
            lineEntries.Add(new LineEntry((float)i/(float)numberOfEntries, 0f));
            Debug.LogError(lineEntries[i].Position);
        }

        m_dataSet.Entries = lineEntries;
        m_chart.GetChartData().DataSets.Add(m_dataSet);

        System_RDIO.EOnNewRadioTypeSelected += OnNewRadioTypeSelected;
        mainApplication.EOnAppLoaded += OnMainApplicationLoaded;
        mainApplication.EOnAppUnloaded += OnMainApplicationUnloaded;
    }

    void OnMainApplicationLoaded()
    {
        if(System_RDIO.SelectedRadioType == radioType)
        {
            updateDataSetTimer = Timer.Register(GameSettings.RADIO_FREQ_CHART_UPDATE_TIMER, () => UpdateChart(), isLooped: true);
        }
    }

    void OnMainApplicationUnloaded()
    {
        if(updateDataSetTimer != null)
        {
            updateDataSetTimer.Cancel();
        }
    }

    void OnNewRadioTypeSelected(int newRadioType)
    {
        if(newRadioType == (int)radioType && mainApplication.AppIsLoaded)
        {
            updateDataSetTimer = Timer.Register(GameSettings.RADIO_FREQ_CHART_UPDATE_TIMER, () => UpdateChart(), isLooped: true);
        }
        else if(updateDataSetTimer != null)
        {
            updateDataSetTimer.Cancel();
        }
    }

    void UpdateChart()
    {
        for(int i = 0; i < m_dataSet.Entries.Count; i++)
        {
           m_dataSet.Entries[i].Value = Random.Range(-1f,1f) * m_sampleAmplitude; 
        }

        m_chart.SetDirty();
    }

    public static void UpdateClipLoundess(float clipLoudness)
    {
        m_sampleAmplitude = 1 + (clipLoudness * maxMagnitude);
    }
}
