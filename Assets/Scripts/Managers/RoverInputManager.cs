using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoverInputManager : MonoBehaviour
{
    private static bool m_useKeyboardInput = true;
    public static bool UseKeyboardInput { get { return m_useKeyboardInput; } }
    public GameInitializer gameInitializer => GetComponent<GameInitializer>();
    private static InputActionAsset m_InputActions;
    public static InputActionAsset InputActions { get { return m_InputActions; } }

    void Start()
    {
        m_InputActions = Resources.Load<InputActionAsset>("KeyboardActionAsset");
        gameInitializer.InitializeGame(m_useKeyboardInput);
    }
}
