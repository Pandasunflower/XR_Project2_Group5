using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Unity.Mathematics;

public class ShootingController : MonoBehaviour
{
    public float bulletSpeed = 10f;
    public GameObject bulletPrefab;
    public GameObject shootingPoint;

    [Range(0.1f, 1.0f)]
    public float speedH = 1.0f;
    [Range(0.1f, 1.0f)]
    public float speedV = 1.0f;

    public float virbrationStrength = 0.5f;
    public float virbrationDuration = 0.2f;

    public GrabPigGun grabPigGun;

    public quaternion bulletRotation = quaternion.Euler(0, 180f, 0);

    public PlayWwiseEvent shootSound;

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && grabPigGun.isGrabbed)
        {
            OnFire();
        }
    }

    void OnFire()
    {
        Quaternion rot = shootingPoint.transform.rotation * Quaternion.Euler(0, 180f, 0);
        GameObject newBullet = Instantiate(bulletPrefab, shootingPoint.transform.position, rot);
        if (newBullet.GetComponent<Animator>() != null) Debug.Log("Animator component found on bullet prefab.");
        newBullet.GetComponent<Animator>().SetBool("Hit", true);
        // newBullet.transform.LookAt(shootingPoint.transform.right * 30f);
        Rigidbody rb = newBullet.GetComponent<Rigidbody>();
        rb.velocity = shootingPoint.transform.forward * bulletSpeed;
        TriggerVibration();
        shootSound.PlaySound();
    }

    void TriggerVibration()
    {
        OVRInput.SetControllerVibration(virbrationStrength, virbrationStrength, OVRInput.Controller.RTouch);
        StartCoroutine(StopVibrationAfterDuration(virbrationDuration));
    }

    IEnumerator StopVibrationAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}
