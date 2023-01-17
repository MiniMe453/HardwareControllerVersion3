using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Arduino;
using Uduino;
using Rover.Settings;
using Rover.Can;
using Rover.DateTime;
// using Rover.Interface;
using UnityEngine.UI;

namespace Rover.Systems
{
    public enum CameraMode {Cam1, Cam2, Cam3, Cam4};

    public struct Struct_CameraPhoto
    {
        public string name;
        public CameraMode camMode;
        public string gpsCoords;
        public Texture2D photo;
    }
    public class System_CAM : MonoBehaviour
    {
        private static CameraMode m_cameraMode = CameraMode.Cam1;
        public static CameraMode SelectedCameraMode {get {return m_cameraMode;}}
        int[] m_ledPins;
        public List<Camera> cameraList;
        public GameObject mainPhotoCamera;
        private static int m_screenshotCount;
        private static float m_heading;
        public static float Heading {get {return m_heading;}}
        private static List<Struct_CameraPhoto> m_photos = new List<Struct_CameraPhoto>();
        public static List<Struct_CameraPhoto> CameraPhotos {get {return m_photos;}}
        public static event System.Action<CameraMode> EOnNewCameraSelected;
        public static event System.Action<Struct_CameraPhoto> EOnCameraPhotoTaken;
        public Transform xAxisParent;
        private float m_verticalAxis;
        public float turnSpeed = 1f;
        static float m_camPitch;
        public static float CameraPitch {get{return m_camPitch;}}
        void OnEnable()
        {
            ArduinoInputDatabase.EOnDatabasedInitialized += OnDatabaseInit;
            RoverOperatingSystem.EOnRoverControlModeChanged += OnRoverControlModeChanged;
        }

        void OnDatabaseInit()
        {
            m_ledPins = new int[]{
                ArduinoInputDatabase.GetOutputIndexFromName("CAM 1 led pin"),
                ArduinoInputDatabase.GetOutputIndexFromName("cam 2 led pin"),
                ArduinoInputDatabase.GetOutputIndexFromName("cam 3 led pin"),
                ArduinoInputDatabase.GetOutputIndexFromName("cam 4 led pin")
            };

            LEDManager.SetLEDMode(m_ledPins, new int[] {0,0,0,0});

            ArduinoInputDatabase.GetInputFromName("CAM 1 Button").EOnButtonPressed += OnCam1ButtonPressed;
            ArduinoInputDatabase.GetInputFromName("CAM 2 Button").EOnButtonPressed += OnCam2ButtonPressed;
            ArduinoInputDatabase.GetInputFromName("CAM 3 Button").EOnButtonPressed += OnCam3ButtonPressed;
            ArduinoInputDatabase.GetInputFromName("CAM 4 Button").EOnButtonPressed += OnCam4ButtonPressed;
            ArduinoInputDatabase.GetInputFromName("CAM Take Photo Button").EOnButtonPressed += OnTakePhotoButtonPressed;
            ArduinoInputDatabase.GetInputFromName("Joystick Y").EOnValueChanged += OnVerticalAxis;
            
            SelectNewCameraMode(CameraMode.Cam1);
        }

        void OnRoverControlModeChanged(RoverControlMode newMode)
        {
            switch(newMode)
            {
                case RoverControlMode.RVR:
                    mainPhotoCamera.SetActive(false);
                    break;
                case RoverControlMode.CAM:
                    mainPhotoCamera.SetActive(true);
                    break;
            }
        }

        public void SelectNewCameraMode(CameraMode newMode)
        {
            if(newMode == SelectedCameraMode)
                return;

            m_cameraMode = newMode;

            int[] pinValues = new int[4];

            pinValues[(int)newMode] = 1;

            LEDManager.SetLEDMode(m_ledPins, pinValues);

            EOnNewCameraSelected?.Invoke(m_cameraMode);

            Debug.Log("Selected Rover Camera is now "+m_cameraMode);
        }

        void OnCam1ButtonPressed(int pin)
        {
            SelectNewCameraMode(CameraMode.Cam1);
        }

