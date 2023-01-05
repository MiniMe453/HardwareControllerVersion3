using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

namespace Rover.Arduino
{
    public class ArduinoPrinterManager : MonoBehaviour
    {
        public static ArduinoPrinterManager Instance;

        void Awake()
        {
            // ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
            Instance = this;
        }

        // void OnDatabaseInit()
        // {
        //     ArduinoInputDatabase.GetInputFromName("Print Button 01").EOnButtonPressed += PrintObjectScan;
        // }

        public void PrintObjectScan(int pin)
        {
            object[] data = new object[1];

            data[0] = "This was sent from Unity";

            UduinoManager.Instance.sendCommand("pobj", data);
        }

        public void SimplePrint(string _data)
        {
            object[] data = new object[1];

            data[0] = _data;

            UduinoManager.Instance.sendCommand("prt", data);
        }
    }
}
