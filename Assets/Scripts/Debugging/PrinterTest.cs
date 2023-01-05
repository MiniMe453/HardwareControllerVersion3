using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using Rover.Arduino;

public class PrinterTest : MonoBehaviour
{
    int m_counter;

    void OnEnable()
    {
        Timer.Register(1f, () => PrintSomething(), isLooped: true);
    }

    void PrintSomething()
    {
        if(!Uduino.UduinoManager.Instance.isConnected())
            return;
        
        m_counter++;

        ArduinoPrinterManager.Instance.SimplePrint(m_counter.ToString("000"));
    }
}
