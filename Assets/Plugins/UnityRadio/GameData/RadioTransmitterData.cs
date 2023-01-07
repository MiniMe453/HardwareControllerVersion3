using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "Radio Transmitter", menuName = "Radio/Radio Transmitter")]
public class RadioTransmitterData : ScriptableObject
{
    public float watts;

    public float antennaGain;

    public float frequency;

    public float cableLoss;

    public RadioManager.EAntennaTypes antennaType;

    public AudioMixerGroup mixerGroup;
}
