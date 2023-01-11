using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Settings;

public class System_GPS : MonoBehaviour
{
    private static float m_gpsCoordY;
    private static float m_gpsCoordX;
    public static Vector2 GPSCoordinates {get {return new Vector2(m_gpsCoordX, m_gpsCoordY);} }
    private static float m_heading;
    public static float Heading { get { return m_heading; } }
    
    void Update()
    {
        m_gpsCoordX = GameSettings.GPS_COORD_X_MIN + ((gameObject.transform.position.x/GameSettings.TERRAIN_MAX) * (GameSettings.GPS_COORD_X_MAX - GameSettings.GPS_COORD_X_MIN));
        m_gpsCoordY = GameSettings.GPS_COORD_Y_MIN + ((gameObject.transform.position.z/GameSettings.TERRAIN_MAX) * (GameSettings.GPS_COORD_Y_MAX - GameSettings.GPS_COORD_Y_MIN));

        // m_heading = gameObject.transform.rotation.y;

        m_heading = Vector3.SignedAngle(transform.forward, new Vector3(0,0,1), Vector3.up);
        float sign = Mathf.Sign(m_heading);

        if(sign < 0)
            m_heading = 360 - Mathf.Abs(m_heading);

    }

    public static Vector2 WorldPosToGPSCoords(Vector3 position)
    {
        float gpsCoordX = GameSettings.GPS_COORD_X_MIN + ((position.x/GameSettings.TERRAIN_MAX) * (GameSettings.GPS_COORD_X_MAX - GameSettings.GPS_COORD_X_MIN));
        float gpsCoordY = GameSettings.GPS_COORD_Y_MIN + ((position.z/GameSettings.TERRAIN_MAX) * (GameSettings.GPS_COORD_Y_MAX - GameSettings.GPS_COORD_Y_MIN));

        return new Vector2(gpsCoordX, gpsCoordY);
    }
}
