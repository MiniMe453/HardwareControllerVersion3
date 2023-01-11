using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchObjectRotation : MonoBehaviour
{
    public Transform transformToMatch;

    void Update()
    {
        Vector3 newRot = transformToMatch.rotation.eulerAngles;

        newRot.x = 0;
        newRot.z = 0;

        transform.rotation = Quaternion.Euler(newRot);
    }
}
