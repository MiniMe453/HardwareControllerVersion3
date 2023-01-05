using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rover.Systems;
using TMPro;
using UnityEngine.UI;
using System;

public class PhotoDatabaseEntry : MonoBehaviour
{
    private Struct_CameraPhoto m_photoMetadata;
    public Struct_CameraPhoto PhotoMetadata {get {return m_photoMetadata;}}
    public TextMeshProUGUI photoNameText;
    public RawImage bgImage;
    private int m_entryIndex;
    public int EntryIndex {get{return m_entryIndex;}}
    public static event Action<Struct_CameraPhoto> EOnEntrySelected;
    
    public void CreateEntry(Struct_CameraPhoto cameraPhoto, int _entryIndex)
    {
        m_photoMetadata = cameraPhoto;
        m_entryIndex = _entryIndex;

        photoNameText.text = m_photoMetadata.name;
    }

    public Struct_CameraPhoto SelectEntry()
    {
        bgImage.color = Color.white;
        photoNameText.color = Color.black;

        EOnEntrySelected?.Invoke(PhotoMetadata);

        return PhotoMetadata;
    }

    public void DeselectEntry()
    {
        bgImage.color = Color.black;
        photoNameText.color = Color.white;
    }

}
