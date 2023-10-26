using System.Collections;
using System.Collections.Generic;
using Rover.Settings;
using UnityEngine;

public class NightVisionSetAmbientColor : MonoBehaviour
{
    public Color defaultAmbientColor;
    public float ambientIntensity = 0.1f;

    void OnEnable()
    {
        RenderSettings.ambientLight = Color.white;

        RenderSettings.ambientIntensity = ambientIntensity * 2;
    }

    void OnDisable()
    {
        RenderSettings.ambientLight = GameSettings.DEFAULT_AMBIENT_COLOR;
        RenderSettings.ambientIntensity = ambientIntensity;
    }
}
