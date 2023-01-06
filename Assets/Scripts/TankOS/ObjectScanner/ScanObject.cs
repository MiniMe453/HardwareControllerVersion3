using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanObject : MonoBehaviour
{
    public float temperature;
    public float magnetic;
    public float radiation;
    public string objName;
    public Struct_ObjectScan GetObjectProperties()
    {
        Struct_ObjectScan objectScan = new Struct_ObjectScan();

        return objectScan;
    }
}
