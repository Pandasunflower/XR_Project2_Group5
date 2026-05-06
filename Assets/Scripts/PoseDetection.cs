using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCrowdSystem;

public class PoseDetection : MonoBehaviour
{
    public bool leftIn = false;
    public bool rightIn = false;
    public bool isIn = false;
    public int Tag = 0;

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
        // if (leftIn && rightIn)
        // {
        //     Debug.Log("雙手舉高");
        //     leftIn = false;
        //     rightIn = false;
        // }
        // else if (leftIn)
        // {
        //     Debug.Log("左手舉高");
        //     leftIn = false;
        // }
        // else if (rightIn)
        // {
        //     Debug.Log("右手舉高");
        //     rightIn = false;
        // }
        // Debug.Log($"PoseDetection 狀態 - leftIn: {leftIn}, rightIn: {rightIn}, isIn: {isIn}");
        if (isIn)
        {
            Debug.Log($"PoseDetection 狀態 Tag: {Tag}");
            if (Tag == 1)
            {
                crowdGenerator.callChangeAnim(1, false);
                Debug.Log("keepJumpingClip");
                isIn = false;
                Tag = 0;
            }
            else if (Tag == 2)
            {
                crowdGenerator.callChangeAnim(2, false);
                Debug.Log("rightLeftDanceClip");
                isIn = false;
                Tag = 0;
            }
        }
    }
}
