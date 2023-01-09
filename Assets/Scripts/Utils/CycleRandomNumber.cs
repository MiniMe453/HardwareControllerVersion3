using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityTimer;
using System;
using UnityEngine.UI;

public class CycleRandomNumber : MonoBehaviour
{
    private TextMeshProUGUI m_text => GetComponent<TextMeshProUGUI>();
    private RectTransform m_rectTransform => GetComponent<RectTransform>();
    private Canvas m_canvas => GetComponentInParent<Canvas>();

    public float minValue = 0f;
    public float maxValue = 1f;
    public string prefix = "";
    public string suffix = "";
    public string stringFormat = "00.0";

    private static Timer m_updateTextTimer;
    private static event Action EOnUpdateTimer;

    void OnEnable()
    {
        EOnUpdateTimer += UpdateText;

        if(m_updateTextTimer == null)
            m_updateTextTimer = Timer.Register(0.15f, () => {EOnUpdateTimer?.Invoke();}, isLooped: true);
    }

    void UpdateText()
    {
        if(!m_canvas.enabled)
            return;

        m_text.text = prefix + UnityEngine.Random.Range(minValue, maxValue).ToString(stringFormat) + suffix;
    }
}
