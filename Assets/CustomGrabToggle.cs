using UnityEngine;

public class CustomGrabToggle : MonoBehaviour
{
    private OVRGrabber grabber;

    void Start()
    {
        grabber = GetComponent<OVRGrabber>();
    }

    void Update()
    {
        if (grabber == null) return;

        // 👉 按一下 trigger
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            ToggleGrab();
        }
    }

    void ToggleGrab()
    {
        if (grabber.grabbedObject == null)
        {
            ForceGrab();
        }
        else
        {
            ForceRelease();
        }
    }

    void ForceGrab()
    {
        // 直接呼叫 internal GrabBegin
        grabber.SendMessage("GrabBegin", SendMessageOptions.DontRequireReceiver);
    }

    void ForceRelease()
    {
        grabber.SendMessage("GrabEnd", SendMessageOptions.DontRequireReceiver);
    }
}