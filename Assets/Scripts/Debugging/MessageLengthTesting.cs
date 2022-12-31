using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using Uduino;

public class MessageLengthTesting : MonoBehaviour
{
    private int counter;
    private string stringToSend = "";
    private object[] data = new object[1];
    void OnEnable()
    {
        UduinoManager.Instance.OnBoardConnected += OnBoardConnected;
    }


    void OnBoardConnected(UduinoDevice device)
    {
        Timer.Register(1f, () => SendTestCommand(), isLooped: true);
    }

    void SendTestCommand()
    {
        stringToSend += "A";
        data[0] = stringToSend;

        UduinoManager.Instance.sendCommand("doNth", data);
    }
}
