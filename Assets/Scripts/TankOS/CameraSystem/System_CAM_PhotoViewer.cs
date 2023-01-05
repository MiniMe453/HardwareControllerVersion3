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
        [Header("Camera Selector")]
        public TextMeshProUGUI[] selectedCameraNumbers;

        [Header("Photo Display")]
        public RawImage loadingPhoto;
        public RectTransform loadingPhotoMask;
        public RawImage photo;
        public GameObject overlay;
        public Texture2D blackTexture;
        
        [Header("Photo Metadata")]
        public TextMeshProUGUI photoName;
        public TextMeshProUGUI cameraName;
        public TextMeshProUGUI gpsCoords;

        [Header("Photo Database")]
        public GameObject photoDatabaseEntryPrefab;
        public RectTransform photoDatabaseTransform;
        private List<PhotoDatabaseEntry> m_photoDatabaseEntries = new List<PhotoDatabaseEntry>();

        private int m_currentPhotoCount;
        private Timer m_LoadPhotoTimer;

        protected override void Init()
        {
            System_CAM.EOnCameraPhotoTaken += OnPhotoTaken;
            System_CAM.EOnNewCameraSelected += OnNewCameraSelected;

            applicationInputs.AddAction("goup", binding:"<Keyboard>/upArrow");
            applicationInputs.AddAction("godown", binding:"<Keyboard>/downArrow");

            applicationInputs["goup"].performed += NavigateUp;
            applicationInputs["godown"].performed += NavigateDown;

            PhotoDatabaseEntry.EOnEntrySelected += OnDatabaseEntrySelected;
        }

        void OnPhotoTaken(Struct_CameraPhoto photo)
        {
            if(AppIsLoaded)
            {
                loadingPhoto.texture = m_photoDatabaseEntries[m_currentPhotoCount].PhotoMetadata.photo;
                m_photoDatabaseEntries[m_currentPhotoCount].DeselectEntry();
            }

            CreateNewDatabaseEntry(photo);
        }

        protected override void OnAppLoaded()
        {
            // LoadPhoto(cameraSystem.m_photos.Count - 1, true);
            // m_currentPhotoCount = cameraSystem.m_photos.Count - 1;
            UIManager.AddToViewport(canvas, 100);
            RoverOperatingSystem.SetUserControl(false);
            applicationInputs.Disable();
        }

        protected override void OnAppQuit()
        {
            UIManager.RemoveFromViewport(canvas);
            RoverOperatingSystem.SetUserControl(true);
        }

        void OnDatabaseEntrySelected(Struct_CameraPhoto photoMetadata)
        {
            LoadPhoto(photoMetadata, !AppIsLoaded);
        }

        void OnNewCameraSelected(CameraMode newMode)
        {
            for(int i = 0; i < selectedCameraNumbers.Length; i++)
            {
                if(i == (int)newMode)
                {
                    selectedCameraNumbers[i].color = Color.white;
                    continue;
                }

                selectedCameraNumbers[i].color = Color.gray;
            }
        }

        private void LoadPhotoUpdate(float secondElapsed, Vector3 loadingPos, Vector3 loadingMaskPos,bool loadFirstPhoto = false)
        {
            float percentage = secondElapsed / (loadFirstPhoto? GameSettings.PHOTO_LOAD_TIME : GameSettings.PHOTO_VIEWER_LOAD_TIME);

            if(loadFirstPhoto)
            {
                loadingPos.y = -409*percentage;
                loadingPhoto.transform.localPosition = loadingPos;    
            }
            else
            {
                loadingMaskPos.y = -409*percentage;
                loadingPos.y = 409*percentage;

                loadingPhotoMask.localPosition = loadingMaskPos;
                loadingPhoto.transform.localPosition = loadingPos;
            }
        }

        private void LoadPhoto(Struct_CameraPhoto photoToLoad, bool loadFirstPhoto = false)
        {
            overlay.SetActive(false);
            RoverOperatingSystem.SetArduinoEnabled(false);

            loadingPhoto.transform.localPosition = Vector3.zero;
            loadingPhoto.color = loadFirstPhoto? Color.black : Color.white;

            loadingPhotoMask.transform.localPosition = Vector3.zero;

            photo.texture = photoToLoad.photo;
            photoName.text = photoToLoad.name;
            cameraName.text = photoToLoad.camMode.ToString();
            gpsCoords.text = photoToLoad.gpsCoords;

            if(m_LoadPhotoTimer != null)
                m_LoadPhotoTimer.Cancel();

            m_LoadPhotoTimer = Timer.Register(loadFirstPhoto? GameSettings.PHOTO_LOAD_TIME : GameSettings.PHOTO_VIEWER_LOAD_TIME, 
                            onUpdate: secondsElapsed => LoadPhotoUpdate(secondsElapsed, Vector3.zero, Vector3.zero, loadFirstPhoto),
                            onComplete: () => {
                                overlay.SetActive(true);
                                applicationInputs.Enable();
                                RoverOperatingSystem.SetArduinoEnabled(true);
                            });
        }

        private void NavigateUp(InputAction.CallbackContext callback)
        {
            if(m_currentPhotoCount + 1 > m_photoDatabaseEntries.Count - 1)
                return;

            loadingPhoto.texture = m_photoDatabaseEntries[m_currentPhotoCount].PhotoMetadata.photo;
            m_photoDatabaseEntries[m_currentPhotoCount].DeselectEntry();
            m_currentPhotoCount++;
            m_photoDatabaseEntries[m_currentPhotoCount].SelectEntry();
            //LoadPhoto(m_currentPhotoCount, false);
        }

        private void NavigateDown(InputAction.CallbackContext callback)
        {
            if(m_currentPhotoCount - 1 < 0 )
                return;

            loadingPhoto.texture = m_photoDatabaseEntries[m_currentPhotoCount].PhotoMetadata.photo;
            m_photoDatabaseEntries[m_currentPhotoCount].DeselectEntry();
            m_currentPhotoCount--;
            m_photoDatabaseEntries[m_currentPhotoCount].SelectEntry();
            //LoadPhoto(m_currentPhotoCount, false);
        }

        private void CreateNewDatabaseEntry(Struct_CameraPhoto photoMetadata)
        {
            GameObject newEntryGO = Instantiate(photoDatabaseEntryPrefab, photoDatabaseTransform);
            PhotoDatabaseEntry newEntry = newEntryGO.GetComponent<PhotoDatabaseEntry>();
            newEntry.CreateEntry(photoMetadata, m_photoDatabaseEntries.Count);
            EOnAppUnloaded += newEntry.DeselectEntry;
            m_currentPhotoCount = newEntry.EntryIndex;
            m_photoDatabaseEntries.Add(newEntry);

            newEntry.SelectEntry();

            if(!AppIsLoaded)
                AppDatabase.LoadApp(AppID);
        }
    }
}