        void OnCam2ButtonPressed(int pin)
        {
            SelectNewCameraMode(CameraMode.Cam2);
        }

        void OnCam3ButtonPressed(int pin)
        {
            SelectNewCameraMode(CameraMode.Cam3);
        }

        void OnCam4ButtonPressed(int pin)
        {
            SelectNewCameraMode(CameraMode.Cam4);
        }

        void OnTakePhotoButtonPressed(int pin)
        {
            if(System_MTR.RoverVelocity > 0.01)
            {
                Debug.Log(System_MTR.RoverVelocity);
                UIManager.ShowMessageBox("STOP THE ROVER", Color.red, 1f);
            }
            else
            {
                TakeCameraPhoto(cameraList[(int)m_cameraMode]);
            }

            // TakeCameraPhoto(cameraList[(int)m_cameraMode]);
            // Debug.LogError("Take Photo Button pressed");
        }

        void OnVerticalAxis(float value, int pin)
        {
            m_verticalAxis = (value - GameSettings.VERTICAL_CENTER_VAL) / GameSettings.VERTICAL_CENTER_VAL;

            if(Mathf.Abs(m_verticalAxis) < GameSettings.JOYSTICK_DEADZONE)
                m_verticalAxis = 0;
        }

        void TakeCameraPhoto(Camera selectedCamera)
        {
            m_screenshotCount++;
            
            if(RoverOperatingSystem.RoverControlMode == RoverControlMode.CAM)
                mainPhotoCamera.SetActive(false);

            selectedCamera.gameObject.SetActive(true);

            RenderTexture rt = new RenderTexture(GameSettings.GAME_RES_X, GameSettings.GAME_RES_Y, 24);
            selectedCamera.targetTexture = rt;
            Texture2D cameraPhoto = new Texture2D(GameSettings.GAME_RES_X, GameSettings.GAME_RES_Y, TextureFormat.RGB24, false);
            selectedCamera.Render();
            RenderTexture.active = rt;
            
            cameraPhoto.ReadPixels(new Rect(0,0,GameSettings.GAME_RES_X, GameSettings.GAME_RES_Y),0,0);
            cameraPhoto.Apply();

            string photoName = $"{m_cameraMode.ToString()}_{TimeManager.ToStringMissionTimeClk(TimeManager.GetCurrentDateTime(), "_")}_{m_screenshotCount.ToString("000")}.bmp";

            Struct_CameraPhoto photoMetadata = new Struct_CameraPhoto();
            photoMetadata.name = photoName;
            photoMetadata.photo = cameraPhoto;
            photoMetadata.camMode = SelectedCameraMode;
            photoMetadata.gpsCoords = System_GPS.GPSCoordsToString(System_GPS.WorldPosToGPSCoords(transform.position));

            m_photos.Add(photoMetadata);

            selectedCamera.targetTexture = null;
            RenderTexture.active = null;
            selectedCamera.gameObject.SetActive(false);
            Destroy(rt);

            EOnCameraPhotoTaken?.Invoke(photoMetadata);
            
            if(RoverOperatingSystem.RoverControlMode == RoverControlMode.CAM)
                mainPhotoCamera.SetActive(true);    
        }

        void Update()
        {
            if(RoverOperatingSystem.RoverControlMode != RoverControlMode.CAM)
                return;

            Vector3 newRot = Vector3.right * turnSpeed * -m_verticalAxis;

            float eulerX = (xAxisParent.localRotation * Quaternion.Euler(newRot)).eulerAngles.x;

            if((eulerX - ((eulerX > 180)? 360 : 0)) < 70 && (eulerX - ((eulerX > 180)? 360 : 0)) > -70)
                xAxisParent.rotation *= Quaternion.Euler(newRot);

            m_camPitch = Vector3.SignedAngle(xAxisParent.forward, transform.up, xAxisParent.right) + 90;

            m_heading = Vector3.SignedAngle(transform.forward, new Vector3(0,0,1), Vector3.up);
            float sign = Mathf.Sign(m_heading);

            if(sign < 0)
                m_heading = 360 - Mathf.Abs(m_heading);
        }
    }

}

