using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanObject : MonoBehaviour
{
    private bool m_wasScanned;
    public bool WasScanned {get{return m_wasScanned;}}
    public float temperature;
    public float magnetic;
    public float radiation;
    public string objName;
    public Struct_ObjectScan GetObjectScan()
    {
        Struct_ObjectScan objectScan = new Struct_ObjectScan();
        m_wasScanned = true;

        return objectScan;
    }
}
