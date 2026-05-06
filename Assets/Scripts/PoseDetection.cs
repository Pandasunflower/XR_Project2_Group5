using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCrowdSystem;

public class PoseDetection : MonoBehaviour
{
    public bool leftIn = false;
    public bool rightIn = false;

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
        if (rightIn)
        {
            crowdGenerator.callChangeAnim(2, true);
            Debug.Log("右手舉高");
            rightIn = false;
        }
    }
}
