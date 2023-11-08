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

        Vector3 roverLoc = System_GPS.WorldSpacePos;

        markerLocation.y = 0;
        roverLoc.y = 0;
        
        m_heading = Vector3.SignedAngle(markerLocation - roverLoc, new Vector3(0,0,1), Vector3.up);
        float sign = Mathf.Sign(m_heading);

        if(sign < 0)
            m_heading = 360 - Mathf.Abs(m_heading);

        Vector3 newCompassPos = new Vector3(m_heading/360f * scaleFactor, 0,0);   
        markerTransform.anchoredPosition = newCompassPos;
    }
}
