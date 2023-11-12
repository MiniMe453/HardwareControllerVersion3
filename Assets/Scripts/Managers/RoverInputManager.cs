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
    public static InputAction YAxis;
    public static InputAction XAxis;
    public static InputAction ThrottleAxis;
    public static InputAction RadioAxis;
    public AudioSource audioSource;

    void Start()
    {
        m_InputActions = Resources.Load<InputActionAsset>("KeyboardActionAsset");
        RadioAxis = RoverInputManager.InputActions["RadioAxis"];
        YAxis = RoverInputManager.InputActions["YAxis"];
        XAxis = RoverInputManager.InputActions["XAxis"];
        ThrottleAxis = RoverInputManager.InputActions["ThrottleAxis"];



        gameInitializer.InitializeGame(m_useKeyboardInput);

        foreach(InputAction action in RoverInputManager.InputActions)
        {
            action.performed += PlaySoundOnPress;
        }
    }

    void PlaySoundOnPress(InputAction.CallbackContext context)
    {
        if(CommandConsoleMain.IsConsoleVisible)
            return;

        audioSource.Play();
    }
}
