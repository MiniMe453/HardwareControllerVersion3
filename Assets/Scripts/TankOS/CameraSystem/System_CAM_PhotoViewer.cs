using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.OS;
using Rover.Systems;
using UnityEngine.UI;
using Rover.Can;
using UnityTimer;
using TMPro;
using UnityEngine.InputSystem;
using Rover.Settings;

namespace Rover.OS
{
    public class System_CAM_PhotoViewer : MonoBehaviourApplication
    {
        public Canvas canvas;
        public System_CAM cameraSystem;
        public RawImage loadingPhoto;
        public RectTransform loadingPhotoMask;
        public RawImage photo;
        public TextMeshProUGUI photoName;
        public GameObject overlay;
        public Texture2D blackTexture;
        private int m_currentPhotoCount;
        private Timer m_LoadPhotoTimer;

        protected override void Init()
        {
            System_CAM.EOnCameraPhotoTaken += OnPhotoTaken;

            applicationInputs.AddAction("goleft", binding:"<Keyboard>/leftArrow");
            applicationInputs.AddAction("goright", binding:"<Keyboard>/rightArrow");

            applicationInputs["goleft"].performed += NavigateLeft;
            applicationInputs["goright"].performed += NavigateRight;
        }

        void OnPhotoTaken(Struct_CameraPhoto photo)
        {
            LoadApp();
        }

        protected override void OnAppLoaded()
        {
            LoadPhoto(cameraSystem.m_photos.Count-1, true);
            m_currentPhotoCount = cameraSystem.m_photos.Count - 1;
            UIManager.AddToViewport(canvas, 100);
            RoverOperatingSystem.SetUserControl(false);
            applicationInputs.Disable();
        }

        protected override void OnAppQuit()
        {
            UIManager.RemoveFromViewport(canvas);
            RoverOperatingSystem.SetUserControl(true);
        }

        private void LoadPhotoUpdate(float secondElapsed, Vector3 loadingPos, Vector3 loadingMaskPos,bool loadFirstPhoto = false)
        {
            float percentage = secondElapsed / (loadFirstPhoto? GameSettings.PHOTO_LOAD_TIME : GameSettings.PHOTO_VIEWER_LOAD_TIME);

            if(loadFirstPhoto)
            {
                loadingPos.y = -500*percentage;
                loadingPhoto.transform.localPosition = loadingPos;    
            }
            else
            {
                loadingMaskPos.y = -500*percentage;
                loadingPos.y = 500*percentage;

                loadingPhotoMask.localPosition = loadingMaskPos;
                loadingPhoto.transform.localPosition = loadingPos;
            }
        }

        private void LoadPhoto(int index, bool loadFirstPhoto = false)
        {
            overlay.SetActive(false);

            loadingPhoto.transform.localPosition = Vector3.zero;
            loadingPhoto.color = loadFirstPhoto? Color.black : Color.white;

            loadingPhotoMask.transform.localPosition = Vector3.zero;

            photo.texture = cameraSystem.m_photos[index].photo;
            photoName.text = cameraSystem.m_photos[index].name;

            if(m_LoadPhotoTimer != null)
                m_LoadPhotoTimer.Cancel();

            m_LoadPhotoTimer = Timer.Register(loadFirstPhoto? GameSettings.PHOTO_LOAD_TIME : GameSettings.PHOTO_VIEWER_LOAD_TIME, 
                            onUpdate: secondsElapsed => LoadPhotoUpdate(secondsElapsed, Vector3.zero, Vector3.zero, loadFirstPhoto),
                            onComplete: () => {
                                overlay.SetActive(true);
                                applicationInputs.Enable();
                            });
        }

        private void NavigateLeft(InputAction.CallbackContext callback)
        {
            if(m_currentPhotoCount + 1 > cameraSystem.m_photos.Count - 1)
                return;

            loadingPhoto.texture = cameraSystem.m_photos[m_currentPhotoCount].photo;
            m_currentPhotoCount++;
            LoadPhoto(m_currentPhotoCount, false);
        }

        private void NavigateRight(InputAction.CallbackContext callback)
        {
            if(m_currentPhotoCount - 1 < 0 )
                return;

            loadingPhoto.texture = cameraSystem.m_photos[m_currentPhotoCount].photo;
            m_currentPhotoCount--;
            LoadPhoto(m_currentPhotoCount, false);
        }
    }
}

