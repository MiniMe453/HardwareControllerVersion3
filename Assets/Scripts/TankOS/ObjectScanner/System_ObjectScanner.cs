using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using Rover.DateTime;
using Rover.Interface;
using System;
using Kit;

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
        Debug.LogError("Button pressed");

        if(!m_scanObject)
        {
            UIManager.ShowMessageBox("NO SCANNABLE OBJECTS", Color.red, 2f);
            return;
        }

        Struct_ObjectScan objectScan = m_scanObject.GetObjectScan();
        
        ClearScanObject();
    }

    void ClearScanObject()
    {
        m_scanObject = null;

        if(m_messageBox)
        {
            m_messageBox.HideMessageBox();
            m_messageBox = null;

            Debug.LogError("messagebox cleared");
        }
    }

    void OnDrawGizmos()
    {
        GizmosExtend.DrawCapsule(transform.position, (transform.forward * objectScanCheckDistance) + transform.position, objectScanRadius, Color.red);
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        ScanObject tmpScanObj = null;

        if(Physics.SphereCast(transform.position, objectScanRadius,transform.forward, out hit, objectScanCheckDistance, ~LayerMask.GetMask(new string[] {"Rover", "Terrain"})))
        {
            if(hit.collider.gameObject.TryGetComponent(out ScanObject scanObject))
            {
                if(!scanObject.WasScanned)
                    tmpScanObj = scanObject;
            }
        } 

        if(!tmpScanObj && m_scanObject)
        {
            ClearScanObject();
        }

        if(tmpScanObj != m_scanObject)
        {
            m_scanObject = tmpScanObj;
        }

        if(!m_messageBox && m_scanObject)
        {
            m_messageBox = UIManager.ShowMessageBox("OBJECT IN RANGE", Color.white, -1f);
            Debug.LogError("Message box Shown");
        }
    }
}
