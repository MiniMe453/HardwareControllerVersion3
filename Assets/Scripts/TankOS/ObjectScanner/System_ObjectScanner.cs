using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using Rover.DateTime;
using Rover.Interface;
using System;
using Kit;
using UnityEngine.InputSystem;

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
public enum SurfaceTypes { Strontium, Tungsten, Iron, Aluminum, Lead, Carbon, Radium, Cobalt, Sulfur, Copper, Titanium, Potassium, Sodium, Unknown }
public class System_ObjectScanner : MonoBehaviour, IInputTypes
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
        GameInitializer.EOnGameInitialized += OnDatabaseInit;
    }

    void OnDatabaseInit()
    {
        if(RoverInputManager.UseKeyboardInput)
            AssignKeyboardEvents();
        else
            AssignArduinoEvents();
    }

    public void AssignArduinoEvents()
    {
        ArduinoInputDatabase.GetInputFromName("LIDAR Button").EOnButtonPressed += OnScanButtonPressed;
    }

    public void AssignKeyboardEvents()
    {
        RoverInputManager.InputActions["LIDAR Button"].performed += OnScanButtonPressed;
    }

    void OnScanButtonPressed(int pin)
    {
        if (!m_scanObject)
        {
            CheckForScannabaleObjects();
            return;
        }

        Struct_ObjectScan objectScan = m_scanObject.GetObjectScan();
        objectScan.objDistAtScan = m_distToObject;

        ClearScanObject();

        ArduinoPrinterManager.Instance.PrintObjectScan(objectScan);
    }

    void OnScanButtonPressed(InputAction.CallbackContext context)
    {
        OnScanButtonPressed(-1);
    }

    void ClearScanObject()
    {
        m_scanObject = null;

        if (m_messageBox)
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
        int closestIndex = -1;

        Collider[] collidedObjects = Physics.OverlapSphere(transform.position, 100f);

        if (m_messageBox != null)
        {
            m_messageBox.HideMessageBox();
            m_messageBox = null;
        }

        for (int i = 0; i < collidedObjects.Length; i++)
        {
            if (collidedObjects[i].gameObject.TryGetComponent(out ScanObject obj))
            {
                if (obj.WasScanned)
                    continue;

                scannableObjectsCount++;

                float dist = Vector3.Distance(transform.position, collidedObjects[i].gameObject.transform.position);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestIndex = i;
                }
            }
        }

        if (scannableObjectsCount == 0)
        {
            m_messageBox = UIManager.ShowMessageBox("NO SCANNABLE OBJECTS", Color.red, 2f);
            return;
        }

        float heading = Vector3.SignedAngle((collidedObjects[closestIndex].ClosestPointOnBounds(transform.position) - transform.position), new Vector3(0, 0, 1), Vector3.up) - 180f;
        float sign = Mathf.Sign(heading);

        Debug.LogError((collidedObjects[closestIndex].ClosestPointOnBounds(transform.position).ToString() + "  " + transform.position));


        if (sign < 0)
            heading = 360 - Mathf.Abs(heading);


        m_messageBox = UIManager.ShowMessageBox(scannableObjectsCount.ToString() + " SCANNABLE" + (scannableObjectsCount == 1 ? " OBJECT" : " OBJECTS") + " FOUND. CLOSEST OBJECT DIRECTION: " + heading.ToString("0.0") + "°", Color.white, 2f);
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        ScanObject tmpScanObj = null;

        if (Physics.SphereCast(transform.position, objectScanRadius, transform.forward, out hit, objectScanCheckDistance, ~LayerMask.GetMask(new string[] { "Rover", "Terrain" })))
        {
            if (hit.collider.gameObject.TryGetComponent(out ScanObject scanObject))
            {
                if (!scanObject.WasScanned)
                {
                    tmpScanObj = scanObject;
                    m_distToObject = hit.distance;
                }
            }
        }

        if (!tmpScanObj && m_scanObject)
        {
            ClearScanObject();
        }

        if (tmpScanObj != m_scanObject)
        {
            m_scanObject = tmpScanObj;
        }

        if (!m_messageBox && m_scanObject)
        {
            m_messageBox = UIManager.ShowMessageBox("SCANNABLE OBJECT IN RANGE", Color.white, -1f);
        }
    }
}
