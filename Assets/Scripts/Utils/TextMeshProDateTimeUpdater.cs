using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.DateTime;
using TMPro;

public enum DateTimeDisplayType {IngameDate, MissionClock}
public class TextMeshProDateTimeUpdater : MonoBehaviour
{
    private TextMeshProUGUI m_textMeshPro => GetComponent<TextMeshProUGUI>();
    public DateTimeDisplayType displayType = DateTimeDisplayType.IngameDate;
    void OnEnable()
    {
        TimeManager.EOnDateTimeUpdated += OnNewDateTime;
    }

    void OnNewDateTime(DateTimeStruct newDateTime)
    {
        switch(displayType)
        {
            case DateTimeDisplayType.IngameDate:
                m_textMeshPro.text = TimeManager.ToStringIngameDate + " " + TimeManager.ToStringMissionTimeClk(newDateTime);
                break;
            case DateTimeDisplayType.MissionClock:
                m_textMeshPro.text = TimeManager.ToStringMissionTimeLong(newDateTime);
                break;
        }
    }
}
