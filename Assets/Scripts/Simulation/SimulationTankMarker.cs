using System.Collections;
using System.Collections.Generic;
using Rover.Settings;
using UnityEngine;

public class SimulationTankMarker : MonoBehaviour
{
    public void AddNode(SimulationNodeMonobehaviour newNode)
    {
        System_SRS.AddNode(newNode);
    }

    public void RemoveNode(SimulationNodeMonobehaviour oldNode)
    {
        System_SRS.RemoveNode(oldNode);
    }
}
