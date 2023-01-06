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
            List<object> data = new List<object>();

            data.Add(objectScan.surfaceProperties.Count);
            data.Add(objectScan.objName);
            data.Add(objectScan.objectSurfaceDepth.ToString("00.0"));
            data.Add(objectScan.temperature.ToString("000.0"));
            data.Add(objectScan.magneticField.ToString("000.0"));
            data.Add(objectScan.radiation.ToString("000.0"));
            data.Add(objectScan.objDistAtScan.ToString("0.0"));
            data.Add(TimeManager.ToStringIngameDate + "," + TimeManager.ToStringMissionTimeClk(objectScan.scanTime));

            foreach(SurfaceProperty property in objectScan.surfaceProperties)
            {
                data.Add((int)property.materialType);
                data.Add(property.materialDensity.ToString());
            }

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
