using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureSimNode : SimulationNodeBase
{
    public TemperatureSimNode(float value, Vector3 location) : base (value, location)
    {
        TemperatureSim.AddTemperatureNode(this);
    }
}

public static class TemperatureSim
{
    private static List<TemperatureSimNode> m_temperatureNodes = new List<TemperatureSimNode>();
    public static List<TemperatureSimNode> TemperatureNodes { get { return m_temperatureNodes; } }
    private static float backgroundTempAvg = -62f;
    public static void AddTemperatureNode(TemperatureSimNode node)
    {
        m_temperatureNodes.Add(node);
        Debug.LogError("Node reigstered");
    }

    public static float ReadTemperatureFromLocation(Vector3 location)
    {
        int nodeCount = 0;
        float nodeTempFull = 0f;
        location.y = 0;
        float backgroundTemp = Random.Range(backgroundTempAvg - 0.5f, backgroundTempAvg + 0.5f);

        foreach (TemperatureSimNode node in m_temperatureNodes)
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
