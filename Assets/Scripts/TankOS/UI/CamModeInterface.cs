using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rover.Arduino;
using Rover.Systems;
public class CamModeInterface : MonoBehaviour
{
    public TextMeshProUGUI[] cameraNumbers;
    public RectTransform photoDatabaseTransform;
    public GameObject photoDatabaseEntry;
    private List<GameObject> m_photoDatabaseEntryObjects = new List<GameObject>();
    public TextMeshProUGUI pitchText;
    public RectTransform pitchImageTransform;
    public TextMeshProUGUI headingText;
    public Image cam1PreviewImage;
    public Image cam2PreviewImage;
    public TextMeshProUGUI cam1Text;
    public TextMeshProUGUI cam2Text;
    void Awake()
    {
        System_CAM.EOnNewCameraSelected += OnNewCameraSelected;
        System_CAM.EOnCameraPhotoTaken += OnCameraPhotoTaken;
    }

    void OnNewCameraSelected(CameraMode newMode)
    {
        for(int i = 0; i < cameraNumbers.Length; i++)
        {
            cameraNumbers[i].color = i == (int)newMode? Color.white : Color.gray;
        }

        cam1PreviewImage.enabled = newMode != CameraMode.Cam2;
        cam1Text.enabled = newMode != CameraMode.Cam2;
        cam2PreviewImage.enabled = newMode == CameraMode.Cam2;
        cam2Text.enabled = newMode == CameraMode.Cam2;

        cam1Text.text = "CAM" + (((int)newMode) + 1).ToString() + " PREVIEW";
    }

    void Update()
    {
        if(RoverOperatingSystem.OSMode != OSMode.Rover)
            return;

        pitchImageTransform.anchoredPosition = new Vector2(0,System_CAM.CameraPitch * 2f);
        pitchText.text = "PTCH: " + (System_CAM.CameraPitch.ToString("0.0")).PadLeft(6);
        headingText.text = "HDNG: " + System_GPS.Heading.ToString("0.0").PadLeft(6);
    }

    void OnCameraPhotoTaken(Struct_CameraPhoto newPhoto)
    {
        SetCameraPhotoDatabaseEntries();
    }

    void OnEnable()
    {
        SetCameraPhotoDatabaseEntries();
    }

    void SetCameraPhotoDatabaseEntries()
    {
        if(System_CAM.CameraPhotos.Count == 0 && m_photoDatabaseEntryObjects.Count == 0)
        {
            GameObject location = Instantiate(photoDatabaseEntry, photoDatabaseTransform);
            location.GetComponent<TextMeshProUGUI>().text = "PHOTO";
            m_photoDatabaseEntryObjects.Add(location);


            GameObject location1 = Instantiate(photoDatabaseEntry, photoDatabaseTransform);
            location1.GetComponent<TextMeshProUGUI>().text = "BUTTON TO TAKE A";
            m_photoDatabaseEntryObjects.Add(location1);

            GameObject location2 = Instantiate(photoDatabaseEntry, photoDatabaseTransform);
            location2.GetComponent<TextMeshProUGUI>().text = "PRESS THE TKPH";
            m_photoDatabaseEntryObjects.Add(location2);

            return;
        }

        if(System_CAM.CameraPhotos.Count == 0)
            return;

        if(m_photoDatabaseEntryObjects.Count > 0)
        {
                for(int i =0; i < m_photoDatabaseEntryObjects.Count;i++)
                {
                    DestroyImmediate(m_photoDatabaseEntryObjects[i]);   
                }

                m_photoDatabaseEntryObjects.Clear();
        }

        foreach(Struct_CameraPhoto photo in System_CAM.CameraPhotos)
        {
                GameObject location = Instantiate(photoDatabaseEntry, photoDatabaseTransform);
                location.GetComponent<TextMeshProUGUI>().text = photo.name;
                location.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                m_photoDatabaseEntryObjects.Add(location);
        }
    }
}
