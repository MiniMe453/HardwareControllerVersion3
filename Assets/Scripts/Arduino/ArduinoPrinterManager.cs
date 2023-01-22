using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using Rover.Interface;
using Rover.DateTime;

namespace Rover.Arduino
{
    public class ArduinoPrinterManager : MonoBehaviour
    {
        public static ArduinoPrinterManager Instance;
        private MessageBox m_printMessageBox;
        private List<List<object>> m_objectScanLists = new List<List<object>>();
        private bool m_isPrinting = false;
        public bool IsPrinting {get {return m_isPrinting;}}

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

            m_isPrinting = false;
            //UduinoManager.Instance.pauseArduinoWrite = false;
        }

        public void PrintObjectScan(Struct_ObjectScan objectScan)
        {
            List<object> data = new List<object>();

            data.Add((int)objectScan.scanDataType);
            data.Add(objectScan.objDistAtScan * 100f);

            UduinoManager.Instance.sendCommand("pobs", data.ToArray());  
            RoverOperatingSystem.SetArduinoEnabled(false);

            m_printMessageBox = UIManager.ShowMessageBox("PRINTING DATA", Color.red, -1f);
            //UduinoManager.Instance.pauseArduinoWrite = true;
            m_isPrinting = true;

        }

        public void SimplePrint(string _data)
        {
            object[] data = new object[1];

            data[0] = _data;

            UduinoManager.Instance.sendCommand("prt", data);
        }
    }
}
