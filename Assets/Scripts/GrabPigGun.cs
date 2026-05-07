using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPigGun : MonoBehaviour
{
    public OVRGrabber oVRGrabber;
    public bool isGrabbed = false;
    // public bool isLocked = false;

    void Update()
    {
        if (oVRGrabber.grabbedObject != null)
        {
            isGrabbed = true;
        }
        else
        {
            isGrabbed = false;
        }
    }
}
