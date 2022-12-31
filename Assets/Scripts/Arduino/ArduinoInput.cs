using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using System;
using UnityTimer;

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

    public static class ArduinoInputDatabase
    {
        private static List<ArduinoInput> m_arduinoInputs = new List<ArduinoInput>();
        public static List<ArduinoInput> ArduinoInputs { get {return m_arduinoInputs;} }
        private static ArduinoInputActionMap m_InputActionMap;
        private static Timer inputUpdateTimer;
        public static event Action EOnDatabasedInitialized;
        public const float INPUT_TIMER_DELAY = 0.05f;

        static ArduinoInputDatabase()
        {

        }

        public static void InitializeDatabase()
        {
            UduinoManager.Instance.OnBoardConnected += OnBoardConnected;
            m_InputActionMap = Resources.Load<ArduinoInputActionMap>("RoverInputActions");
            Debug.LogError(m_InputActionMap);
        }

        private static void OnBoardConnected(UduinoDevice device)
        {
            InitializeArduinoData();
        }

        private static void InitializeArduinoData()
        {
            List<object> digitalPinData = new List<object>();
            List<object> analogPinData = new List<object>();
            List<object> outputPinData = new List<object>();
            int digitalPinCount = 0;
            int analogPinCount = 0;
            int outputPinCount = 0;

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

            foreach(ArduinoOutputData data in m_InputActionMap.OutputDataList)
            {
                outputPinCount++;
                outputPinData.Add(data.pinNumber);
            }
            
            digitalPinData.Insert(0, digitalPinCount);
            analogPinData.Insert(0, analogPinCount);
            outputPinData.Insert(0, outputPinCount);

            UduinoManager.Instance.sendCommand("initD", digitalPinData.ToArray());
            
            //We need a timer here to prevent sending too much data to the arduino in the same frame. 
            //If we do that, it will overflow the timer and we won't be able to read the data properly.
            Timer.Register(0.1f, () => { 
                    UduinoManager.Instance.sendCommand("initA", analogPinData.ToArray());
                    
                    Timer.Register(0.1f, () => {
                        UduinoManager.Instance.sendCommand("initO", outputPinData.ToArray());
                        
                        ArduinoInputDecoder.InitializedInputDecoder(analogPinCount + 1);
                        LEDManager.InitializeLEDManager(outputPinData.ToArray());

                        EOnDatabasedInitialized?.Invoke();
                        Debug.Log("Database initd");


                        // string disp = "";
                        // for(int i = 0; i< digitalPinData.Count; i++)
                        // {
                        //     disp += digitalPinData[i];
                        //     disp += " ";
                        // }
                        // Debug.LogError(disp + " | Count: " + disp.Length);

                        inputUpdateTimer = Timer.Register(INPUT_TIMER_DELAY, () => {OnInputReadTimerUpdate();}, isLooped: true);
                    });
                } );

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
            // if(!Rover.OS.OperatingSystem.AllowUserControl)
            //     return;

            foreach(ArduinoInput input in m_arduinoInputs)
            {
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
        private static List<string> m_lastMessage = new List<string>();
        public static List<string> LastMessage { get {return m_lastMessage;} }
        private static bool m_readMessage = false;
        static ArduinoInputDecoder()
        {
            UduinoManager.Instance.OnDataReceived += OnMessageReceived;
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
            if(!m_readMessage)
                return;

            if(data[0] != '_' || m_lastMessage.Count == 0)
                return;
                
            data = data.Remove(0,1);

            string[] split = data.Split(' ');

            for(int i = 0; i<split.Length;i++)
            {
                m_lastMessage[i] = split[i];
            }

            EOnSerialMessageRecieved?.Invoke(data);
        }
    }
}
