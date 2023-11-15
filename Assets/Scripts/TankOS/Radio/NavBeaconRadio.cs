using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavBeaconRadio : MonoBehaviour
{
    public AudioSource beaconAudioSource;
    public float m_timeInterval = 1f;
    private float m_currentTime;
    private float m_detectionAngle = 15f;

    
    void Start()
    {
        
    }

    
    void Update()
    {
        if(!beaconAudioSource.enabled)
            return;

        m_currentTime += Time.deltaTime;

        if(m_currentTime < m_timeInterval)
            return;

        m_currentTime = 0;


        float heading = GetRoverHeading();

        if(heading < m_detectionAngle || heading > 360f - m_detectionAngle)
            beaconAudioSource.volume = 1f;
        else
        {   
            float fullRange = 360 - m_detectionAngle * 2;
            beaconAudioSource.volume = Mathf.Abs(heading - fullRange/2)/(fullRange/2);
        }

        Vector3 worldLoc = transform.position;
        Vector3 roverLoc = System_GPS.WorldSpacePos;

        worldLoc.y = 0;
        roverLoc.y = 0;

        float lerpVal = Mathf.Clamp01(Vector3.Distance(worldLoc, roverLoc)/250f);
        lerpVal = -Mathf.Pow(lerpVal - 1, 4) + 1;

        m_timeInterval = Mathf.Lerp(0.175f, 1.5f, lerpVal);
        Debug.Log(m_timeInterval);

        beaconAudioSource.Play();
    }

    float GetRoverHeading()
    {
        Vector3 roverLoc = System_GPS.WorldSpacePos;
        Vector3 worldPos = transform.position;

        worldPos.y = 0;
        roverLoc.y = 0;
        
        float heading = Vector3.SignedAngle(-Vector3.ProjectOnPlane(System_GPS.RoverForwardVector, Vector3.up), (worldPos - roverLoc).normalized, Vector3.up);
        float sign = Mathf.Sign(heading);

        if(sign < 0)
            heading = 360 - Mathf.Abs(heading);

        return heading;
    }
}
