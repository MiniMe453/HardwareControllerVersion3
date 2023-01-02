using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rover.Interface;
using Rover.OS;

public static class UIManager
{
    private static List<Canvas> m_uiScreens = new List<Canvas>();
    private static bool uiHidden;
    private static GameObject messageBox;

    static UIManager()
    {
        messageBox = (GameObject)Resources.Load("MessageBox");
        RoverOperatingSystem.EOnOSModeChanged += OnOperatingSystemStateChanged;
    }

    public static void OnOperatingSystemStateChanged(OSMode newState)
    {
        if (newState == OSMode.Rover)
        {
            ClearViewport();
        }
    }

    public static void AddToViewport(Canvas newUI, int sortOrder = 0)
    {
        if (!uiHidden)
        {
            m_uiScreens.Add(newUI);
            newUI.enabled = true;

            if (sortOrder != 0)
            {
                newUI.sortingOrder = sortOrder;
            }
        }
    }

    public static void RemoveFromViewport(Canvas oldUI)
    {
        if (!uiHidden)
        {
            m_uiScreens.Remove(oldUI);
            oldUI.enabled = false;
        }
    }

    public static bool IsInViewport(Canvas canvas)
    {
        return canvas.enabled;
    }

    public static void ClearViewport()
    {
        foreach (Canvas c in m_uiScreens)
        {
            c.enabled = false;
        }

        m_uiScreens.Clear();
    }

    private static void HideAllUI()
    {
        uiHidden = !uiHidden;

        if (uiHidden)
        {
            foreach (Canvas c in m_uiScreens)
            {
                c.enabled = false;
            }
        }
        else
        {
            foreach (Canvas c in m_uiScreens)
            {
                c.enabled = true;
            }
        }
    }

    public static MessageBox ShowMessageBox(string text, Color color, float duration, bool waitForInput = false)
    {
        GameObject GO = Object.Instantiate(messageBox);

        MessageBox message = GO.GetComponent<MessageBox>();
        message.ShowMessageBox(text, color, duration, waitForInput);

        return message;
    }
}
