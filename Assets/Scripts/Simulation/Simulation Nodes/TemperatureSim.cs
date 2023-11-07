using System.Collections;
using System.Collections.Generic;
using System.Data.Common.CommandTrees;
using UnityEngine;

// public class TemperatureSimNode : SimulationNodeBase
// {
//     public TemperatureSimNode(float value, Vector3 location) : base (value, location)
//     {
//         TemperatureSim.AddTemperatureNode(this);
//     }
// }

// public static class TemperatureSim
// {
//     private static List<TemperatureSimNode> m_temperatureNodes = new List<TemperatureSimNode>();
//     public static List<TemperatureSimNode> TemperatureNodes { get { return m_temperatureNodes; } }
//     private static float backgroundTempAvg = -62f;
//     private static int maxStepCount = 0;
//     private static int currentStepCount = 0;
//     private static int numOfNodesPerFrame = 25;
//     private static float nodeTempFull;
//     private static float nodeCount;
//     public static void AddTemperatureNode(TemperatureSimNode node)
//     {
//         m_temperatureNodes.Add(node);

//         maxStepCount = (int)Mathf.Ceil((float)m_temperatureNodes.Count/(float)numOfNodesPerFrame);
//     }

//     public static float ReadTemperatureFromLocation(Vector3 location)
//     {
//         location.y = 0;
//         float backgroundTemp = Random.Range(backgroundTempAvg - 0.5f, backgroundTempAvg + 0.5f);

//         for(int i = currentStepCount * numOfNodesPerFrame; i < (currentStepCount * numOfNodesPerFrame) + numOfNodesPerFrame; i++)
//         {
//             if(i >= m_temperatureNodes.Count)
//                 break;

//             TemperatureSimNode node = m_temperatureNodes[i];
//             if (Vector3.Distance(location, node.Location) > node.NodeValue)
//                 continue;

//             //Calculate the distance between the point and the location of the sensor. Invert it.
//             float tmp = 1 - Vector3.Distance(location, node.Location) / node.NodeValue;
//             //Calculate the weighted value of the temperature of that node 
//             tmp *= node.NodeValue;

//             nodeCount++;
//             nodeTempFull += tmp;
//         }

//         currentStepCount++;

//         if(currentStepCount == maxStepCount)
//         {   
//             nodeTempFull = 0;
//             nodeCount = 0;
//             currentStepCount = 0;
//         }

//         if (nodeCount > 0)
//             return (nodeTempFull / nodeCount) + backgroundTemp;
//         else
//             return backgroundTemp;
//     }
// }
