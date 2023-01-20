using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiationMonobehaviour : MonoBehaviour
{
    public float radiationValue;
    private RadiationSimNode m_radiationSimNode;

    void OnEnable()
    {
        m_radiationSimNode = new RadiationSimNode(radiationValue, transform.position);
    }
}
