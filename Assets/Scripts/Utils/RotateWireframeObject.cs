using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWireframeObject : MonoBehaviour
{
    public float rotationSpeed = 1f;

    public Vector3 axis = new Vector3(0,0,1);

    void Update()
    {
        transform.rotation *= Quaternion.Euler(axis * rotationSpeed);
    }
}
