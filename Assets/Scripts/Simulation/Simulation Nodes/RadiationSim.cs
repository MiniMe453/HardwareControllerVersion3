using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiationSimNode : SimulationNodeBase
{
    public RadiationSimNode(float value, Vector3 location) : base (value, location)
    {
        RadiationSim.AddRadiationNode(this);
    }
}

public static class RadiationSim
{
    private static List<RadiationSimNode> m_radiationNOdes = new List<RadiationSimNode>();
    public static List<RadiationSimNode> TemperatureNodes { get { return m_radiationNOdes; } }
    private static float backgroundTempAvg = .0001f;
    public static void AddRadiationNode(RadiationSimNode node)
    {
        m_radiationNOdes.Add(node);
        Debug.LogError("Node reigstered");
    }

    public static float ReadRadiationFromLocation(Vector3 location)
    {
        int nodeCount = 0;
        float nodeTempFull = 0f;
        location.y = 0;
        float backgroundTemp = Random.Range(backgroundTempAvg - 0.0001f, backgroundTempAvg + 0.0001f);

        foreach (RadiationSimNode node in m_radiationNOdes)
        {
            if (Vector3.Distance(location, node.Location) > node.NodeValue)
                continue;

            //Calculate the distance between the point and the location of the sensor. Invert it.
            float tmp = 1 - Vector3.Distance(location, node.Location) / node.NodeValue;
            //Calculate the weighted value of the temperature of that node 
            tmp *= node.NodeValue;

            nodeCount++;
            nodeTempFull += tmp;

            
            if(nodeCount > 5)
                break;
        }

        if (nodeCount > 0)
            return (nodeTempFull / nodeCount) + backgroundTemp;
        else
            return backgroundTemp;
    }
}

