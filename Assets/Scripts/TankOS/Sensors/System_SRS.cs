using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using System;

public class System_SRS : MonoBehaviour
{
    private Timer m_readTimer;
    public static event Action EOnSensorsUpdated;
    private static float m_temperature;
    public static float Temperature { get {return m_temperature;}}
    void OnEnable()
    {
        m_readTimer = Timer.Register(0.25f, () => ReadSensors(), isLooped: true);
    }

    void ReadSensors()
    {
        m_temperature = TemperatureSim.ReadTemperatureFromLocation(transform.position);

        EOnSensorsUpdated?.Invoke();
    }
}
