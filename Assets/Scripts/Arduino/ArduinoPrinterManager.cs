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
            List<object> dataItr1 = new List<object>();
            List<object> dataItr2 = new List<object>();
            List<object> dataItr3 = new List<object>();

            dataItr1.Add(0);
            dataItr1.Add(objectScan.objName);
            dataItr1.Add(objectScan.objectSurfaceDepth.ToString("00.0"));
            dataItr1.Add(objectScan.temperature.ToString("000.0"));
            dataItr1.Add(objectScan.magneticField.ToString("000.0"));
            dataItr1.Add(objectScan.radiation.ToString("000.0"));
            
            dataItr2.Add(1);
            dataItr2.Add(objectScan.objDistAtScan.ToString("0.0"));
            dataItr2.Add(TimeManager.ToStringIngameDate + ", " + TimeManager.ToStringMissionTimeClk(objectScan.scanTime));
            
            // dataItr3.Add(objectScan.surfaceProperties.Count);
            dataItr3.Add(128);

            foreach(SurfaceProperty property in objectScan.surfaceProperties)
            {
                dataItr3.Add((int)property.materialType);
                dataItr3.Add(property.materialDensity.ToString("0.0"));
            }

            m_objectScanLists.Add(dataItr1);
            m_objectScanLists.Add(dataItr2);
            m_objectScanLists.Add(dataItr3);

            StartCoroutine(SendObjScanCoroutine());


        }

        IEnumerator SendObjScanCoroutine()
        {
            int iteration = 0;

            while(iteration < 3)
            {
                if(iteration < 2)
                {
                    UduinoManager.Instance.sendCommand("sobs", m_objectScanLists[iteration].ToArray());  
                }
                else
                {
                    UduinoManager.Instance.sendCommand("pobs", m_objectScanLists[iteration].ToArray());  
                    RoverOperatingSystem.SetArduinoEnabled(false);

                    m_printMessageBox = UIManager.ShowMessageBox("PRINTING DATA", Color.red, -1f);
                    //UduinoManager.Instance.pauseArduinoWrite = true;
                    m_isPrinting = true;
                }
                
                
                iteration++;
                yield return new WaitForSeconds(0.01f);
            }
        }

        public void SimplePrint(string _data)
        {
            object[] data = new object[1];

            data[0] = _data;

            UduinoManager.Instance.sendCommand("prt", data);
        }
    }
}
