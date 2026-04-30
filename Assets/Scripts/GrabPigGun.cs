using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPigGun : MonoBehaviour
{
    public OVRGrabber oVRGrabber;
    public bool isGrabbed = false;

    void Update()
    {
        if (oVRGrabber.grabbedObject != null)
        {
            // Debug.Log("Grabbed object: " + oVRGrabber.grabbedObject.name);
            isGrabbed = true;
        }
        else
        {
            isGrabbed = false;
        }
    }
}
