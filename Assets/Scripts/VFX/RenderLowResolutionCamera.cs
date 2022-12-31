using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rover.Settings;

public class RenderLowResolutionCamera : MonoBehaviour
{
    public Camera lowResCamera;
    public RawImage cameraOutputImage;
    private float m_camPhotoFpsCounter;

    void Update()
    {
        m_camPhotoFpsCounter += Time.deltaTime;

        if(!lowResCamera.gameObject.activeSelf)
            return;

        if(m_camPhotoFpsCounter > 1/GameSettings.PHOTO_VIEWER_FPS)
        {
            m_camPhotoFpsCounter = 0;

            lowResCamera.enabled = true;

            RenderTexture rt = new RenderTexture(GameSettings.GAME_RES_X / 2, GameSettings.GAME_RES_Y / 2, 24);
            lowResCamera.targetTexture = rt;
            Texture2D cameraPhoto = new Texture2D(GameSettings.GAME_RES_X / 2, GameSettings.GAME_RES_Y / 2, TextureFormat.RGB24, false);
            lowResCamera.Render();
            RenderTexture.active = rt;
            
            cameraPhoto.ReadPixels(new Rect(0,0,GameSettings.GAME_RES_X / 2, GameSettings.GAME_RES_Y / 2),0,0);
            cameraPhoto.Apply();

            cameraOutputImage.texture = cameraPhoto;

            lowResCamera.enabled = false;
        }   
    }
}
