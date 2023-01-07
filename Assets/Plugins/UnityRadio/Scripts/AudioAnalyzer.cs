using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAnalyzer
{

    private float clipLoudness;
    private float[] clipSampleArr;
    private AudioSource audioClip;

    public AudioAnalyzer(int sampleLength, AudioSource clip)
    {
        audioClip = clip;
        clipSampleArr = new float[sampleLength];
    }
    public float AverageAmplitude()
    {
        audioClip.clip.GetData(clipSampleArr, audioClip.timeSamples);
        clipLoudness = 0f;

        foreach (var sample in clipSampleArr)
        {
            clipLoudness += Mathf.Abs(sample);
        }
        return clipLoudness /= clipSampleArr.Length;
    }
}
