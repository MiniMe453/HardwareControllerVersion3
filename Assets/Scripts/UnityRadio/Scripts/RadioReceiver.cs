using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class RadioReceiver : MonoBehaviour
{

    private float LAMBDA = (Mathf.Pow(10, 8) * 3);

    [SerializeField]
    private RadioReceiverData receiverData;
    public RadioReceiverData RecieverData {get {return receiverData;}}
    [SerializeField]
    private RadioManager m_RadioManager;
    public Transform antennaLocation;
    public AudioSource staticAudio;
    private AudioAnalyzer staticAudioAnalyzer;
    private AudioLowPassFilter lowPassFilter;

    public RadioManager.ERadioTypes radioType;
    public RadioManager.EAntennaTypes antennaType;

    //Smooth out the realtime data values
    private float[] movingAverageArr;
    private int movingAverageArrStep;

    private float currentUpdateTime;
    private float updateStep = 0.05f;

    void Start()
    {
        //Set up the other required data
        movingAverageArr = new float[5];
        staticAudioAnalyzer = new AudioAnalyzer(32, staticAudio);
        receiverData.RadioType = radioType;
        receiverData.AntennaType = antennaType;
    }
    void Update()
    {

        if (receiverData.AntennaType != antennaType)
        {
            receiverData.AntennaType = antennaType;
        }

        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep)
        {
            bool setStaticVolume = true;

            currentUpdateTime = 0f;

            foreach (RadioTransmitter transmitter in m_RadioManager.RadioTransmitters)
            {
                float signalVolume = CalculateSignalVolume(transmitter);

                if (signalVolume != 0f)
                {
                    setStaticVolume = false;
                    // signalVolume = CalculateObstructions(signalVolume, transmitter);
                    // signalVolume *= CalculateRecieverAngle(transmitter);
                    staticAudio.volume = 1 - signalVolume;
                    receiverData.signalStrength = signalVolume;
                    transmitter.SetSignalStrength(signalVolume);
                    ChartAudioSignalValues.UpdateClipLoundess(signalVolume);
                }

                if (setStaticVolume)
                {
                    transmitter.SetSignalStrength(0);
                }
            }

            if (setStaticVolume)
            {
                staticAudio.volume = 1;
                receiverData.signalStrength = 0;
                ChartAudioSignalValues.UpdateClipLoundess(0);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (m_RadioManager.RadioTransmitters.Count != 0)
        {
            foreach (RadioTransmitter transmitter in m_RadioManager.RadioTransmitters)
            {
                Gizmos.DrawWireSphere(transmitter.GetTransmitterLocation(), CalculateCoverage(transmitter));
                Debug.Log(CalculateCoverage(transmitter));
            }
        }
    }

    public float CalculateCoverage(RadioTransmitter transmitter)
    {
        float eirp = transmitter.GetPt() - transmitter.GetCableLoss() + transmitter.GetGt();
        float fsl = eirp - receiverData.sensitivity;
        float dist = fsl + 20f * Mathf.Log10(LAMBDA / transmitter.GetFrequency()) - 21.98f;
        return Mathf.Pow(10f, dist / 20f) / 100000000f;
    }

    //Calculates the signal strength based on the distance from the transmitter
    private float CalculateDistanceStrength(RadioTransmitter transmitter)
    {
        return Mathf.Clamp(4 - (Vector3.Distance(antennaLocation.position, transmitter.GetTransmitterLocation()) / CalculateCoverage(transmitter)) * 4, 0, 4);
    }

    //Calculates the signal strength modifier based on the current receiver frequency
    //Works like this: 
    //modifier is 1 when the receiver has the same frequency as the transmitter
    //when the receiver frequency is > receiverData.FrequencyBand away from transmitter frequency, the modifier will be 0
    private float CalculateFrequencyStrength(RadioTransmitter transmitter)
    {
        return Mathf.Clamp01((receiverData.frequencyBand - Mathf.Abs(transmitter.GetFrequency() - receiverData.Frequency)) / receiverData.frequencyBand);
    }

    //Calculates the final volume
    //Uses a moving average to smooth out the data
    private float CalculateSignalVolume(RadioTransmitter transmitter)
    {
        float rawVolume = Mathf.Clamp01(Mathf.Log10(transmitter.GetAverageAmplitude() / staticAudioAnalyzer.AverageAmplitude()));
        float smoothedVolume = UpdateMovingAverage(rawVolume);
        float signalStrength = CalculateDistanceStrength(transmitter) * CalculateFrequencyStrength(transmitter);


        //When the signal strength is low, the volume will blend between the signal and static
        //When the signal strength is high, the volume will be weighted towards the main signal (this prevents audio problems)
        return Mathf.Clamp01(((smoothedVolume * signalStrength) * (1 - signalStrength / 4)) + signalStrength / 4);
    }

    //Calculate the signal modifier based on how many obstructions are in the way
    //Is a simple calculation
    private float CalculateObstructions(float signalVolume, RadioTransmitter transmitter)
    {
        RaycastHit[] hits;

        hits = Physics.RaycastAll(antennaLocation.position, (transmitter.GetTransmitterLocation() - antennaLocation.position).normalized, Vector3.Distance(antennaLocation.position, transmitter.GetTransmitterLocation()));

        if (hits.Length != 0)
        {
            //Make this so that it's more dependent on the distance from the transmitter? A stronger signal
            //will go through more walls, while a weaker signal will get messed up faster as a result of the walls.
            //The greater the distance, the more the signal needs to be modulated.
            signalVolume = Mathf.Clamp01(signalVolume - hits.Length * 0.1f);
            return signalVolume;
        }

        return signalVolume;
    }
    private float CalculateRecieverAngle(RadioTransmitter transmitter)
    {
        Vector3 tPos = (transmitter.GetTransmitterLocation() - antennaLocation.position).normalized;
        Vector3 rPos = antennaLocation.forward;

        float angle = Mathf.Acos(Mathf.Clamp((rPos.x * tPos.x + rPos.z * tPos.z + rPos.y * tPos.y) / (Mathf.Sqrt(Mathf.Pow(rPos.x, 2) + Mathf.Pow(rPos.y, 2) + Mathf.Pow(rPos.z, 2)) * Mathf.Sqrt(Mathf.Pow(tPos.x, 2) + Mathf.Pow(rPos.y, 2) + Mathf.Pow(tPos.z, 2))), -1, 1));

        angle = (3 - Mathf.Clamp(angle, 0, 3)) / 3;

        switch (receiverData.AntennaType)
        {
            case RadioManager.EAntennaTypes.Omnidirectional:
                return 1f;
            case RadioManager.EAntennaTypes.Directional:
                return angle;
            case RadioManager.EAntennaTypes.BeamAntenna:
                return Mathf.Pow(angle, 7);
        }
        return 1f;
    }
    private float MovingAverageFilter(float[] valueArr)
    {
        float average = 0f;
        foreach (float n in valueArr)
        {
            average += n;
        }

        return average / valueArr.Length;
    }
    private float UpdateMovingAverage(float inputValue)
    {
        movingAverageArr[movingAverageArrStep] = inputValue;
        if (movingAverageArrStep != movingAverageArr.Length - 1) movingAverageArrStep++;
        else movingAverageArrStep = 0;

        return MovingAverageFilter(movingAverageArr);
    }
}
