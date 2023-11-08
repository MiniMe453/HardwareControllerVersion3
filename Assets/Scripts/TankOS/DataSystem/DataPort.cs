using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPort : MonoBehaviour
{
    private bool m_dataDownloaded = false;
    public bool DataDownloaded {get{return m_dataDownloaded;}}
    public DataLog dataLog;

    public DataLog GetDataLog()
    {
        m_dataDownloaded = true;
        return dataLog;
    }
}
