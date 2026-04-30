using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPigGun : MonoBehaviour
{
    public OVRGrabber oVRGrabber;
    public bool isGrabbed = false;

    void Update()
    {
        if (oVRGrabber.grabbedObject == gameObject)
        {
            isGrabbed = true;
        }
        else
        {
            isGrabbed = false;
        }
    }
}
