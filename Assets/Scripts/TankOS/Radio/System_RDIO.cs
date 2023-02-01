using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using System;
using Uduino;
using Rover.Interface;
using Rover.DateTime;

public class System_RDIO : MonoBehaviour
{
    public static System_RDIO Instance;
    private static RadioReceiverData m_receiverData;
    public static RadioReceiverData ReceiverData {get {return m_receiverData;}}
    private float m_freqPercentage;
    private float m_frequency;
    public float Frequency {get {return m_frequency;}}
    private static int m_selectedRadioType = 2;
    public static RadioManager.ERadioTypes SelectedRadioType {get {return (RadioManager.ERadioTypes)m_selectedRadioType;}}
    private int[] m_radioLEDPinIndexes;
    public static event Action<int> EOnNewRadioTypeSelected;
    public static event Action<float> EOnRadioFrequencyUpdated;
    private static List<Struct_RadioScan> m_prevScanResult = new List<Struct_RadioScan>();
    public static List<Struct_RadioScan> PrevScanResults { get {return m_prevScanResult;}}
    private static int m_signalStrength;
    public static int SignalStrength {get{return m_signalStrength;}}

    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
        m_receiverData = GetComponent<RadioReceiver>().RecieverData;
        Instance = this;
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

        OnRadioButtonPressed(0);
    }


    void OnRadioEncoderValueChanged(float value, int pin)
    {
        m_freqPercentage = value/1024f;

        m_frequency = ((ReceiverData.frequencyMax - ReceiverData.frequencyMin) * m_freqPercentage) + ReceiverData.frequencyMin;
        ReceiverData.Frequency = m_frequency;

        string strFreq = "";
        
        if(ReceiverData.Frequency < 10)
            strFreq = (ReceiverData.Frequency*10f).ToString("000.0");
        else
            strFreq = ReceiverData.Frequency.ToString("000.0");
        
        strFreq = strFreq.Replace('.',strFreq[4]).Remove(4);

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
        OnRadioEncoderValueChanged(m_freqPercentage * 1024f, 0);

        EOnNewRadioTypeSelected?.Invoke(m_selectedRadioType);
    }

    public void SetScanResult(List<Struct_RadioScan> scanResult)
    {
        m_prevScanResult = scanResult;

        if(!CommandConsoleMain.IsConsoleVisible)
            return;

        StartCoroutine(DisplayScanResultsAnim());
    }

    public static void SetSignalStrength(float signalStrength)
    {
        m_signalStrength = Mathf.CeilToInt(signalStrength * 100f);
    }

    IEnumerator DisplayScanResultsAnim()
    {
        RoverOperatingSystem.SetArduinoEnabled(false);
        CommandConsoleMain.Instance.EnableUserInput(false);
        MessageBox scanningMessage = UIManager.ShowMessageBox("SCANNING FREQUENCIES", Color.white, -1f);
        int counter = 0;

        CommandConsoleMain.Instance.UpdateConsoleOutput("TIME: " + TimeManager.ToStringMissionTimeLong(TimeManager.dateTime));
        CommandConsoleMain.Instance.UpdateConsoleOutput("================");
        CommandConsoleMain.Instance.UpdateConsoleOutput("BND   FREQ   STR");
        CommandConsoleMain.Instance.UpdateConsoleOutput("----------------");
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f,2f));

        while(true)
        {
            if(counter == m_prevScanResult.Count)
                break;

            Struct_RadioScan scan = m_prevScanResult[counter];
            CommandConsoleMain.Instance.UpdateConsoleOutput($"{scan.radioType.ToString().PadRight(3)} | {scan.frequency.ToString("000.0")} | {scan.strength.ToString().PadLeft(3)}%");

            yield return new WaitForSeconds(UnityEngine.Random.Range(1f,2f));
            counter++;
        }
        CommandConsoleMain.Instance.UpdateConsoleOutput("----------------");
        CommandConsoleMain.Instance.UpdateConsoleOutput($"FOUND: {m_prevScanResult.Count}");
        CommandConsoleMain.Instance.UpdateConsoleOutput("================");
        CommandConsoleMain.Instance.UpdateConsoleOutput("SCAN COMPLETE");
        
        scanningMessage.HideMessageBox();
        RoverOperatingSystem.SetArduinoEnabled(true);
        CommandConsoleMain.Instance.EnableUserInput(true);
    }
}
