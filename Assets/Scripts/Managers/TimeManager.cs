using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using System;

namespace Rover.DateTime
{
    public struct DateTimeStruct
    {
        public int Years;
        public int Days;
        public int Hours;
        public int Minutes;
        public int Seconds;

    }
    public static class TimeManager
    {
        private static float m_timeScale = 1f;
        public static float TimeScale { get { return m_timeScale; } set { m_timeScale = value; } }
        public static DateTimeStruct dateTime;
        public static event Action<DateTimeStruct> EOnDateTimeUpdated;

        static TimeManager()
        {
            Timer.Register(1f, () => UpdateTime(), isLooped: true);

            DateTimeStruct tmp = new DateTimeStruct();
            tmp.Years = 5;
            tmp.Days = 167;
            tmp.Hours = 8;
            tmp.Minutes = 34;
            tmp.Seconds = 15;

            dateTime = tmp;
        }

        private static void UpdateTime()
        {
            DateTimeStruct tmp = dateTime;

            tmp.Seconds++;

            if (tmp.Seconds == 60)
            {
                tmp.Seconds = 0;
                tmp.Minutes++;
                if (tmp.Minutes == 60)
                {
                    tmp.Minutes = 0;
                    tmp.Hours++;
                    if (tmp.Hours == 23)
                    {
                        tmp.Hours = 0;
                        tmp.Days++;
                        if (tmp.Days == 365)
                        {
                            tmp.Days = 0;
                            tmp.Years++;
                        }
                    }
                }
            }

            dateTime = tmp;

            EOnDateTimeUpdated?.Invoke(dateTime);
        }

        public static string TimeToStringFull(DateTimeStruct dateTimeStruct)
        {
            string year = dateTimeStruct.Years.ToString("00");
            string day = dateTimeStruct.Days.ToString("000");
            string hour = dateTimeStruct.Hours.ToString("00");
            string minute = dateTimeStruct.Minutes.ToString("00");
            string seconds = dateTimeStruct.Seconds.ToString("00");

            return year + "y, " + day + "d, " + hour + ":" + minute + ":" + seconds;
        }

        public static string TimeToStringYD(DateTimeStruct dateTimeStruct)
        {
            string year = dateTimeStruct.Years.ToString("00");
            string day = dateTimeStruct.Days.ToString("000");

            return year + "y, " + day + "d";
        }

        public static string TimeToStringTime(DateTimeStruct dateTimeStruct, string splitChar = ":")
        {
            string hour = dateTimeStruct.Hours.ToString("00");
            string minute = dateTimeStruct.Minutes.ToString("00");
            string seconds = dateTimeStruct.Seconds.ToString("00");

            return hour + splitChar + minute + splitChar + seconds;
        }

        public static DateTimeStruct GetCurrentDateTime()
        {
            return dateTime;
        }
    }
}
