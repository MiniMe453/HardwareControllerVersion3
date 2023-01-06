using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using Rover.Interface;

namespace Rover.Arduino
{
    public class ArduinoPrinterManager : MonoBehaviour
    {
        public static ArduinoPrinterManager Instance;
        private MessageBox m_printMessageBox;

        void Awake()
        {
            // ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
            Instance = this;
            ArduinoInputDecoder.EOnPrinterFinishedMessageReceived += OnPrintFinished;
        }

        // void OnDatabaseInit()
        // {
        //     ArduinoInputDatabase.GetInputFromName("Print Button 01").EOnButtonPressed += PrintObjectScan;
        // }

        void OnPrintFinished()
        {
            RoverOperatingSystem.SetArduinoEnabled(true);

            if(m_printMessageBox)
            {
                m_printMessageBox.HideMessageBox();
                m_printMessageBox = null;
            }
        }

        public void PrintObjectScan(Struct_ObjectScan objectScan)
        {
            object[] data = new object[1];

            data[0] = "This was sent from Unity";

            UduinoManager.Instance.sendCommand("pobs", data);

            RoverOperatingSystem.SetArduinoEnabled(false);

            m_printMessageBox = UIManager.ShowMessageBox("PRINTING DATA", Color.red, -1f);
        }

        public void SimplePrint(string _data)
        {
            object[] data = new object[1];

            data[0] = _data;

            UduinoManager.Instance.sendCommand("prt", data);
        }
    }
}
