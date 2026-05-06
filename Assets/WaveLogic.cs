using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveLogic : MonoBehaviour
{
    public bool isWaveLeft = false;
    public bool isWaveRight = false;

    void Update()
    {
        if (isWaveLeft && isWaveRight)
        {
            Debug.Log("波浪舞");
            isWaveLeft = false;
            isWaveRight = false;
        }
    }
}
