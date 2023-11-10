using System.Collections;
using System.Collections.Generic;
using Rover.Settings;
using UnityEngine;

public class SimulationNodeMonobehaviour : MonoBehaviour
{
    public float Temperature;
    private  SimulationNodeBase m_simNode;
    public Simulations simType; 
    void OnEnable()
    {
        m_simNode = new SimulationNodeBase(simType, Temperature, transform.position);
        SimulationSystemsManager.AddNodeToSim(m_simNode);
    }
}
