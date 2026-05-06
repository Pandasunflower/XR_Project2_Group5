using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveDetection : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wave"))
        {
            Debug.Log($"wave碰撞物件: {other.gameObject.name}");
        }
    }
}
