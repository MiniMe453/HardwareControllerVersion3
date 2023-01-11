using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchObjectLocation : MonoBehaviour
{
    public Transform transformToMatch;
    public Vector3 axisMods;

    void Update()
    {
        float x = transformToMatch.position.x * axisMods.x;
        float y = transformToMatch.position.y * axisMods.y;
        float z = transformToMatch.position.z * axisMods.z;

        transform.position = new Vector3(axisMods.x > 0? x : transform.position.x, 
        axisMods.y > 0? y : transform.position.y, 
        axisMods.z > 0? z : transform.position.z);
    }
}
