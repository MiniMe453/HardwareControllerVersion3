using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationNodeBase
{
    protected float m_nodeValue;
    public float NodeValue {get {return m_nodeValue;}}
    protected Vector3 m_location;
    public Vector3 Location { get{return m_location;}}

    public SimulationNodeBase(float value, Vector3 location)
    {
        m_nodeValue = value;
        m_location = location;
    }
}