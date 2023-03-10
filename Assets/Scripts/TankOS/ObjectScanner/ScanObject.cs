using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.DateTime;

public enum ScanDataType {Template, Test, Reactor_Core, Power_Station, Ron_Davis, Magnetic_Bush, Mars_Lander, Lillie_Nunez, Kenneth_Williams, Connie_Hoskins, Shirley_Thompson,
Thomas_Anderson, Rover_A, Rover_B}

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

    void OnEnable()
    {

    }

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
