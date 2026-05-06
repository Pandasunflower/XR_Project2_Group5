using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCrowdSystem;

public class MicrophoneDetection : MonoBehaviour
{
    public GPUI_CrowdGenerator crowdGenerator;

    void Awake()
    {
        if (crowdGenerator == null)
        {
            crowdGenerator = FindObjectOfType<GPUI_CrowdGenerator>();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Microphone"))
        {

            Debug.Log($"碰撞物件: {other.gameObject.name}");
            crowdGenerator.callChangeAnim(0, false);
        }
    }
}
