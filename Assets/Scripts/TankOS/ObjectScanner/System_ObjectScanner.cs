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
    public ScanDataType scanDataType;
}

[Serializable]
public struct SurfaceProperty
{
    [SerializeField]
    public SurfaceTypes materialType;
    [SerializeField]
    public float materialDensity;
}
public enum SurfaceTypes {Strontium, Tungsten, Iron, Aluminum, Lead, Carbon, Radium, Cobalt, Sulfur, Copper, Titanium, Potassium, Sodium, Unknown}
public class System_ObjectScanner : MonoBehaviour
{
    public float objectScanCheckDistance = 5f;
    public float objectScanRadius = 1f;
    private ScanObject m_scanObject;
    private float m_distToObject;
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
            CheckForScannabaleObjects();
            return;
        }

        Struct_ObjectScan objectScan = m_scanObject.GetObjectScan();
        objectScan.objDistAtScan = m_distToObject;
        
        ClearScanObject();
        
        ArduinoPrinterManager.Instance.PrintObjectScan(objectScan);
    }

    void ClearScanObject()
    {
        m_scanObject = null;

        if(m_messageBox)
        {
            m_messageBox.HideMessageBox();
            m_messageBox = null;
        }
    }

    void OnDrawGizmos()
    {
        GizmosExtend.DrawCapsule(transform.position, (transform.forward * objectScanCheckDistance) + transform.position, objectScanRadius, Color.red);
    }

    void CheckForScannabaleObjects()
    {
        int scannableObjectsCount = 0;
        float closestDistance = 999f;

        Collider[] collidedObjects = Physics.OverlapSphere(transform.position, 100f);

        if(collidedObjects.Length == 0)
        {
            UIManager.ShowMessageBox("NO SCANNABLE OBJECTS", Color.red, 2f);
            return;
        }

        foreach(Collider collider in collidedObjects)
        {
            if(collider.gameObject.TryGetComponent(out ScanObject obj))
            {
                scannableObjectsCount++;

                float dist = Vector3.Distance(transform.position, collider.gameObject.transform.position);

                if(dist < closestDistance)
                    closestDistance = dist;
            }
        }

        if(scannableObjectsCount > 0)
        {
            UIManager.ShowMessageBox(scannableObjectsCount.ToString() + (scannableObjectsCount == 1? " OBJECT" : " OBJECTS") + " FOUND. CLOSEST OBJECT: " + closestDistance.ToString("0.0") + "M" , Color.white, 2f);
        }
        else
        {
            UIManager.ShowMessageBox("NO SCANNABLE OBJECTS", Color.red, 2f); 
        }

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
                {
                    tmpScanObj = scanObject;
                    m_distToObject = hit.distance;
                }
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
        }
    }
}
