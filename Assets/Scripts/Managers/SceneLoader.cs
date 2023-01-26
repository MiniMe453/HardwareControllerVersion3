using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rover.Arduino;
using UnityEngine.InputSystem;

public class SceneLoader : MonoBehaviour
{
    public InputActionMap inputActions;
    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.UnloadSceneAsync("StartupScene");

        inputActions["resetGame"].performed += OnResetGame;
        inputActions.Enable();
    }

    void OnSceneUnloaded(Scene unloadedScene)
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        if(!SceneManager.GetSceneByName("Mars_scene").isLoaded)
        {
            SceneManager.LoadScene("Mars_scene", LoadSceneMode.Additive);
        }

        Debug.LogError("Scene unloaded");
    }

    void OnResetGame(InputAction.CallbackContext context)
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.UnloadSceneAsync("Mars_scene");
    }
}
