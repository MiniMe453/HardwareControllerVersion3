using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIDataLogEntryTemplate : MonoBehaviour
{
    public TextMeshProUGUI subjectText;
    public TextMeshProUGUI authorText;
    public TextMeshProUGUI dateUpdated;
    public TextMeshProUGUI size;
    private DataLog m_dataLog;
    public DataLog DataLog {get
    {    
        m_dataLog.hasBeenRead = true;   
        return m_dataLog;
    } }
    public RawImage backgroundImage;
    public bool isVisible = true;

    public void SetData(DataLog log)
    {
        m_dataLog = log;

        subjectText.text = (log.hasBeenRead? "" : "*") + log.dataLogName;
        authorText.text = log.AuthorAsString;
        dateUpdated.text = log.dateUpdated;
        size.text = (log.entries.Count * 16).ToString() + "kb";
    }

    public void Highlight()
    {
        backgroundImage.color = Color.white;
        subjectText.color = Color.black;
        authorText.color = Color.black;
        dateUpdated.color = Color.black;
        size.color = Color.black;
    }

    public void Unhighlight()
    {
        backgroundImage.color = Color.black;
        subjectText.color = Color.white;
        authorText.color = Color.white;
        dateUpdated.color = Color.white;
        size.color = Color.white;
    }

    public void SetInvisible()
    {
        isVisible = false;

        backgroundImage.gameObject.SetActive(false);
    }

    public void SetVisible()
    {
        isVisible = true;
        backgroundImage.gameObject.SetActive(true);
    }
}
