using System.Collections;
using System.Collections.Generic;
// using System.Diagnostics;
using UnityEngine;

public class PointDetection : MonoBehaviour
{
    public PoseDetection poseDetection;
    void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("LeftHand"))
        // {
        //     poseDetection.leftIn = true;
        // }
        // else if (other.CompareTag("RightHand"))
        // {
        //     poseDetection.rightIn = true;
        // }
        if (other.CompareTag("LeftHand") || other.CompareTag("RightHand"))
        {
            // Debug.Log($"poseDetection 碰撞物件: {other.gameObject.name}");
            poseDetection.isIn = true;
            poseDetection.Tag = gameObject.tag == "keepJumpingClip" ? 1 : 2;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // if (other.CompareTag("LeftHand"))
        // {
        //     poseDetection.leftIn = false;
        // }
        // else if (other.CompareTag("RightHand"))
        // {
        //     poseDetection.rightIn = false;
        // }
        if (other.CompareTag("LeftHand") || other.CompareTag("RightHand"))
        {
            // Debug.Log($"poseDetection 離開物件: {other.gameObject.name}");
            poseDetection.isIn = false;
            poseDetection.Tag = 2;
        }
    }
}
