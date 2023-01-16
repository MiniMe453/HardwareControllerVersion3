using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureMonobehaviour : MonoBehaviour
{
    public float Temperature;
    private TemperatureSimNode temperatureSimNode;
    void OnAwake()
    {
        temperatureSimNode = new TemperatureSimNode(Temperature, transform.position);
    }
}
