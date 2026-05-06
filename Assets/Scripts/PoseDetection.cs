using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseDetection : MonoBehaviour
{
    public bool leftIn = false;
    public bool rightIn = false;

    void Update()
    {
        if (leftIn && rightIn)
        {
            Debug.Log("雙手舉高");
            leftIn = false;
            rightIn = false;
        }
        else if (leftIn)
        {
            Debug.Log("左手舉高");
            leftIn = false;
        }
        else if (rightIn)
        {
            Debug.Log("右手舉高");
            rightIn = false;
        }
    }
}
