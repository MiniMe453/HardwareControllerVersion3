using System.Collections;
using System.Collections.Generic;
using Rover.Settings;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SimulationNodeMonobehaviour : MonoBehaviour
{
    private SphereCollider triggerSphere => GetComponent<SphereCollider>();
    public float nodeValue = 0f;
    public Simulations simluationType;

    void Awake()
    {
        triggerSphere.radius = GameSettings.SIMULATION_NODE_RADIUS;
        triggerSphere.isTrigger = true;
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.TryGetComponent<SimulationTankMarker>(out SimulationTankMarker tankMarker))
        {
            tankMarker.AddNode(this);
        }
        else
        {
            return;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if(collider.TryGetComponent<SimulationTankMarker>(out SimulationTankMarker tankMarker))
        {
            tankMarker.RemoveNode(this);
        }
        else
        {
            return;
        }
    }
}
