using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
public class KeyboardAxisManager : MonoBehaviour
{
    private static float m_yAxis;
    private static float m_xAxis;
    private static float m_throttleAxis;
    private static float m_radioAxis;
    public static float RadioAxis {get{return m_radioAxis;}}
    public static float YAxis { get { return m_yAxis; } }
    public static float XAxis { get { return m_xAxis; } }
    public static float ThrottleAxis { get { return m_throttleAxis; } }
    public float returnSpeed;
    public float increaseSpeed;
    public static event Action<float> EOnYAxis;
    public static event Action<float> EOnXAxis;
    public static event Action<float> EOnThrottleAxis;
    public static event Action<float> EOnRadioAxis;
    private bool m_allowInput = false;

    void OnEnable()
    {
        GameInitializer.EOnGameInitialized += OnGameInitialized;
    }

    void OnDisable()
    {
        GameInitializer.EOnGameInitialized -= OnGameInitialized;
    }

    void OnGameInitialized()
    {
        m_allowInput = true;
    }

    void Update()
    {
        if(!m_allowInput)
            return;

        //We do this first because the radio has different blocking requirements than the rest of the code.
        float radio = RoverInputManager.RadioAxis.ReadValue<float>();
        m_radioAxis = radio == 0? m_radioAxis : IncreaseAxisValue(m_radioAxis, radio, .1f);
        EOnRadioAxis?.Invoke(m_radioAxis);

        if(CommandConsoleMain.IsConsoleVisible)
            return;

        float y = RoverInputManager.YAxis.ReadValue<float>();
        float x = RoverInputManager.XAxis.ReadValue<float>();
        float throttle = RoverInputManager.ThrottleAxis.ReadValue<float>();

        m_yAxis = y == 0 ? ReduceAxisValue(m_yAxis, y, returnSpeed) : IncreaseAxisValue(m_yAxis, y, increaseSpeed);
        m_xAxis = x == 0 ? ReduceAxisValue(m_xAxis, x, returnSpeed) : IncreaseAxisValue(m_xAxis, x, increaseSpeed);
        m_throttleAxis = throttle == 0 ? m_throttleAxis : IncreaseAxisValue(m_throttleAxis, -throttle, increaseSpeed);

        EOnXAxis?.Invoke(m_xAxis);
        EOnYAxis?.Invoke(m_yAxis);
        EOnThrottleAxis?.Invoke(m_throttleAxis);
    }

    float ReduceAxisValue(float currVal, float dir, float speed = 1f)
    {
        if (Math.Abs(currVal) > 0.01)
            return currVal - (speed * Time.deltaTime * Mathf.Sign(currVal));
        else
            return 0;
    }

    float IncreaseAxisValue(float currVal, float dir, float speed = 1f)
    {
        float newVal = currVal + (speed * Time.deltaTime * dir);

        if (Math.Abs(newVal) > 1)
            return dir;
        else
            return newVal;

    }
}
