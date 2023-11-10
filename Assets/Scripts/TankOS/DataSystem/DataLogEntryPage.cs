using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataLogEntryPage : MonoBehaviour
{
    public TextMeshProUGUI subjectText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI dataLogName;
    public TextMeshProUGUI pageNumber;
    public Canvas canvas;
    private DataLog m_dataLog;
    private int m_maxNumPages;
    private int m_currentPageIdx;

    public void SetText(DataLog dataLog)
    {
        m_dataLog = dataLog;

        dataLogName.text = m_dataLog.dataLogName;

        m_currentPageIdx = 0;
        m_maxNumPages = dataLog.entries.Count;

        DisplayLogPage(0);
    }

    public void ShowEntryPage()
    {
        canvas.enabled = true;
    }

    public void HideEntryPage()
    {
        canvas.enabled = false;
    }

    public void PageForward()
    {
        m_currentPageIdx++;

        if(m_currentPageIdx == m_maxNumPages)
        {
            m_currentPageIdx = m_maxNumPages - 1;
            return;
        }

        DisplayLogPage(m_currentPageIdx);
    }

    public void PageBackward()
    {
        m_currentPageIdx--;

        if(m_currentPageIdx < 0)
        {
            m_currentPageIdx = 0;
            return;
        }

        DisplayLogPage(m_currentPageIdx);
    }

    private void DisplayLogPage(int entryIdx)
    {
        DataLogEntry entry = m_dataLog.entries[entryIdx];

        subjectText.text = entry.subject;
        dateText.text = $"{entry.date} : {entry.time}";
        bodyText.text = entry.textEntry;
        pageNumber.text = $"LOG# {(m_currentPageIdx + 1).ToString("000")}/{m_maxNumPages.ToString("000")}";
    }
    
}
