using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneDetection : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Microphone"))
        {
            Debug.Log($"碰撞物件: {other.gameObject.name}");
        }
    }
}
