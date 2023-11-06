using System.Collections;
using System.Collections.Generic;
using Rover.Arduino;
using Rover.Interface;
using UnityEngine;
using UnityEngine.InputSystem;

public class System_DataLogs : MonoBehaviour, IInputTypes
{
    float m_timeSinceLastCheck = 0f;
    float m_maxWaitTime = 0.1f;
    DataPort m_currentAvailableDataPort;
    private float m_distToObject;
    private MessageBox m_messageBox;
    public float objectScanRadius = 1f;
    public float objectScanCheckDistance = 5f;

    //private List<DataLogEntry> dataLogs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnDatabaseInit()
    {

    }

    public void AssignArduinoEvents()
    {
        ArduinoInputDatabase.GetInputFromName("LIDAR Button").EOnButtonPressed += OnDataDownloadButtonPressed;
    }

    public void AssignKeyboardEvents()
    {
        RoverInputManager.InputActions["LIDAR Button"].performed += OnDataDownloadButtonPressed;
    }

    void OnDataDownloadButtonPressed(int pin)
    {

    }

    void OnDataDownloadButtonPressed(InputAction.CallbackContext context)
    {
        if(!m_currentAvailableDataPort)
        {
            Debug.Log("No data port found");
            return;
        }

        DownloadData();
    }

    void DownloadData()
    {
        Debug.Log("Data downloaded from data port.");

        //Datalogs.append(data from the data port);
    }
    

    void FixedUpdate()
    {
        m_timeSinceLastCheck += Time.deltaTime;

        if(m_timeSinceLastCheck < m_maxWaitTime)
            return;

        m_timeSinceLastCheck = 0f;

        ScanForDataPorts();
    }

    void ScanForDataPorts()
    {
        RaycastHit hit;
        DataPort tmpDataPort = null;

        if (Physics.SphereCast(transform.position, objectScanRadius, transform.forward, out hit, objectScanCheckDistance, ~LayerMask.GetMask(new string[] { "Rover", "Terrain" })))
        {
            if (hit.collider.gameObject.TryGetComponent(out DataPort dataPort))
            {
                tmpDataPort = dataPort;
                m_distToObject = hit.distance;
            }
        }

        if (!tmpDataPort && m_currentAvailableDataPort)
        {
            ClearDataPortObject();
        }

        if (tmpDataPort != m_currentAvailableDataPort)
        {
            m_currentAvailableDataPort = tmpDataPort;
        }

        if (!m_messageBox && m_currentAvailableDataPort)
        {
            m_messageBox = UIManager.ShowMessageBox(m_currentAvailableDataPort.DataDownloaded? "DATA ALREADY DOWNLOADED":"DATA PORT IN RANGE", Color.white, -1f);
        }
    }

    void ClearDataPortObject()
    {
        m_currentAvailableDataPort = null;

        if (m_messageBox)
        {
            m_messageBox.HideMessageBox();
            m_messageBox = null;
        }
    }
}
