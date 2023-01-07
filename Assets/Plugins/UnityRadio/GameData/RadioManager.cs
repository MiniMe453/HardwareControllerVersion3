using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Radios Global Data", menuName = "Radio/Radios Global Data")]
public class RadioManager : ScriptableObject
{
    public enum ERadioTypes { FM, AM, Test, Other };
    public enum EAntennaTypes { Omnidirectional, Directional, BeamAntenna };
    private List<RadioTransmitter> m_RadioTransmitters = new List<RadioTransmitter>();

    public List<RadioTransmitter> RadioTransmitters { get { return m_RadioTransmitters; } }

    public void RegisterTransmitter(RadioTransmitter transmitter)
    {
        m_RadioTransmitters.Add(transmitter);
    }

    public void DeregisterTransmitter(RadioTransmitter transmitter)
    {
        m_RadioTransmitters.Remove(transmitter);
    }
}

