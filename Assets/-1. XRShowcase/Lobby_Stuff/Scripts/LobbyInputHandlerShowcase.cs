using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInputHandlerShowcase : MonoBehaviour
{
    [Header("Core References")]
    public LobbySongManager songManager;
    public LobbySceneManager sceneManager;

    [Header("Input Settings")]
    public float inputCooldown = 0.2f;
    private float _lastInputTime;

    // [Header("Controller Settings")]
    // [Range(0.1f, 0.9f)]
    // public float stickThreshold = 0.5f;

    void Update()
    {
        if (Time.time - _lastInputTime < inputCooldown) return;

        // Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        // Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        // Vector2 stickPos = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        // float h = Input.GetAxisRaw("Horizontal");
        // float h = stickPos.x;
        
        bool ButtonPressedNext = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);
        bool ButtonPressedPrev = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);

        // if (h < -stickThreshold || Input.GetKeyDown(KeyCode.A))
        // {
        //     songManager.PreviousSong();
        //     _lastInputTime = Time.time;
        //     songManager.UpdateUI();
        // }
        // else if (h > stickThreshold || Input.GetKeyDown(KeyCode.D))
        // {
        //     songManager.NextSong();
        //     _lastInputTime = Time.time;
        //     songManager.UpdateUI();
        // }

        if (ButtonPressedNext || Input.GetKeyDown(KeyCode.A))
        {
            songManager.NextSong();
            _lastInputTime = Time.time;
            songManager.UpdateUI();
        }
        if (ButtonPressedPrev || Input.GetKeyDown(KeyCode.D))
        {
            songManager.PreviousSong();
            _lastInputTime = Time.time;
            songManager.UpdateUI();
        }

        // if (ButtonPressed || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        // {
        //     sceneManager.RequestStartGame();
        //     _lastInputTime = Time.time;
        // }
    }
}