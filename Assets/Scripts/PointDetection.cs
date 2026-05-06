using System.Collections;
using System.Collections.Generic;
// using System.Diagnostics;
using UnityEngine;

public class PointDetection : MonoBehaviour
{
    public PoseDetection poseDetection;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftHand"))
        {
            poseDetection.leftIn = true;
        }
        else if (other.CompareTag("RightHand"))
        {
            poseDetection.rightIn = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftHand"))
        {
            poseDetection.leftIn = false;
        }
        else if (other.CompareTag("RightHand"))
        {
            poseDetection.rightIn = false;
        }
    }
}
