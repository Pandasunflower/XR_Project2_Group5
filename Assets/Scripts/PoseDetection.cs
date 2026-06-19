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
    public AnimationManager animationManager;

    void Awake()
    {
        if (crowdGenerator == null)
            crowdGenerator = FindObjectOfType<GPUI_CrowdGenerator>();

        if (animationManager == null)
            animationManager = FindObjectOfType<AnimationManager>();
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
                TriggerAnimation(1);
                isIn = false;
                Tag = 0;
            }
            else if (Tag == 2)
            {
                TriggerAnimation(2);
                isIn = false;
                Tag = 0;
            }
        }
    }

    private void TriggerAnimation(int index)
    {
        int new_index;

        switch (index)
        {
            case 1:
                new_index = 2;
                break;
            case 2:
                new_index = 3;
                break;
            default:
                new_index = 0;
                break;
        }

        if (animationManager != null)
        {
            animationManager.StartCoroutine(animationManager.SetCrowdAnimators(new_index));
            return;
        }
        
        if (crowdGenerator != null)
        {
            crowdGenerator.callChangeAnim(index, false);
            return;
        }

        Debug.LogWarning("[PoseDetection] 沒有找到 crowdGenerator 也沒有 animationManager！");
    }
}
