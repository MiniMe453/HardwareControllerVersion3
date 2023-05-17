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
    public static float YAxis { get { return m_yAxis; } }
    public static float XAxis { get { return m_xAxis; } }
    public static float ThrottleAxis { get { return m_throttleAxis; } }
    public float returnSpeed;
    public float increaseSpeed;
    private bool m_yAxisPressed;
    private bool m_xAxisPressed;
    public static event Action<float> EOnYAxis;
    public static event Action<float> EOnXAxis;
    public static event Action<float> EOnThrottleAxis;

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

    }

    void OnYAxisInput(InputAction.CallbackContext context)
    {
        m_yAxisPressed = context.performed;
    }

    void OnXAxisInput(InputAction.CallbackContext context)
    {
        m_xAxisPressed = context.performed;
    }

    void Update()
    {
        float y = InputTypeManager.InputActions["YAxis"].ReadValue<float>();
        float x = InputTypeManager.InputActions["XAxis"].ReadValue<float>();
        float throttle = InputTypeManager.InputActions["ThrottleAxis"].ReadValue<float>();

        m_yAxis = y == 0 ? ReduceAxisValue(m_yAxis, y) : IncreaseAxisValue(m_yAxis, y);
        m_xAxis = x == 0 ? ReduceAxisValue(m_xAxis, x) : IncreaseAxisValue(m_xAxis, x);
        m_throttleAxis = throttle == 0 ? m_throttleAxis : IncreaseAxisValue(m_throttleAxis, throttle);

        EOnXAxis?.Invoke(m_xAxis);
        EOnYAxis?.Invoke(m_yAxis);
        EOnThrottleAxis?.Invoke(m_throttleAxis);
    }

    float ReduceAxisValue(float currVal, float dir)
    {
        if (Mathf.Abs(currVal) > 0.01)
            return currVal - (returnSpeed * Time.deltaTime * Mathf.Sign(currVal));
        else
            return 0;
    }

    float IncreaseAxisValue(float currVal, float dir)
    {
        if (Mathf.Abs(currVal) < 1)
            return currVal + (increaseSpeed * Time.deltaTime * dir);
        else
            return dir;

    }
}
