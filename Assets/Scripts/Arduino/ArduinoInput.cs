using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using System;
using UnityTimer;
using System.IO.Ports;
using UnityEngine.SceneManagement;

namespace Rover.Arduino
{
    public enum InputType {Digital, Analog};
    public enum ArduinoPinMode {INPUT, OUTPUT, INPUT_PULLUP};
    public class ArduinoInput
    {
        public event Action<float, int> EOnValueChanged;
        public event Action<int> EOnButtonPressed;
        public event Action<int> EOnButtonReleased;
        public event Action<int> EOnButtonHeld;
        private InputType m_inputType;
        public InputType InputType {get {return m_inputType;} }
        private float m_oldValue = -999;
        private int m_pin;
        private int m_id;
        private string m_inputName;
        public string InputName {get {return m_inputName;} }
        private bool m_inputEnabled = true;
        private bool m_canHoldButton = false;
        private float m_holdDuration;
        private float m_holdTimer;
        private float m_timeSinceStartHold;
        private bool m_holdStarted = false;
        private bool m_used = false;

        public ArduinoInput(InputType type, int pin, int id, bool buttonHold = false, float holdTimer = 1f, string name = "Arduino Input")
        {
            m_pin = pin;
            m_id = id;
            m_inputType = type;
            m_inputName = name;
            m_canHoldButton = buttonHold;
            m_holdDuration = holdTimer;
        }

        public ArduinoInput(InputType type, int pin, int id, string name = "Arduino Input")
        {
            m_pin = pin;
            m_id = id;
            m_inputType = type;
            m_inputName = name;
            m_canHoldButton = false;
            m_holdDuration = 0;

            Debug.Log("Input created " + m_inputName);
        }

        public void CheckInputValue()
        {   
            if(!m_inputEnabled || !UduinoManager.Instance.hasBoardConnected())
                return;

            switch(m_inputType)
            {
                case InputType.Digital:
                    ReadDigitalInput();
                    break;
                case InputType.Analog:
                    ReadAnalogInput();
                    break;
            }
        }

        public void EnableInput()
        {
            m_inputEnabled = true;
        }

        public void DisableInput()
        {
            m_inputEnabled = false;
        }

        private void ReadDigitalInput()
        {
            if(ArduinoInputDecoder.LastMessage.Count <= 0)
                return;

            if(m_id > ArduinoInputDecoder.LastMessage[0].Length - 1)
            {
                Debug.LogError("Input with ID " + m_id + " exceeds last message length " + ArduinoInputDecoder.LastMessage);
                return;
            }

            float currentValue = float.Parse(ArduinoInputDecoder.LastMessage[0][m_id].ToString());

            // if(currentValue == 1f && !m_buttonHoldStarted)
            // {
            //     m_buttonHoldStarted = true;
            // }
            // else if (m_buttonHoldStarted)
            // {
            //     m_buttonHoldStarted = false;
            // }

            // if(m_buttonHoldStarted)
            // {
            //     m_holdTimer += Time.deltaTime;

            //     if(m_holdTimer > m_holdDuration)
            //     {
            //         EOnButtonHeld?.Invoke(m_pin);
            //         return;
            //     }
            // }

            if(currentValue != m_oldValue)
            {
                if(currentValue == 1f)
                {
                    EOnButtonPressed?.Invoke(m_pin);
                    m_timeSinceStartHold = Time.time;    
                    m_holdStarted = true;
                }
                else
                {
                    EOnButtonReleased?.Invoke(m_pin);
                    m_holdStarted = false;
                }

                m_oldValue = currentValue;
            }

            if(m_canHoldButton && currentValue == 1f)
            {
                if(Time.time - m_timeSinceStartHold > m_holdDuration && m_holdStarted)
                {
                    m_holdTimer = 0;
                    EOnButtonHeld?.Invoke(m_pin);
                    m_holdStarted = false;
                    return;
                }
            }
        }

        private void ReadAnalogInput()
        {
            if(ArduinoInputDecoder.LastMessage.Count <= 0)
                return;

            float currentValue = float.Parse(ArduinoInputDecoder.LastMessage[m_id].ToString());

            if(currentValue != m_oldValue)
            {
                EOnValueChanged?.Invoke(currentValue, m_pin);
                m_oldValue = currentValue;
            }                
        }
    }

    public class ThreeWaySwitch
    {
        private static int m_currentSelection = 0;
        public static int CurrentValue {get {return m_currentSelection;}}
        public static event Action<int> EOnCurrentSelectionChanged;
        private int[] m_mvgAvgFilter = new int[5];
        private float[] m_mvgAvgFilterWeights = new float[] {0.4f, 0.3f, 0.2f, 0.05f, 0.05f};
        private int m_avgValue = 0;

        public ThreeWaySwitch()
        {
            ArduinoInputDatabase.GetInputFromName("3Way Switch").EOnValueChanged += OnValueChanged;
        }

