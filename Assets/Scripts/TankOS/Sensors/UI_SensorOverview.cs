using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Rover.OS;
using UnityTimer;

public class UI_SensorOverview : MonoBehaviour
{
    public MonoBehaviourApplication mainApplication;
    public TextMeshProUGUI tempText;
    public TextMeshProUGUI radText;
    public TextMeshProUGUI magText;
    public TextMeshProUGUI pitchText;
    public TextMeshProUGUI rollText;

    void Start()
    {
        Timer.Register(0.1f, () => UpdateText(), isLooped: true);
    }

    void UpdateText()
    {
        tempText.text = System_SRS.Temperature.ToString("0.0").PadLeft(5) + "°";
        radText.text = System_SRS.Radiation.ToString("0.000") + "uSv";
        magText.text = System_SRS.Magnetic.ToString("0.0").PadLeft(5) + "G";

        pitchText.text = "P:" + SystemPitchRoll.Pitch.ToString("0").PadLeft(4) + "°";
        rollText.text = "R:"+SystemPitchRoll.Roll.ToString("0").PadLeft(4) + "°";
    }
}
