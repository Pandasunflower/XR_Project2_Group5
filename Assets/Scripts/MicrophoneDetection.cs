using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCrowdSystem;

public class MicrophoneDetection : MonoBehaviour
{
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
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Microphone"))
        {
            if (animationManager != null)
            {
                animationManager.StartCoroutine(animationManager.SetCrowdAnimators(0));
            }
            else if (crowdGenerator != null)
            {
                crowdGenerator.callChangeAnim(0, false);
            }
        }
        
    }
}
