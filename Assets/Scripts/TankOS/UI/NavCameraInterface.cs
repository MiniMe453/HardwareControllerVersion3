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

    void Awake()
    {
        System_MTR.EOnBrakeModeChanged += OnBrakeStateChanged;
        System_RDIO.EOnRadioFrequencyUpdated += OnRadioFrequencyChanged;
        System_RDIO.EOnNewRadioTypeSelected += OnRadioBandChanged;
        System_CAM.EOnNewCameraSelected += OnNewCameraSelected;
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
