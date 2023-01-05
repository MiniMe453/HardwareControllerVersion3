using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using TMPro;

public class SimpleCountdownTimer : MonoBehaviour
{

    public int startHours;
    public int startMinutes;
    public int startSeconds;
    public int totalTime;
    public TextMeshProUGUI timerText;

    void OnEnable()
    {
        Timer.Register(1f, () => OnTimerTicked(), isLooped: true);

        totalTime = ((startHours*60)*60) + (startMinutes * 60) + startSeconds;
    }

    void OnTimerTicked()
    {
        totalTime--;
        string timerStr = "";

        int hours = Mathf.FloorToInt( Mathf.FloorToInt(totalTime / 60) / 60 );
        int minutes = Mathf.FloorToInt((totalTime - ((hours * 60)*60))/60);
        int seconds = totalTime - ((hours * 60 * 60) + (minutes * 60));

        timerStr = "-" + hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");

        timerText.text = timerStr;
    }

}
