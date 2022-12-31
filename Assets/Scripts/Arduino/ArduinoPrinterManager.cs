using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using Rover.Arduino;

public class ArduinoPrinterManager : MonoBehaviour
{


    void Awake()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("Print Button 01").EOnButtonPressed += PrintObjectScan;
    }

    public void PrintObjectScan(int pin)
    {
        object[] data = new object[1];

        data[0] = "This was sent from Unity";

        UduinoManager.Instance.sendCommand("pobj", data);
    }
}
