using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.OS;
using UnityEngine.UI;

public class System_RDIO_Interface : MonoBehaviourApplication
{
    private AudioAnalyzer m_radioSignalAnalyzer;
    public Canvas canvas;
    
    protected override void Init()
    {
        
    }
    protected override void OnAppLoaded()
    {
        UIManager.AddToViewport(canvas, 100);
    }

    protected override void OnAppQuit()
    {
        UIManager.RemoveFromViewport(canvas);
    }
}
