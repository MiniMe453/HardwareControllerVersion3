using System.Collections;
using System.Collections.Generic;
using Rover.Settings;
using UnityEngine;

public class RenderObjectStencil : MonoBehaviour
{
    public RenderTexture RT;
    public Camera SecondCamera;//Second Camera Renders Normal that store to RenderTexture

    public Shader shader;

    void Update(){
    }

    void OnRenderImage(RenderTexture source,RenderTexture target){

        int resWidth = 400;
        int resHeight = 300;

        // RT = new RenderTexture(resWidth, resHeight, 24);

        // SecondCamera.targetTexture = RT; //Create new renderTexture and assign to camera
        // Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false); //Create new texture

        // RenderSettings.ambientLight = Color.white;
        SecondCamera.Render();
        // RenderSettings.ambientLight = GameSettings.DEFAULT_AMBIENT_COLOR;

        // RenderTexture.active = RT;
        // screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0); //Apply pixels from camera onto Texture2D

        // SecondCamera.targetTexture = null;
        // RenderTexture.active = null; //Clean

        // Material mat = new Material(shader);

        // mat.SetTexture ("_NormalScene", RT);

        Graphics.Blit (source, target);


        //Destroy(RT); //Free memory
    }
}
