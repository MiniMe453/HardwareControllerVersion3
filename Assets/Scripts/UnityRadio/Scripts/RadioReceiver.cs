using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using UnityEngine;
using UnityEngine.Audio;

public struct Struct_RadioScan
{
    public RadioManager.ERadioTypes radioType;
    public float frequency;
    public int strength;
}

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
    private int numRadiosPerStep = 2;
    private int currentStepCount = 0;
    private int maxStepCounts = 0;
    private bool setStaticVolume = true;
    private Command RADIO_SCAN;

    void Start()
    {
        //Set up the other required data
        movingAverageArr = new float[5];
        staticAudioAnalyzer = new AudioAnalyzer(32, staticAudio);
        receiverData.RadioType = radioType;
        receiverData.AntennaType = antennaType;

        RADIO_SCAN = new Command(
            "SYS.RDIO.SCAN",
            "Peform a freq scan",
            "SYS.RDIO.SCAN",
            CmdPerformRadioScan
        );

        foreach(RadioTransmitter transmitter in m_RadioManager.RadioTransmitters)
        {
            transmitter.SetSignalStrength(0);
        }

        maxStepCounts = Mathf.CeilToInt((float)m_RadioManager.RadioTransmitters.Count/(float)numRadiosPerStep);
    }
    void Update()
    {
        if(m_RadioManager.RadioTransmitters.Count == 0)
            return;

        if (receiverData.AntennaType != antennaType)
        {
            receiverData.AntennaType = antennaType;
        }

        currentUpdateTime += Time.deltaTime;

        if (currentUpdateTime < updateStep)
            return;

        currentUpdateTime = 0f;
        int startIdx = currentStepCount * numRadiosPerStep;

        for(int i = startIdx; i < startIdx + numRadiosPerStep; i++)
        {
            if(i > m_RadioManager.RadioTransmitters.Count - 1)
            {
                break;
            }

            RadioTransmitter transmitter = m_RadioManager.RadioTransmitters[i];
            float signalVolume = CalculateSignalVolume(transmitter);
            // Debug.Log(signalVolume);

            if(signalVolume == 0f)
            {
                if(transmitter.IsAudioSourceEnabled())
                {
                    transmitter.SetSignalStrength(0);
                    transmitter.DisableAudioSource();
                }

                continue;
            }

            if (signalVolume != 0f)
            {
                setStaticVolume = false;
                // signalVolume = CalculateObstructions(signalVolume, transmitter);
                // signalVolume *= CalculateRecieverAngle(transmitter);
                staticAudio.volume = 1-signalVolume;
                receiverData.signalStrength = signalVolume;
                transmitter.SetSignalStrength(signalVolume);
                ChartAudioSignalValues.UpdateClipLoundess(signalVolume);
                System_RDIO.SetSignalStrength(signalVolume);

                if(!transmitter.IsAudioSourceEnabled())
                    transmitter.EnableAudioSource();

                continue;
            }
        }

        currentStepCount++;

        if (setStaticVolume && currentStepCount == maxStepCounts)
        {
            staticAudio.volume = 1;
            receiverData.signalStrength = 0;
            ChartAudioSignalValues.UpdateClipLoundess(0);
            System_RDIO.SetSignalStrength(0);
        }

        if(currentStepCount == maxStepCounts)
        {
            setStaticVolume = true;
            currentStepCount = 0;
        }
    }

    void OnDrawGizmos()
    {
        if (m_RadioManager.RadioTransmitters.Count != 0)
        {
            foreach (RadioTransmitter transmitter in m_RadioManager.RadioTransmitters)
            {
                Gizmos.DrawWireSphere(transmitter.GetTransmitterLocation(), CalculateCoverage(transmitter));
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
        return Mathf.Clamp01((smoothedVolume * signalStrength * (1 - signalStrength / 4)) + signalStrength / 4);
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
        for(int i = 0; i < valueArr.Length; i++)
        {
            average += valueArr[i];
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

    private void CmdPerformRadioScan()
    {
        List<Struct_RadioScan> radioScanData = new List<Struct_RadioScan>();

        foreach(RadioTransmitter transmitter in m_RadioManager.RadioTransmitters)
        {
            float distanceStrength = CalculateDistanceStrength(transmitter);
            
            if(distanceStrength <= 0f)
                continue;

            Struct_RadioScan scanResult = new Struct_RadioScan();
            scanResult.strength = Mathf.CeilToInt(distanceStrength * 25f);
            scanResult.radioType = transmitter.TransmitterData.radioType;
            scanResult.frequency = transmitter.TransmitterData.frequency;

            float heading = Vector3.SignedAngle((transmitter.transform.position - transform.position), new Vector3(0,0,1), Vector3.up) - 180f;
            float sign = Mathf.Sign(heading);

            Debug.LogError((transmitter.transform.position.ToString() + "  " + transform.position));


            if(sign < 0)
                heading = 360 - Mathf.Abs(heading);

            scanResult.strength = (int)heading;

            radioScanData.Add(scanResult);
        }

        System_RDIO.Instance.SetScanResult(radioScanData);
    }
}
