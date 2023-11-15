using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateMapMarkerCompass : MonoBehaviour
{
    public RectTransform markerTransform;
    private Vector3 markerLocation;
    private float m_heading;
    private bool m_dataSet = false;
    public float scaleFactor = -1024f;

    public void SetMarkerData(Vector3 transform)
    {
        markerLocation = transform;
        m_dataSet = true;
    }

    void Update()
    {
        if(!m_dataSet)
            return;

        float m_heading01 = 1 - Mathf.Clamp01(System_GPS.GetHeadingToRover(transform.position)/360f);

        Vector3 newCompassPos = new Vector3(m_heading01 * 537f + 537f/2, -10f,0);   
        markerTransform.anchoredPosition = newCompassPos;
    }
}
