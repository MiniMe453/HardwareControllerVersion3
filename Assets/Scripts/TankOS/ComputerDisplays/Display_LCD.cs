using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.DateTime;
using Uduino;
using Rover.Arduino;

public class Display_LCD : MonoBehaviour
{
    void OnEnable()
    {
        GameInitializer.EOnGameInitialized += OnDatabaseInit;
    }

    void OnDatabaseInit()
    {
        if (UduinoManager.Instance.isConnected())
            TimeManager.EOnDateTimeUpdated += OnDateTimeUpdated;
    }

    void OnDateTimeUpdated(DateTimeStruct newDateTime)
    {
        if (ArduinoPrinterManager.Instance.IsPrinting)
            return;

        object[] data = new object[2];

        data[0] = TimeManager.ToStringMissionTimeLong(newDateTime);
        data[1] = System_GPS.GPSCoordinates.x.ToString("00.000") + ":" + System_GPS.GPSCoordinates.y.ToString("00.000");

        UduinoManager.Instance.sendCommand("lcd", data);
    }
}
