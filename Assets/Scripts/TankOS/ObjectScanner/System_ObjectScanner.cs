using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using Rover.DateTime;
using Rover.Interface;
using System;

public struct Struct_ObjectScan
{
    public string objName;
    public DateTimeStruct scanTime;
    public float objDistAtScan;
    public Mesh objectMesh;
    public float objectSurfaceDepth;
    public List<SurfaceProperty> surfaceProperties;
    public float temperature;
    public float magneticField;
    public float radiation;
}

public struct SurfaceProperty
{
    SurfaceTypes materialType;
    float materialAmount;
}
public enum SurfaceTypes {Strontium, Tungsten, Iron, Aluminum, Lead, Carbon, Radium, Cobalt, Sulfur, Copper, Titanium, Potassium, Sodium, Unknown}
public class System_ObjectScanner : MonoBehaviour
{
    public float objectScanCheckDistance = 5f;
    public float objectScanRadius = 1f;
    private ScanObject m_scanObject;
    private bool m_messageBoxShown;
    private MessageBox m_messageBox;
    public static event Action<Struct_ObjectScan> EOnObjectScanned;
    
    void OnEnable()
    {
        ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
    }

    void OnDatabaseInit()
    {
        ArduinoInputDatabase.GetInputFromName("LIDAR Button").EOnButtonPressed += OnScanButtonPressed;
    }

    void OnScanButtonPressed(int pin)
    {
        if(!m_scanObject)
        {
            UIManager.ShowMessageBox("NO SCANNABLE OBJECTS", Color.red, 2f);
            return;
        }

        Struct_ObjectScan objectScan = m_scanObject.GetObjectProperties();
        m_scanObject = null;

    }

    void FixedUpdate()
    {
        RaycastHit hit;
        ScanObject tmpScanObj = null;

        if(Physics.SphereCast(transform.position, 1f,transform.forward, out hit, objectScanCheckDistance))
        {
            if(hit.collider.gameObject.TryGetComponent(out ScanObject scanObject))
            {
                tmpScanObj = scanObject;
            }
        } 

        if(tmpScanObj && !m_scanObject)
        {
            m_scanObject = null;
            
            if(m_messageBox)
            {
                m_messageBox.HideMessageBox();
                m_messageBox = null;
            }
        }

        if(tmpScanObj != m_scanObject)
        {
            m_scanObject = tmpScanObj;
        }

        if(!m_messageBox)
        {
            m_messageBox = UIManager.ShowMessageBox("OBJECT IN RANGE", Color.white, -1f);
        }
    }
}
