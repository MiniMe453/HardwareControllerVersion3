using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticMonobehaviour : MonoBehaviour
{
    public float Magnetic;
    private MagneticSimNode magneticSimNode;
    void OnEnable()
    {
        magneticSimNode = new MagneticSimNode(Magnetic, transform.position);
    }
}
