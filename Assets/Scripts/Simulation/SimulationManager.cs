using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Rover.Settings;
using UnityEngine;

public class SimulationManager
{
    private List<SimulationNodeBase> m_simulationNodes = new List<SimulationNodeBase>();
    private List<SimulationNodeBase> m_nearestSimNodes = new List<SimulationNodeBase>();
    private int currNodeCount = 0;
    private float m_backgroundValMin = 0f;
    private float m_backgroundValMax = 0f;
    private int m_currentStepCount = 0;
    private int m_numOfNodesPerFrame = 25;
    private int m_maxStepCount;

    public SimulationManager(float valMin, float valMax)
    {
        m_backgroundValMin = valMin;
        m_backgroundValMax = valMax;
    }

    public bool AddNode(SimulationNodeBase newNode)
    {
        m_simulationNodes.Add(newNode);

        m_maxStepCount = Mathf.CeilToInt((float)m_simulationNodes.Count/(float)m_numOfNodesPerFrame);
        Debug.Log(m_maxStepCount);

        return true;
    }

    public bool RemoveNode(SimulationNodeBase oldNode)
    {
        int nodeIdx = m_simulationNodes.FindIndex((x) => (x == oldNode));

        if(nodeIdx < 0)
            return false;

        m_simulationNodes[nodeIdx] = null;
        currNodeCount = nodeIdx;

        Debug.Log("Node was removed from the simulation manager");

        return true;
    }

    public float ReadSimulationValue(Vector3 location)
    {
        location.y = 0;
        float backgroundTemp = UnityEngine.Random.Range(m_backgroundValMin, m_backgroundValMax);
        float nodeCount = 0;
        float nodeTempFull = 0;

        for(int i = 0; i < m_nearestSimNodes.Count; i++)
        {
            SimulationNodeBase node = m_nearestSimNodes[i];

            //Calculate the distance between the point and the location of the sensor. Invert it.
            float tmp = Mathf.Pow(-Vector3.Distance(location, node.Location)/100f, 2f) + 1f;
            nodeCount += tmp/2f;
            //Calculate the weighted value of the temperature of that node 
            tmp *= node.NodeValue;

            nodeTempFull += tmp;
        }

        if (nodeCount > 0)
            return (nodeTempFull / nodeCount) + backgroundTemp;
        else
            return backgroundTemp;
    }

    public void UpdateNearestNodes(Vector3 location)
    {
        location.y = 0;

        for(int i = m_currentStepCount * m_numOfNodesPerFrame; i < (m_currentStepCount * m_numOfNodesPerFrame) + m_numOfNodesPerFrame; i++)
        {
            if(i >= m_simulationNodes.Count)
                break;

            SimulationNodeBase node = m_simulationNodes[i];
            int idx = m_nearestSimNodes.FindIndex((x) => (x == node));


            if (Vector3.SqrMagnitude(location - node.Location) > 100f * 100f)
            {   
                if(idx != -1)
                    m_nearestSimNodes.RemoveAt(idx);
                
                continue;
            }

            if(idx != -1)
            {
                // Debug.Log("This node is already in the code");
                return;
            }

            m_nearestSimNodes.Add(node);
        }

        m_currentStepCount++;

        if(m_currentStepCount == m_maxStepCount)
        {
            m_currentStepCount = 0;
        }
    }
}

