using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rover.Settings;

public class RenderCameraToTexture : MonoBehaviour
{
    public Camera lowResCamera;
    public CustomRenderTexture renderTexture;
    private float m_camPhotoFpsCounter;

    void OnEnable()
    {
        
    }

    void Update()
    {
        m_camPhotoFpsCounter += Time.deltaTime;

        if(!lowResCamera.gameObject.activeSelf)
            return;

        if(m_camPhotoFpsCounter > 1/GameSettings.PHOTO_VIEWER_FPS)
        {
            m_camPhotoFpsCounter = 0;

            lowResCamera.enabled = true;
            lowResCamera.targetTexture = renderTexture;

            Texture2D cameraPhoto = new Texture2D(GameSettings.GAME_RES_X / 2, GameSettings.GAME_RES_Y / 2, TextureFormat.RGB24, false);
            lowResCamera.Render();
            RenderTexture.active = renderTexture;
            
            cameraPhoto.ReadPixels(new Rect(0,0,GameSettings.GAME_RES_X / 2, GameSettings.GAME_RES_Y / 2),0,0);
            cameraPhoto.Apply();

            lowResCamera.targetTexture = null;

            lowResCamera.enabled = false;
        }   
    }
}
