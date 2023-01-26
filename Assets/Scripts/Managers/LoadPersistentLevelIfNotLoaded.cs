using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPersistentLevelIfNotLoaded : MonoBehaviour
{
    void Awake()
    {
        SceneManager.LoadScene("PersistentLevel", LoadSceneMode.Additive);
    }
}
