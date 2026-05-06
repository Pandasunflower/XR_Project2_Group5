using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveDetection : MonoBehaviour
{
    public bool leftorright = false; // true: left, false: right
    public WaveLogic waveLogic;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wave"))
        {
            Debug.Log($"wave碰撞物件: {other.gameObject.name}");
            if (leftorright)
            {
                waveLogic.isWaveLeft = true;
            }
            else
            {
                waveLogic.isWaveRight = true;
            }
        }
    }
}
