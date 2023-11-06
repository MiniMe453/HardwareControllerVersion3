using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class RoverTireTracksGenerator : MonoBehaviour
{
    public SplineComputer spline => GetComponent<SplineComputer>();
    // Start is called before the first frame update
    public SplineUser splineMesh => GetComponent<SplineUser>();
    private Vector3 lastPointPos;
    public float distanceBeforeNewSpline = 2f;
    public Transform refTransform;

    private SplinePoint[] tireTrackPoints = new SplinePoint[100];
    private List<SplinePoint> points = new List<SplinePoint>();
    void Start()
    {
        for (int i=0;i < tireTrackPoints.Length; i++)
        {
            tireTrackPoints[i] = new SplinePoint();
            tireTrackPoints[i].position = refTransform.position;
            tireTrackPoints[i].normal = Vector3.up;
            tireTrackPoints[i].size = 1f;
            tireTrackPoints[i].color = Color.white;
        }

        spline.SetPoints(tireTrackPoints);

        if(!splineMesh.enabled)
            splineMesh.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!splineMesh.enabled)
            splineMesh.enabled = true;

        if(Vector3.Distance(lastPointPos, refTransform.position) < distanceBeforeNewSpline)
            return;

        for (int i= tireTrackPoints.Length - 1;i >= 0; i--)
        {
            if(i != 0)
            {
                tireTrackPoints[i] = tireTrackPoints[i - 1];
                continue;
            }

            SplinePoint newSplinePoint = new SplinePoint();
            newSplinePoint.position = refTransform.position;
            newSplinePoint.normal = Vector3.up;
            newSplinePoint.size = 1f;
            newSplinePoint.color = Color.white;
            tireTrackPoints[i] = newSplinePoint;
        }

        spline.SetPoints(tireTrackPoints);

        lastPointPos = refTransform.position;
        
    }
}
