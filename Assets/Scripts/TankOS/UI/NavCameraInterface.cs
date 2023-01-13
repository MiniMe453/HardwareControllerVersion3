using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Interface;
using Rover.OS;
using UnityEngine.UI;
using TMPro;
using UnityTimer;
using System;

public class NavCameraInterface : MonoBehaviour
{

    [Header("Rover Variables")]
    public TextMeshProUGUI brakeText;

    [Header("Roll Pitch Stuff")]
    public RectTransform pitchImageTransform;
    public RectTransform rollImageTransform;

    [Header("Radio Variables")] 
    public TextMeshProUGUI frequencyText;
    public TextMeshProUGUI[] bandArr;

    void OnEnable()
    {
        System_MTR.EOnBrakeModeChanged += OnBrakeStateChanged;
        System_RDIO.EOnRadioFrequencyUpdated += OnRadioFrequencyChanged;
        System_RDIO.EOnNewRadioTypeSelected += OnRadioBandChanged;
    }

    void OnBrakeStateChanged(bool newState)
    {
        brakeText.color = newState? Color.white : Color.gray;
    }

    void OnRadioFrequencyChanged(float newFreq)
    {
        frequencyText.text = Math.Round(newFreq, 1).ToString().PadLeft(5);
    }

    void OnRadioBandChanged(int newBand)
    {
        for(int i = 0; i < bandArr.Length; i++)
        {
            bandArr[i].color = i == newBand? Color.white : Color.gray;
        }
    }

    void Update()
    {
        if(RoverOperatingSystem.OSMode != OSMode.Rover)
            return;

        rollImageTransform.rotation = Quaternion.Euler(new Vector3(0,0,SystemPitchRoll.Roll));
    }
}
