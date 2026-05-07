using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRPlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    Vector3 appliedVelocity = Vector3.zero;
    public float gravity = -9.81f;

    void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }


    void Update()
    {
        // gravity
        appliedVelocity.y += gravity * Time.deltaTime;
        Vector3 move = appliedVelocity * Time.deltaTime;
        //Debug.Log($"Applying move: {move}, velocity: {appliedVelocity}");
        characterController.Move(move);

        // determine grounded either via layer-based check or CharacterController.isGrounded
        bool isGrounded = characterController.isGrounded;

        // when grounded, clear downward vertical velocity so we don't keep falling
        if (isGrounded && appliedVelocity.y < 0)
        {
            appliedVelocity.z = 0f;
            appliedVelocity.y = -1f;
        }
    }
}
