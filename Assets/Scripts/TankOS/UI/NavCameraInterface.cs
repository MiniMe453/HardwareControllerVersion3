using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Interface;
using Rover.OS;
using UnityEngine.UI;
using TMPro;
using UnityTimer;
using System;
using Rover.Systems;
using Rover.DateTime;

public class NavCameraInterface : MonoBehaviour
{
    public TextMeshProUGUI dateTimeText;
    [Header("Camera System")]
    public TextMeshProUGUI[] cameraTexts;

    [Header("Rover Variables")]
    public TextMeshProUGUI brakeText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI throttleText;

    [Header("Positioning Variables")]
    public RectTransform pitchImageTransform;
    public RectTransform rollImageTransform;
    public TextMeshProUGUI rollText;
    public TextMeshProUGUI pitchText;
    public TextMeshProUGUI headingText;
    public TextMeshProUGUI elevationText;
    public TextMeshProUGUI gpsCoordsText;
    public RectTransform compassTransform;
    public RectTransform compassStaticTransform;
    public GameObject mapMarkerIconPrefab;

    [Header("Radio Variables")] 
    public TextMeshProUGUI frequencyText;
    public TextMeshProUGUI freqStrengthText;
    public TextMeshProUGUI[] bandArr;
    [Header("Map Marker")]
    public RectTransform mapMarkerTransform;
    public GameObject mapMarkerPrefab;
    private List<GameObject> m_mapMarkerList = new List<GameObject>();
    [Header("Radio Scan")]
    public RectTransform radioScanTransform;
    private List<GameObject> m_radioScanList = new List<GameObject>();
    [Header("Sensors")]
    public TextMeshProUGUI temperatureReading;
    public TextMeshProUGUI radiationReading;
    public TextMeshProUGUI magneticReading;
    [Header("Warning Lights")]
    public TextMeshProUGUI[] proximityTexts;
    public TextMeshProUGUI magWarnText;
    public TextMeshProUGUI tempWarnText;
    public TextMeshProUGUI radWarnText;
    public TextMeshProUGUI angleWarnText;
    [Header("Transmit Texts")]
    public TextMeshProUGUI transmitText;
    public TextMeshProUGUI readyText;

    void Awake()
    {
        System_MTR.EOnBrakeModeChanged += OnBrakeStateChanged;
        System_RDIO.EOnRadioFrequencyUpdated += OnRadioFrequencyChanged;
        System_RDIO.EOnNewRadioTypeSelected += OnRadioBandChanged;
        System_CAM.EOnNewCameraSelected += OnNewCameraSelected;
        RoverOperatingSystem.EOnOSModeChanged += OnRoverOSModeChanged;
        System_SRS.EOnSensorsUpdated += OnSensorsRead;

        System_WARN.EOnAngleWarningLight += OnAngleWarning;
        System_WARN.EOnRadiationWarningLight += OnRadiationWarning;
        System_WARN.EOnTemperatureWarningLight += OnTemperatureWarning;
        System_WARN.EOnMagWarningLight += OnMagneticWarning;
        System_WARN.EOnProximitySensorModified += OnProximityWarning;

        System_Nav_Interface.EOnMapMarkerAdded += OnMapMarkerAddedToWorld;

        RoverOperatingSystem.EOnArduinoInputStateChanged += OnArduinoInputChanged;
        TimeManager.EOnDateTimeUpdated += OnNewDateTime;
    }

    void OnEnable()
    {
        SetMapMarkerList();
        SetRadioScanList();
    }

    void OnRoverOSModeChanged(OSMode newMode)
    {
        if(newMode != OSMode.Rover)
            return;
        
        SetMapMarkerList();
        SetRadioScanList();
    }

    void OnNewDateTime(DateTimeStruct time)
    {
        dateTimeText.text = TimeManager.ToStringIngameDate + " " + TimeManager.ToStringMissionTimeClk(time);
    }

    void OnArduinoInputChanged(bool newInput)
    {
        transmitText.color = newInput? Color.gray : Color.white;
        readyText.color = newInput? Color.white : Color.gray;
    }

    void OnRadiationWarning(bool state)
    {
        radWarnText.color = state? Color.white : Color.gray;
        radiationReading.color = state? Color.red : Color.white;
    }

    void OnTemperatureWarning(bool state)
    {
        tempWarnText.color = state? Color.white : Color.gray;
        temperatureReading.color = state? Color.red : Color.white;
    }

    void OnMagneticWarning(bool state)
    {
        magWarnText.color = state? Color.white : Color.gray;
        magneticReading.color = state? Color.red : Color.white;
    }
    
    void OnAngleWarning(bool state)
    {
        angleWarnText.color = state? Color.white : Color.gray;
        rollText.color = state? Color.red : Color.white;
        pitchText.color = state? Color.red : Color.white;
    }

    void OnProximityWarning(int state)
    {
        for(int i = 0; i < proximityTexts.Length; i++)
        {
            proximityTexts[i].color = i == state? Color.white: Color.gray;
        }
    }

    void OnSensorsRead()
    {
        if(RoverOperatingSystem.OSMode != OSMode.Rover)
            return;

        temperatureReading.text = System_SRS.Temperature.ToString("0.0").PadRight(4) + "C";
        radiationReading.text = System_SRS.Radiation.ToString("0.0000").PadLeft(6) + "uSv/h";
        magneticReading.text = System_SRS.Magnetic.ToString("0.0").PadLeft(4) + "G";
    }

