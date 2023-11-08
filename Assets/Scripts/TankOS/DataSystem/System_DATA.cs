using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rover.Arduino;
using Rover.Interface;
using UnityEngine;
using UnityEngine.InputSystem;

public class System_DATA : MonoBehaviour, IInputTypes
{
    float m_timeSinceLastCheck = 0f;
    float m_maxWaitTime = 0.1f;
    DataPort m_currentAvailableDataPort;
    private MessageBox m_messageBox;
    public float objectScanRadius = 1f;
    public float objectScanCheckDistance = 5f;
    private static List<DataLog> m_dataLogEntries = new List<DataLog>();
    public static List<DataLog> DataLogs {get{return m_dataLogEntries;}}

    // Start is called before the first frame update
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
        ArduinoInputDatabase.GetInputFromName("LIDAR Button").EOnButtonPressed += OnDataDownloadButtonPressed;
    }

    public void AssignKeyboardEvents()
    {
        RoverInputManager.InputActions["LIDAR Button"].performed += OnDataDownloadButtonPressed;
    }

    void OnDataDownloadButtonPressed(int pin)
    {
        if(!m_currentAvailableDataPort)
        {
            return;
        }

        DownloadData();
    }

    void OnDataDownloadButtonPressed(InputAction.CallbackContext context)
    {
        OnDataDownloadButtonPressed(-1);
    }

    void DownloadData()
    {
        m_dataLogEntries.Insert(0, m_currentAvailableDataPort.GetDataLog());

        ClearDataPortObject();
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
                if(dataPort == m_currentAvailableDataPort)
                    return;

                tmpDataPort = dataPort;
            }
        }

        if (tmpDataPort == null)
        {
            ClearDataPortObject();
            return;
        }

        if (tmpDataPort != m_currentAvailableDataPort)
        {
            m_currentAvailableDataPort = tmpDataPort;
        }

        if (!m_messageBox && m_currentAvailableDataPort)
        {
            m_messageBox = UIManager.ShowMessageBox("PORT: "+m_currentAvailableDataPort.dataLog.dataPortID+"\n"+(m_currentAvailableDataPort.DataDownloaded? "DATA ALREADY DOWNLOADED":"DATA PORT IN RANGE"), Color.white, -1f);
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
