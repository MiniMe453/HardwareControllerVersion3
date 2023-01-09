using System.Collections;
using System.Collections.Generic;
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
        RenderSettings.ambientLight = defaultAmbientColor;
    }
}
