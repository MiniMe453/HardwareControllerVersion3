using System;
using System.Linq;
using Rover.Settings;
using UnityEngine;

public class SimulationManager
{
    private SimulationNodeMonobehaviour[] activeSimulationNodes = new SimulationNodeMonobehaviour[GameSettings.SIMULATION_NODE_ARRAY_SIZE];
    private int currNodeCount = 0;
    private float backgroundValMin = 0f;
    private float backgroundValMax = 0f;

    public SimulationManager(float valMin, float valMax)
    {
        backgroundValMin = valMin;
        backgroundValMax = valMax;
    }

    public  bool AddNode(SimulationNodeMonobehaviour newNode)
    {
        if(currNodeCount == GameSettings.SIMULATION_NODE_ARRAY_SIZE)
        {    
            return false;
        }

        activeSimulationNodes[currNodeCount] = newNode;

        currNodeCount++;


        Debug.Log("Node was added to the simulation manager");

        return true;
    }

    public bool RemoveNode(SimulationNodeMonobehaviour oldNode)
    {
        int nodeIdx = Array.IndexOf(activeSimulationNodes, oldNode);

        if(nodeIdx < 0)
            return false;

        activeSimulationNodes[nodeIdx] = null;
        currNodeCount = nodeIdx;

        Debug.Log("Node was removed from the simulation manager");

        return true;
    }

    public float ReadSimulationValue(Vector3 location)
    {
        float simValue = 0f;
        int nodeCount = 0;
        location.y = 0;

        float backgroundTemp = UnityEngine.Random.Range(backgroundValMin, backgroundValMax);

        for(int i = 0; i < activeSimulationNodes.Length; i++)
        {
            if(activeSimulationNodes[i] == null)
                continue;

            SimulationNodeMonobehaviour node = activeSimulationNodes[i];
            Vector3 nodeLoc = node.transform.position;
            nodeLoc.y = 0;

            float tmp = Mathf.Clamp01(1 - (Vector3.Distance(location, nodeLoc) / GameSettings.SIMULATION_NODE_RADIUS));
            
            // Debug.Log(tmp);
            //Calculate the weighted value of the temperature of that node 
            tmp *= node.nodeValue;

            nodeCount++;
            simValue += tmp;
        }

        if (nodeCount > 0)
            return (simValue / nodeCount) + backgroundTemp;
        else
            return backgroundTemp;
    }
}

