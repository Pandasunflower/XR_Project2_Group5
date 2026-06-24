using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerrisWheel : MonoBehaviour
{
    private Quaternion pivotOriginRot;
    public Transform pivot;
    public List<Transform> passengerCars;
    public float rotationSpeedNPC = 20f; 

    void Start()
    {
        StartCoroutine(NPCRideCoroutine());
    }
    IEnumerator NPCRideCoroutine()
    {
        pivotOriginRot = pivot.localRotation;
        float rotatedAngle = 0f; 
        while (true)
        {
            float deltaAngle = rotationSpeedNPC * Time.deltaTime;

            pivot.Rotate(Vector3.left, deltaAngle);
            rotatedAngle += deltaAngle;

            foreach (Transform car in passengerCars)
            {
                car.localRotation = Quaternion.Euler(rotatedAngle, 0f, 0f);
            }

            yield return null;
        }
    }
}
