using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.OS;
using Unity.UI;

public class System_COM_Interface : MonoBehaviourApplication
{
    public Canvas canvas;
    public RenderCameraToTexture planetWireframeCamera;
    protected override void OnAppLoaded()
    {
        planetWireframeCamera.enableRendering = true;
        UIManager.AddToViewport(canvas, 100);
    }

    protected override void OnAppQuit()
    {
        planetWireframeCamera.enableRendering = false;
        UIManager.RemoveFromViewport(canvas);
    }
}
