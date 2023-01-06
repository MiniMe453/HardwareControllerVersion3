using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class ShowMessageBoxTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Timer.Register(1f, () => {UIManager.ShowMessageBox("Testmessage box", Color.red, 1f);});
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
