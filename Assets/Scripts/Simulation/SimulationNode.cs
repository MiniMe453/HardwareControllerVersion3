using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationNodeBase
{
    protected float m_nodeValue;
    public float NodeValue {get {return m_nodeValue;}}

    public SimulationNodeBase(float value)
    {
        m_nodeValue = value;
    }
}
