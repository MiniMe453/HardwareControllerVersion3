using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Systems;
using TMPro;
using UnityEngine.UI;

public class PhotoDatabaseEntry : MonoBehaviour
{
    private Struct_CameraPhoto m_photoMetadata;
    public Struct_CameraPhoto PhotoMetadata {get {return m_photoMetadata;}}
    public TextMeshProUGUI photoNameText;
    public RawImage bgImage;
    
    public void CreateEntry(Struct_CameraPhoto cameraPhoto)
    {
        m_photoMetadata = cameraPhoto;

        photoNameText.text = m_photoMetadata.name;
    }

    public Struct_CameraPhoto SelectEntry()
    {
        bgImage.color = Color.white;
        photoNameText.color = Color.black;

        return PhotoMetadata;
    }

    public void DeselectEntry()
    {
        bgImage.color = Color.black;
        photoNameText.color = Color.white;
    }

}
