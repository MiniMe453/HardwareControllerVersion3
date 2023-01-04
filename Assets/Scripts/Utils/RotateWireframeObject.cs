using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWireframeObject : MonoBehaviour
{
    public float rotationSpeed = 1f;

    void Update()
    {
        transform.rotation *= Quaternion.Euler(Vector3.forward * rotationSpeed);
    }
}
