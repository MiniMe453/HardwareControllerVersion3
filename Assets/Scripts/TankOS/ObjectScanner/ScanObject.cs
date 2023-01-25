using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.DateTime;

public enum ScanDataType {TemplateData, TestString, Main_HDU, RoverScan, HumanScan, HDUScan, ScanBushesFar, ScanBushesMed, ScanBushesClose}

public class ScanObject : MonoBehaviour
{
    private bool m_wasScanned;
    public bool WasScanned {get{return m_wasScanned;}}
    public string objName;
    public Mesh objScanMesh;
    public float objSurfaceDepth;
    public List<SurfaceProperty> surfaceProperties = new List<SurfaceProperty>();
    public float temperature;
    public float magnetic;
    public float radiation;
    public ScanDataType scanDataType;

    public Struct_ObjectScan GetObjectScan()
    {
        Struct_ObjectScan objectScan = new Struct_ObjectScan();
        m_wasScanned = true;

        objectScan.objName = objName;
        objectScan.objectSurfaceDepth = objSurfaceDepth;
        objectScan.objectMesh = objScanMesh;
        objectScan.radiation = radiation;
        objectScan.temperature = temperature;
        objectScan.magneticField = magnetic;
        objectScan.surfaceProperties = surfaceProperties;
        objectScan.scanTime = TimeManager.GetCurrentDateTime();
        objectScan.scanDataType = scanDataType;

        return objectScan;
    }
}
