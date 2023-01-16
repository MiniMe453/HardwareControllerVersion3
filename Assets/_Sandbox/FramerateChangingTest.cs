using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class FramerateChangingTest : MonoBehaviour
{
    private int bounceValue = 1; 
    // Start is called before the first frame update
    private bool GoUp;
    void Start()
    {
       QualitySettings.vSyncCount = 0;
       Application.targetFrameRate = 30; 

       Timer.Register(1f, () => SetFramerate(), isLooped: true);
    }

    void SetFramerate()
    {
        if(Application.targetFrameRate < 5)
            GoUp = true;
        else if (Application.targetFrameRate > 30)
            GoUp = false;

        Application.targetFrameRate += GoUp? 1 : -1;

        Debug.LogError(Application.targetFrameRate);
    }
}
