using System.Collections;
using System.Collections.Generic;
using Rover.Settings;
using UnityEngine;

public class NightVisionSetAmbientColor : MonoBehaviour
{
    public Color defaultAmbientColor;

    void OnEnable()
    {
        RenderSettings.ambientLight = Color.white;
    }

    void OnDisable()
    {
        RenderSettings.ambientLight = GameSettings.DEFAULT_AMBIENT_COLOR;
    }
}