    void SetMapMarkerList()
    {
        if(System_Nav_Interface.MapMarkers.Count == 0 && m_mapMarkerList.Count == 0)
        {
            GameObject location = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location.GetComponent<TextMeshProUGUI>().text = "MAP MARKERS";
            m_mapMarkerList.Add(location);

            GameObject location1 = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location1.GetComponent<TextMeshProUGUI>().text = "MODE TO CREATE";
            m_mapMarkerList.Add(location1);

            GameObject location2 = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location2.GetComponent<TextMeshProUGUI>().text = "USE THE MAP";
            m_mapMarkerList.Add(location2);

            return;
        }

        if(System_Nav_Interface.MapMarkers.Count == 0)
            return;

        if(m_mapMarkerList.Count > 0)
        {
                for(int i =0; i < m_mapMarkerList.Count;i++)
                {
                    DestroyImmediate(m_mapMarkerList[i]);   
                }

                m_mapMarkerList.Clear();
        }

        foreach(string marker in System_Nav_Interface.MapMarkers)
        {
                GameObject location = Instantiate(mapMarkerPrefab, mapMarkerTransform);
                location.GetComponent<TextMeshProUGUI>().text = marker;
                location.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                m_mapMarkerList.Add(location);
        }
    }

    void SetRadioScanList()
    {
        if(System_RDIO.PrevScanResults.Count == 0 && m_radioScanList.Count == 0)
        {
            GameObject location = Instantiate(mapMarkerPrefab, radioScanTransform);
            location.GetComponent<TextMeshProUGUI>().text = "RADIO FREQUENCIES";
            m_radioScanList.Add(location);

            GameObject location1 = Instantiate(mapMarkerPrefab, radioScanTransform);
            location1.GetComponent<TextMeshProUGUI>().text = "CONSOLE TO SCAN";
            m_radioScanList.Add(location1);

            GameObject location2 = Instantiate(mapMarkerPrefab, radioScanTransform);
            location2.GetComponent<TextMeshProUGUI>().text = "USE THE COMMAND";
            m_radioScanList.Add(location2);

            return;
        }

        if(System_RDIO.PrevScanResults.Count == 0)
            return;

       if(m_radioScanList.Count > 0)
       {
            for(int i =0; i < m_radioScanList.Count;i++)
            {
                DestroyImmediate(m_radioScanList[i]);   
            }

            m_radioScanList.Clear();
       }

       foreach(Struct_RadioScan result in System_RDIO.PrevScanResults)
       {
            GameObject resultGO = Instantiate(mapMarkerPrefab, radioScanTransform);
            resultGO.GetComponent<TextMeshProUGUI>().text = $" {result.radioType.ToString().PadLeft(3)}  {result.frequency.ToString("000.0")}  {result.strength.ToString().PadLeft(3)}";
            resultGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            m_radioScanList.Add(resultGO);
       }

        GameObject InitVal = Instantiate(mapMarkerPrefab, radioScanTransform);
        InitVal.GetComponent<TextMeshProUGUI>().text = $" {"BND".PadLeft(3)}  {"FREQ".PadLeft(5)}  {"DIR".PadLeft(3)}";
        InitVal.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        m_radioScanList.Add(InitVal);
    }

    void OnBrakeStateChanged(bool newState)
    {
        brakeText.color = newState? Color.white : Color.gray;
    }

    void OnRadioFrequencyChanged(float newFreq)
    {
        frequencyText.text = (Math.Round(newFreq, 1).ToString()).PadLeft(5);
    }

    void OnRadioBandChanged(int newBand)
    {
        for(int i = 0; i < bandArr.Length; i++)
        {
            bandArr[i].color = i == newBand? Color.white : Color.gray;
        }
    }

    void OnNewCameraSelected(CameraMode newMode)
    {
        for(int i = 0; i < cameraTexts.Length; i++)
        {
            cameraTexts[i].color = i == (int)newMode? Color.white : Color.gray;
        }
    }

    void Update()
    {
        if(RoverOperatingSystem.OSMode != OSMode.Rover)
            return;

        rollImageTransform.rotation = Quaternion.Euler(new Vector3(0,0,SystemPitchRoll.Roll));
        rollText.text = "ROLL: " + (SystemPitchRoll.Roll.ToString("0.0")).PadLeft(6);

        pitchImageTransform.anchoredPosition = new Vector2(0,SystemPitchRoll.Pitch * 3f);
        pitchText.text = "PTCH: " + SystemPitchRoll.Pitch.ToString("0.0").PadLeft(6);

        headingText.text = "HDNG: " + System_GPS.Heading.ToString("0.0").PadLeft(6);
        elevationText.text = "ELV: " + System_GPS.Elevation.ToString("0.0").PadLeft(5) + "m";

        Vector3 newCompassPos = new Vector3(System_GPS.Heading/360f * 537f, 0,0);
        compassTransform.anchoredPosition = newCompassPos;

        speedText.text = "SPD: " + System_MTR.RoverVelocity.ToString("0.0").PadLeft(4) + "m/s";
        throttleText.text = "THRTL:  " + Mathf.CeilToInt(-System_MTR.ThrottleAxis * 100f).ToString().PadLeft(4) + "%";

        gpsCoordsText.text = System_GPS.GPSCoordsToString(System_GPS.GPSCoordinates);

        freqStrengthText.text = System_RDIO.SignalStrength.ToString().PadLeft(3) + "%";
    }

    private void OnMapMarkerAddedToWorld(Vector3 markerLocation)
    {
        GameObject obj = Instantiate(mapMarkerIconPrefab, compassStaticTransform);
        UpdateMapMarkerCompass updateMapMarker = obj.GetComponent<UpdateMapMarkerCompass>();
        updateMapMarker.SetMarkerData(markerLocation);
    }
}
