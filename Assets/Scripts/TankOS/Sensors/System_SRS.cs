using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using System;
using System.Linq;

public class System_SRS : MonoBehaviour
{
    private Timer m_readTimer;
    public static event Action EOnSensorsUpdated;
    private int m_avgListLength = 20;
    
    //Temperature
    private static float m_temperature;
    public static float Temperature { get {return m_temperature;}}
    private static float m_maxTemp = -62f;
    private static float m_minTemp = -62f;
    private static List<float> m_avgTempList = new List<float>(20);
    private static float m_avgTemp;

    //Radiation
    private static float m_cumlativeRadiation;
    private static float m_maxRad;
    private static List<float> m_avgRad = new List<float>(20);
    private static float m_currentRad;
    public static float Radiation {get{return m_currentRad;}}

    //Magnetic
    private static float m_currentMag;
    public static float Magnetic {get{return m_currentMag;}}
    private static float m_maxMag;

    void OnEnable()
    {
        m_readTimer = Timer.Register(0.25f, () => ReadSensors(), isLooped: true);
    }

    void ReadSensors()
    {
        ReadTemperature();
        ReadMagnetic();
        ReadRadiation();

        EOnSensorsUpdated?.Invoke();
    }

    void ReadTemperature()
    {
        m_temperature = TemperatureSim.ReadTemperatureFromLocation(transform.position);
        m_avgTempList.Add(m_temperature);

        if(m_avgTempList.Count > m_avgListLength)
        {
            m_avgTempList.RemoveAt(0);
        }

        m_avgTemp = Queryable.Average(m_avgTempList.AsQueryable());

        if(m_temperature > m_maxTemp)
            m_maxTemp = m_temperature;
        
        if(m_temperature < m_minTemp)
            m_minTemp = m_temperature;
    }

    void ReadRadiation()
    {
        m_currentRad = RadiationSim.ReadRadiationFromLocation(transform.position);
    }

    void ReadMagnetic()
    {
        m_currentMag = MagneticSim.ReadMagneticFromLocation(transform.position);
    }
}