        void OnValueChanged(float value, int pin)
        {
            int avgMaxCount = 0;

            // for(int i = 0; i < m_mvgAvgFilter.Length; i++)
            // {
            //     if(i == 0)
            //         continue;

            //     m_mvgAvgFilter[i - 1] = m_mvgAvgFilter[i];
            //     avgMaxCount += m_mvgAvgFilter[i - 1];

            //     if(i == m_mvgAvgFilter.Length - 1)
            //     {
            //         m_mvgAvgFilter[i] = Mathf.CeilToInt(value);
            //         avgMaxCount += m_mvgAvgFilter[i];
            //     }
            // }

            // m_avgValue = avgMaxCount / 5;

            // if(m_avgValue < 75 && m_currentSelection != 2)
            // {
            //     m_currentSelection = 2;
            //     EOnCurrentSelectionChanged?.Invoke(m_currentSelection);
            // }
            // else if (m_avgValue > 600 && value < 700 && m_currentSelection != 1)
            // {
            //     m_currentSelection = 1;
            //     EOnCurrentSelectionChanged?.Invoke(m_currentSelection);
            // }
            // else if(m_avgValue > 975 && m_currentSelection != 0)
            // {
            //     m_currentSelection = 0;
            //     EOnCurrentSelectionChanged?.Invoke(m_currentSelection);
            // }

            if(value < 150 && m_currentSelection != 2)
            {
                m_currentSelection = 2;
                EOnCurrentSelectionChanged?.Invoke(m_currentSelection);
            }
            else if((value > 300 && value < 850) && m_currentSelection != 1)
            {
                m_currentSelection = 1;
                EOnCurrentSelectionChanged?.Invoke(m_currentSelection);
            }
            else if(value >875 && m_currentSelection != 0)
            {
                m_currentSelection = 0;
                EOnCurrentSelectionChanged?.Invoke(m_currentSelection);
            }
        }
    }

    public static class ArduinoInputDatabase
    {
        private static List<ArduinoInput> m_arduinoInputs = new List<ArduinoInput>();
        public static List<ArduinoInput> ArduinoInputs { get {return m_arduinoInputs;} }
        private static ArduinoInputActionMap m_InputActionMap;
        private static Timer inputUpdateTimer;
        public static event Action EOnDatabasedInitialized;
        private static bool m_databaseInitialized = false;
        public const float INPUT_TIMER_DELAY = 0.05f;
        public static ThreeWaySwitch threeWaySwitch;
        private static string[] m_inputsToSkipWhenNoControl = new string[] {"Joystick X", "Joystick Y", "Push Potentiometer"};
        
        #region Arduino Setup Variables
        private static int setupMessagesSendCount;
        private static int setupMessagesEndCount;
        private static Timer sendSetupMessagesTimer;
        static List<object> digitalPinData = new List<object>();
        static List<object> analogPinData = new List<object>();
        static List<object[]> outputPinData = new List<object[]>();
        static int outputArrayIndexToSend = 0;
        #endregion

        static ArduinoInputDatabase()
        {

        }

        public static void InitializeDatabase()
        {
            UduinoManager.Instance.OnBoardConnected += OnBoardConnected;
            m_InputActionMap = Resources.Load<ArduinoInputActionMap>("RoverInputActions");
            Debug.LogError(m_InputActionMap);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            if(!m_databaseInitialized)
                return;

            Timer.Register(0.1f, () => {EOnDatabasedInitialized?.Invoke(); Debug.LogError("InputDatabase initialized");}); 
        }

        private static void OnBoardConnected(UduinoDevice device)
        {
            InitializeArduinoData();
        }

        private static void InitializeArduinoData()
        {

            int outputDataListArrayIndex = 0;
            int digitalPinCount = 0;
            int analogPinCount = 0;

            m_arduinoInputs.Clear();

            foreach(ArduinoInputData data in m_InputActionMap.InputDataList)
            {
                if(data.inputIndex == -1)
                    continue;

                //Initialize the software values
                ArduinoInput newInput = new ArduinoInput(data.inputType, data.pinNumber, data.inputIndex, data.inputName);
                m_arduinoInputs.Add(newInput);

                if(data.pinNumber == -1)
                    continue;

                switch(data.inputType)
                {
                    case InputType.Digital:
                        digitalPinCount++;
                        digitalPinData.Add(data.pinNumber);
                        //digitalPinData.Add((int)data.pinMode);
                        break;
                    case InputType.Analog:
                        analogPinCount++;
                        analogPinData.Add(data.pinNumber);
                        //analogPinData.Add((int)data.pinMode);
                        break;
                }
            }

            //Create the arrays inside the list and set their initial values so we know how much we have in each array.
            for(int i = 0; i < Mathf.CeilToInt(m_InputActionMap.OutputDataList.Count/12f); i++)
            {
                outputPinData.Add(new object[((i + 1) * 12 < m_InputActionMap.OutputDataList.Count? 12 : m_InputActionMap.OutputDataList.Count % 12) + 3]);

                outputPinData[i][0] = i == 0? m_InputActionMap.OutputDataList.Count : -1;
                outputPinData[i][1] = i * 12;
                outputPinData[i][2] = outputPinData[i].Length - 3;

                Debug.LogError(outputPinData[i].Length);

            }

            for(int i = 0; i < m_InputActionMap.OutputDataList.Count; i++)
            {
                ArduinoOutputData outputData = m_InputActionMap.OutputDataList[i];

                outputPinData[Mathf.FloorToInt(i/12)][(i % 12) + 3] = outputData.pinNumber;
            }

            foreach(ArduinoOutputData data in m_InputActionMap.OutputDataList)
            {
                if(outputPinData[outputDataListArrayIndex].Length == 15)
                {                    
                    outputDataListArrayIndex++;
                }

                
            }
            
            digitalPinData.Insert(0, digitalPinCount);
            analogPinData.Insert(0, analogPinCount);

            sendSetupMessagesTimer = Timer.Register(0.1f, () => SendSetupMessages(), isLooped: true);
            setupMessagesEndCount = outputPinData.Count + 2;
        }

