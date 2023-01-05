using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rover.Systems;
using System;
using Rover.OS;

public class HomeScreen : MonoBehaviourApplication
{
    [Header("Main UI Variables")]
    public Canvas homeScreenCanvas;
    [Header("Rover and Camera Variables")]
    public TextMeshProUGUI[] selectedCameraArray;
    public TextMeshProUGUI roverSpeedText;
    public TextMeshProUGUI brakeText;

    public static event Action EOnHomeScreenLoaded;
    public static event Action EOnHomeScreenRemoved;

    void OnEnable()
    {
        System_CAM.EOnNewCameraSelected += OnNewCameraSelected;
        System_MTR.EOnBrakeModeChanged += OnBrakeSwitchChanged;
        System_MTR.EOnRoverVelocityUpdate += OnRoverVelocityUpdate;
    }

    public void LoadHomeScreen()
    {
        AppDatabase.LoadApp(AppID);
    }

    public void RemoveHomeScreen()
    {
        AppDatabase.CloseApp(AppID);
    }

    void OnNewCameraSelected(CameraMode newMode)
    {
        for(int i = 0; i < selectedCameraArray.Length; i++)
        {
            if(i == (int)newMode)
            {
                selectedCameraArray[i].color = Color.white;
                continue;
            }

            selectedCameraArray[i].color = Color.gray;
        }
    }

    void OnBrakeSwitchChanged(bool brakeActive)
    {
        brakeText.color = brakeActive? Color.white : Color.gray;
    }

    void OnRoverVelocityUpdate(float velocity)
    {
        roverSpeedText.text = "SPD: " + velocity.ToString("00.0") + "m/s";
    }
}
