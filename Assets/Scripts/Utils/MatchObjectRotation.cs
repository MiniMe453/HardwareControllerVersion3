using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchObjectRotation : MonoBehaviour
{
    public Transform transformToMatch;
    public Vector3 axisMods;

    void Update()
    {
        float x = transformToMatch.rotation.eulerAngles.x * axisMods.x;
        float y = transformToMatch.rotation.eulerAngles.y * axisMods.y;
        float z = transformToMatch.rotation.eulerAngles.z * axisMods.z;

        transform.rotation = Quaternion.Euler(axisMods.x > 0? x : transform.rotation.eulerAngles.x, 
        axisMods.y > 0? y : transform.rotation.eulerAngles.y, 
        axisMods.z > 0? z : transform.rotation.eulerAngles.z);
    }
}
