using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticSimNode : SimulationNodeBase
{
    public MagneticSimNode(float value, Vector3 location) : base (value, location)
    {
        MagneticSim.AddMagneticNode(this);  
    }
}

public static class MagneticSim
{
    private static List<MagneticSimNode> m_mangeticNodes = new List<MagneticSimNode>();
    public static List<MagneticSimNode> TemperatureNodes { get { return m_mangeticNodes; } }
    private static float backgroundTempAvg = 0f;
    public static void AddMagneticNode(MagneticSimNode node)
    {
        m_mangeticNodes.Add(node);
        Debug.LogError("Node reigstered");
    }

    public static float ReadMagneticFromLocation(Vector3 location)
    {
        int nodeCount = 0;
        float nodeTempFull = 0f;
        location.y = 0;
        float backgroundTemp = Random.Range(backgroundTempAvg - 0.01f, backgroundTempAvg + 0.01f);

        foreach (MagneticSimNode node in m_mangeticNodes)
        {
            if (Vector3.Distance(location, node.Location) > node.NodeValue/2f)
                continue;

            //Calculate the distance between the point and the location of the sensor. Invert it.
            float tmp = 1 - Vector3.Distance(location, node.Location) /( node.NodeValue/2f);
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
