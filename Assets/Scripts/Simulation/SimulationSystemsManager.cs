using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;
using Rover.Settings;
using UnityEngine;

public class SimulationSystemsManager : MonoBehaviour
{
    private static SimulationManager m_temperatureSim = new SimulationManager(GameSettings.BACKGROUND_TEMP - 0.5f, GameSettings.BACKGROUND_TEMP + 0.5f);
    public static SimulationManager TemperatureSim {get{return m_temperatureSim;}}
    private static SimulationManager m_magneticSim = new SimulationManager(0f, 1f);
    public static SimulationManager MagneticSim {get{return m_magneticSim;}}
    private static SimulationManager m_radiationSim = new SimulationManager(0f,0.0001f);
    public static SimulationManager RadiationSim {get{return m_radiationSim;}}

    public static void AddNodeToSim(SimulationNodeBase newNode)
    {
        switch(newNode.SimulationType)
        {
            case Simulations.Temperature:
                TemperatureSim.AddNode(newNode);
                break;
        }
    }

    public static void RemoveNodeFromSim(SimulationNodeBase oldNode)
    {
        switch(oldNode.SimulationType)
        {
            case Simulations.Temperature:
                TemperatureSim.RemoveNode(oldNode);
                break;
        }
    }

    void Update()
    {
        TemperatureSim.UpdateNearestNodes(System_GPS.WorldSpacePos);
    }
}
