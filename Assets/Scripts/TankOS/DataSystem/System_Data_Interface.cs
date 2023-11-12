using System.Collections;
using System.Collections.Generic;
using Rover.OS;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityTimer;

public class System_Data_Interface : MonoBehaviourApplication
{
    public Canvas canvas;
    public DataLogEntryPage dataLogEntryPage;
    public TextMeshProUGUI pageNumberText;
    public UIDataLogEntryTemplate[] dataLogEntryTemplates;
    private int m_currentPageIdx;
    private int m_maxNumPages;
    private int m_numEntriesPerPage = 13;
    private int m_selectedDataLogIdx = 0;
    private bool m_canOpenDataLog = false;
    private bool m_isDataLogLoaded = false;
    protected override void Init()
    {
        applicationInputs.AddAction("goup", binding:"<Keyboard>/upArrow");
        applicationInputs.AddAction("godown", binding:"<Keyboard>/downArrow");
        applicationInputs.AddAction("pageForward", binding:"<Keyboard>/rightArrow");
        applicationInputs.AddAction("pageBackward", binding:"<Keyboard>/leftArrow");
        applicationInputs.AddAction("openLog", binding:"<Keyboard>/enter");

        applicationInputs["goup"].performed += NavigateUp;
        applicationInputs["godown"].performed += NavigateDown;
        applicationInputs["pageForward"].performed += PageForward;
        applicationInputs["pageBackward"].performed += PageBackward;
        applicationInputs["openLog"].performed += OpenDataLog;
    }

    protected override void OnAppLoaded()
    {
        UIManager.AddToViewport(canvas, 100);

        m_maxNumPages = Mathf.CeilToInt((float)System_DATA.DataLogs.Count/(float)m_numEntriesPerPage);
        m_currentPageIdx = 0;

        RefreshDataLogsList();

        Timer.Register(0.1f, () => {m_canOpenDataLog = true;});
    }

    protected override void OnAppQuit()
    {
        UIManager.RemoveFromViewport(canvas);
        dataLogEntryPage.HideEntryPage();
        m_canOpenDataLog = false;
    }

    protected override void Action_Quit(InputAction.CallbackContext context)
    {
        if(m_isDataLogLoaded)
        {
            dataLogEntryPage.HideEntryPage();
            m_isDataLogLoaded = false;
        }
        else
        {
            base.Action_Quit(context);
        }
    }

    private void RefreshDataLogsList()
    {
        dataLogEntryTemplates[m_selectedDataLogIdx].Unhighlight();

        int startIdx = m_numEntriesPerPage * m_currentPageIdx;

        for(int i = startIdx; i < startIdx + m_numEntriesPerPage; i++)
        {
            if(i >= System_DATA.DataLogs.Count)
            {
                dataLogEntryTemplates[i].SetInvisible();
                continue;
            }
            else if(!dataLogEntryTemplates[i].isVisible)
            {
                dataLogEntryTemplates[i].SetVisible();
            }

            dataLogEntryTemplates[i].SetData(System_DATA.DataLogs[i]);
        }

        m_selectedDataLogIdx = startIdx;
        dataLogEntryTemplates[m_selectedDataLogIdx].Highlight();
        pageNumberText.text = $"PAGE {(m_currentPageIdx + 1).ToString("000")}/{m_maxNumPages.ToString("000")}";
    }

    private void NavigateUp(InputAction.CallbackContext context)
    {
        if(m_isDataLogLoaded)
            return;

        if(m_selectedDataLogIdx - 1 < 0)
        {
            return;
        }
        
        dataLogEntryTemplates[m_selectedDataLogIdx].Unhighlight();

        m_selectedDataLogIdx--;

        dataLogEntryTemplates[m_selectedDataLogIdx].Highlight();
    }

    private void NavigateDown(InputAction.CallbackContext context)
    {   
        if(m_isDataLogLoaded)
            return;

        if(m_selectedDataLogIdx + 1 >= m_numEntriesPerPage || m_selectedDataLogIdx + 1 >= System_DATA.DataLogs.Count)
        {
            return;
        }

        dataLogEntryTemplates[m_selectedDataLogIdx].Unhighlight();

        m_selectedDataLogIdx++;

        dataLogEntryTemplates[m_selectedDataLogIdx].Highlight();
    }

    private void PageForward(InputAction.CallbackContext context)
    {
        if(m_isDataLogLoaded)
        {
            dataLogEntryPage.PageForward();
            return;
        }

        m_currentPageIdx++;

        if(m_currentPageIdx == m_maxNumPages)
        {
            m_currentPageIdx = m_maxNumPages - 1;
            return;
        }

        RefreshDataLogsList();
    }

    private void PageBackward(InputAction.CallbackContext context)
    {
        if(m_isDataLogLoaded)
        {
            dataLogEntryPage.PageBackward();
            return;
        }

        m_currentPageIdx--;

        if(m_currentPageIdx < 0)
        {
            m_currentPageIdx = 0;
            return;
        }

        RefreshDataLogsList();
    }

    private void OpenDataLog(InputAction.CallbackContext context)
    {
        if(!m_canOpenDataLog || m_isDataLogLoaded)    
            return;

        if(System_DATA.DataLogs.Count == 0)
            return;

        DataLog dataLog = dataLogEntryTemplates[m_selectedDataLogIdx].DataLog;

        dataLogEntryPage.SetText(dataLog);
        dataLogEntryPage.ShowEntryPage();
        m_isDataLogLoaded = true;
    }
}

