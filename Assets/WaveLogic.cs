using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCrowdSystem;

public class WaveLogic : MonoBehaviour
{
    public bool isWaveLeft = false;
    public bool isWaveRight = false;
    public GPUI_CrowdGenerator crowdGenerator;

    void Awake()
    {
        if (crowdGenerator == null)
        {
            crowdGenerator = FindObjectOfType<GPUI_CrowdGenerator>();
        }
    }

    void Update()
    {
        if (isWaveLeft || isWaveRight)
        {
            // Debug.Log($"WaveLogic 狀態 - isWaveLeft: {isWaveLeft}, isWaveRight: {isWaveRight}");
        }
        if (isWaveLeft && isWaveRight)
        {
            crowdGenerator.TriggerBigWave();
            Debug.Log("波浪舞");
            isWaveLeft = false;
            isWaveRight = false;
        }
    }
}
