using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Uduino;

namespace Rover.Arduino
{
            
    [System.Serializable]
    public class ArduinoInputData
    {
        public InputType inputType;
        public ArduinoPinMode pinMode;
        public int pinNumber;
        public string inputName;
        public int inputIndex;
        public bool canHoldButton;
        public int dataIndex;
    }

    [System.Serializable]
    public class ArduinoOutputData
    {
        public ArduinoPinMode pinMode = ArduinoPinMode.OUTPUT;
        public string outputName;
        public int pinNumber;
        public int dataIndex;
    }
    //
    [CreateAssetMenu(fileName = "Arduino InputActionMap", menuName = "Rover/Arduino InputActionMap")]
    public class ArduinoInputActionMap : ScriptableObject
    {
        public List<ArduinoInputData> m_InputData;
        public List<ArduinoInputData> InputDataList {get{return m_InputData;}}

        public List<ArduinoOutputData> m_OutputData;
        public List<ArduinoOutputData> OutputDataList {get{return m_OutputData;}}



        void OnEnable()
        {
            if(m_InputData == null)
                m_InputData = new List<ArduinoInputData>();

            if(m_OutputData == null)
                m_OutputData = new List<ArduinoOutputData>();
        }

        public ArduinoInputData GetInputData(string name)
        {
            int index = m_InputData.FindIndex(0, x => x.inputName == name);

            if(index == -1)
            {
                Debug.LogError("Key not in input dictionary!");
                return new ArduinoInputData();
            }

            return m_InputData[index];
        }

        public void CreateNewInput()
        {
            ArduinoInputData newData = new ArduinoInputData();

            newData.inputName = System.DateTime.Now.ToLongTimeString();
            newData.dataIndex = m_InputData.Count;

            m_InputData.Add(newData);
        }

        public void CreateNewOutput()
        {
            ArduinoOutputData newData = new ArduinoOutputData();

            newData.outputName = System.DateTime.Now.ToLongTimeString();
            newData.dataIndex = m_OutputData.Count;

            m_OutputData.Add(newData);
        }

        public void RemoveInput(int index)
        {
            m_InputData.RemoveAt(index);

            for(int i = 0; i < m_InputData.Count; i++)
            {
                m_InputData[i].dataIndex = i;
            }
        }

        public void RemoveOutput(int index)
        {
            m_OutputData.RemoveAt(index);

            for(int i = 0; i < m_OutputData.Count; i++)
            {
                m_OutputData[i].dataIndex = i;
            }
        }
    }
}