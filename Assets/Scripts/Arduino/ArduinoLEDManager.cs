using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Uduino;
using UnityTimer;

namespace Rover.Arduino
{
    public static class LEDManager
    {
        private static object[] m_ledPinStates;
        private static bool m_pinStatesUpdated = false;
        private static bool applicationQuitting = false;

        static LEDManager()
        {

        }

        public static void InitializeLEDManager(object[] outputPinList)
        {   
            m_ledPinStates = new object[(int)outputPinList[0]];
            //Starting from 1 because the first value in the array is the length of the array.
            for(int i = 0; i < m_ledPinStates.Length; i++)
            {
                m_ledPinStates[i] = 0;
            }

//            Debug.LogError(m_ledPinStates.Length);
            
            //Timer.Register(0.1f, () => SendLEDCommand(), isLooped: true);
        }

        private static void SendLEDCommand()
        {
            if(UduinoManager.Instance.isConnected() && m_pinStatesUpdated)
            {
                m_pinStatesUpdated = false;
                UduinoManager.Instance.sendCommand("led", m_ledPinStates);
            }
        }

        public static void SetLEDMode(int pinIndex, int value)
        {
            if (applicationQuitting)
                return;

            m_ledPinStates[pinIndex] = value;

            m_pinStatesUpdated = true;
            SendLEDCommand();
        }

        public static void SetLEDMode(int[] pinIndexes, int[] values)
        {
            if (applicationQuitting)
                return;

            if (pinIndexes.Length != values.Length)
            {
                Debug.LogError("LEDManager: Pin array and value array are not the same length!");
                return;
            }

            for (int i = 0; i < pinIndexes.Length; i++)
            {
                m_ledPinStates[pinIndexes[i]] = values[i];
            }

            m_pinStatesUpdated = true;

            SendLEDCommand();
        }

        public static bool GetLEDState(int pinIndex)
        {
            return (int)m_ledPinStates[pinIndex] == 1 ? true: false;
        }
    }
}

