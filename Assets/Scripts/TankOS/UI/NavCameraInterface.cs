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

public class NavCameraInterface : MonoBehaviour
{
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

    [Header("Radio Variables")] 
    public TextMeshProUGUI frequencyText;
    public TextMeshProUGUI[] bandArr;
    [Header("Map Marker")]
    public RectTransform mapMarkerTransform;
    public GameObject mapMarkerPrefab;
    private List<GameObject> m_mapMarkerList;
    [Header("Radio Scan")]
    public RectTransform radioScanTransform;
    private List<GameObject> m_radioScanList;

    void Awake()
    {
        System_MTR.EOnBrakeModeChanged += OnBrakeStateChanged;
        System_RDIO.EOnRadioFrequencyUpdated += OnRadioFrequencyChanged;
        System_RDIO.EOnNewRadioTypeSelected += OnRadioBandChanged;
        System_CAM.EOnNewCameraSelected += OnNewCameraSelected;
    }

    void OnEnable()
    {
        SetMapMarkerList();
        SetRadioScanList();
    }

    void SetMapMarkerList()
    {
        if(System_Nav_Interface.MapMarkers.Count == 0 && m_mapMarkerList.Count == 0)
        {
            GameObject location = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location.GetComponent<TextMeshProUGUI>().text = "Use the map mode";
            m_mapMarkerList.Add(location);

            GameObject location1 = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location1.GetComponent<TextMeshProUGUI>().text = "to create map";
            m_mapMarkerList.Add(location1);

            GameObject location2 = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location2.GetComponent<TextMeshProUGUI>().text = "markers";
            m_mapMarkerList.Add(location2);

            return;
        }


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
            m_mapMarkerList.Add(location);
       }
    }

    void SetRadioScanList()
    {
        if(System_RDIO.PrevScanResults.Count == 0 && m_mapMarkerList.Count == 0)
        {
            GameObject location = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location.GetComponent<TextMeshProUGUI>().text = "Use the cmd console";
            m_mapMarkerList.Add(location);

            GameObject location1 = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location1.GetComponent<TextMeshProUGUI>().text = "scan for radio";
            m_mapMarkerList.Add(location1);

            GameObject location2 = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            location2.GetComponent<TextMeshProUGUI>().text = "frequencies";
            m_mapMarkerList.Add(location2);

            return;
        }


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
            GameObject resultGO = Instantiate(mapMarkerPrefab, mapMarkerTransform);
            resultGO.GetComponent<TextMeshProUGUI>().text = $" {result.radioType.ToString().PadLeft(3)}  {result.frequency.ToString("000.0")}  {result.strength.ToString().PadRight(3)}%";
            m_radioScanList.Add(resultGO);
       }
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
        pitchText.text = "PTCH: " + (SystemPitchRoll.Pitch.ToString("0.0")).PadLeft(6);

        headingText.text = "HDNG: " + System_GPS.Heading.ToString("0.0").PadLeft(6);
        elevationText.text = "ELV: " + System_GPS.Elevation.ToString("0.0").PadLeft(5) + "m";

        speedText.text = "SPD: " + System_MTR.RoverVelocity.ToString("0.0").PadLeft(4) + "m/s";
        throttleText.text = "THRTL:  " + Mathf.CeilToInt(-System_MTR.ThrottleAxis * 100f).ToString().PadLeft(4) + "%";

        gpsCoordsText.text = System_GPS.GPSCoordsToString(System_GPS.GPSCoordinates);
    }
}