        static void SendSetupMessages()
        {   
            bool stopSendingSetupMessages = false;
            
            switch(setupMessagesSendCount)
            {
                case 0:
                    UduinoManager.Instance.sendCommand("initD", digitalPinData.ToArray());
                    setupMessagesSendCount++;
                    break;
                case 1:
                    UduinoManager.Instance.sendCommand("initA", analogPinData.ToArray());
                    setupMessagesSendCount++;
                    break;
                case 2: 
                    if(outputArrayIndexToSend == outputPinData.Count)
                    {
                        stopSendingSetupMessages = true;
                        break;
                    }
                    
                    UduinoManager.Instance.sendCommand("initO", outputPinData[outputArrayIndexToSend]);
                    outputArrayIndexToSend++;
                    break;
            }

            if(!stopSendingSetupMessages)
                return;

            ArduinoInputDecoder.InitializedInputDecoder((int)analogPinData[0] + 2);
            LEDManager.InitializeLEDManager((int)outputPinData[0][0]);

            Debug.Log("Database initd");

            inputUpdateTimer = Timer.Register(INPUT_TIMER_DELAY, () => {OnInputReadTimerUpdate();}, isLooped: true);

            threeWaySwitch = new ThreeWaySwitch();
            sendSetupMessagesTimer.Cancel();

            EOnDatabasedInitialized?.Invoke();
            m_databaseInitialized = true;
        }


        public static ArduinoInput GetInputFromName(string name)
        {
            int index = m_arduinoInputs.FindIndex(0, input => input.InputName == name);

            if(index == -1)
            {
                Debug.LogError($"Input with name {name} not found in m_inputList!");
                return null;
            }

            return m_arduinoInputs[index];
        }

        public static int GetOutputIndexFromName(string name)
        {
            int index = m_InputActionMap.OutputDataList.FindIndex(0, output => output.outputName == name);

            if(index == -1)
            {
                Debug.LogError($"Output with name {name} not found in m_OutputDataList");
                return -1;
            }

            return index;
        }

        private static void OnInputReadTimerUpdate()
        {
            if(!RoverOperatingSystem.ArduinoInputEnabled)
                return;

            bool skipInput = false;

            foreach(ArduinoInput input in m_arduinoInputs)
            {
                skipInput = !RoverOperatingSystem.AllowUserControl && Array.FindIndex(m_inputsToSkipWhenNoControl, 0, x => x == input.InputName) != -1;
                    
                if(!skipInput)
                    input.CheckInputValue();
            }
        }

        public static void RegisterInput(ArduinoInput input)
        {
            if(m_arduinoInputs.IndexOf(input) == -1)
            {
                m_arduinoInputs.Add(input);
            }
        }
    }

    public static class ArduinoInputDecoder
    {
        public static event Action<string> EOnSerialMessageRecieved;
        public static event Action EOnPrinterFinishedMessageReceived;
        private static List<string> m_lastMessage = new List<string>();
        public static List<string> LastMessage { get {return m_lastMessage;} }
        private static bool m_readMessage = false;
        private static SerialPort m_serial = null;
        static ArduinoInputDecoder()
        {
            UduinoManager.Instance.OnDataReceived += OnMessageReceived;
           // Timer.Register(1f/60f, () => ReadTest(), isLooped: true);
        }

        private static void ReadTest()
        {
//            string data = (UduinoManager.Instance.uduinoDevices["RoverController"] as UduinoDevice_DesktopSerial).serial.ReadLine();
     //       ParseInputData(data);
        }

        public static void InitializedInputDecoder(int numOfInputs)
        {
            m_lastMessage.Clear();

            for(int i = 0; i <= numOfInputs; i++)
            {
                m_lastMessage.Add("");
            }

            m_readMessage = true;
        }

        private static void OnMessageReceived(string data, UduinoDevice device)
        {
            if(data == "prt_finished")
            {
                EOnPrinterFinishedMessageReceived?.Invoke();
                return;
            }

            ParseInputData(data);
        }

        private static void ParseInputData(string data)
        {
            if(!m_readMessage)
                return;

            if(data[0] != '_' || m_lastMessage.Count == 0)
                return;
                
            data = data.Remove(0,1);

            string[] split = data.Split(' ');

            for(int i = 0; i < split.Length;i++)
            {
                m_lastMessage[i] = split[i];
            }

            EOnSerialMessageRecieved?.Invoke(data);
        }
    }
}
