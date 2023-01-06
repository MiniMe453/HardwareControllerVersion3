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
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
    }

    void OnDatabaseInit()
    {
        TimeManager.EOnDateTimeUpdated += OnDateTimeUpdated;
    }

    void OnDateTimeUpdated(DateTimeStruct newDateTime)
    {
        if(ArduinoPrinterManager.Instance.IsPrinting)
            return;
            
        object[] data = new object[2];

        data[0] = TimeManager.ToStringMissionTimeLong(newDateTime);
        data[1] = "TestStringBeingSent!";

        UduinoManager.Instance.sendCommand("lcd", data);
    }
}
