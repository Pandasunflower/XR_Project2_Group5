using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCrowdSystem;

public class WaveLogic : MonoBehaviour
{
    public bool isWaveLeft = false;
    public bool isWaveRight = false;
    public GPUI_CrowdGenerator crowdGenerator;
    public AnimationManager animationManager;

    void Awake()
    {
        if (crowdGenerator == null)
        {
            crowdGenerator = FindObjectOfType<GPUI_CrowdGenerator>();
        }

        if (animationManager == null)
        {
            animationManager = FindObjectOfType<AnimationManager>();
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
            if (animationManager != null)
            {
                animationManager.StartCoroutine(animationManager.SetWaveCrowdAnimators(1));
            }
            else if (crowdGenerator != null)
            {
                crowdGenerator.TriggerBigWave();
            }
            Debug.Log("波浪舞");
            isWaveLeft = false;
            isWaveRight = false;
        }
    }
}
