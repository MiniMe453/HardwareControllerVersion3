using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemPitchRoll : MonoBehaviour
{
    static float m_pitch;
    static float m_roll;
    public static float Roll {get {return m_roll;}}
    public static float Pitch {get {return m_pitch;}}

    void Update()
    {
        m_pitch = -(Vector3.SignedAngle(transform.forward, new Vector3(0,1,0), transform.right) + 90);
        m_roll = Vector3.SignedAngle(transform.right, new Vector3(0,1,0), transform.forward) - 90;

        //m_roll = gameObject.transform.root.localRotation.x * 180f;
    }
}
