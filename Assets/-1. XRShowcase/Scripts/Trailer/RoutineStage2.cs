using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class RoutineStage2 : MonoBehaviour
{
    [Header("References")]
    public GameObject MainCamera;
    public GameObject gangster;
    public Animator ganster1;
    public Animator ganster2;
    public Animator ganster3;
    public GameObject phone;
    public Animator phoneAnimator;
    public GameObject motor;
    public GameObject motorNpc;
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    public Transform pos4;
    public Transform pos5;
    public Transform pos6;
    public Transform pos7;
    public Transform pos8;

    [Header("Settings")]
    public float moveTime = 4f;
    public float moveTime2 = 4f;
    public float moveTime3 = 4f;
    public float rotateTime = 2f;
    // Start is called before the first frame update
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(PlaySequence());
        }
    }

    IEnumerator PlaySequence()
    {
        gangster.SetActive(true);
        ganster1.SetBool("Stabbling(bat)", true);
        ganster2.SetBool("Yell_3", true);
        ganster3.SetBool("Yell_3", true);
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(MoveRotate(MainCamera.transform, pos1, pos2, moveTime));
        yield return new WaitForSeconds(6f);
        // gangster.SetActive(false);

        phone.SetActive(true);
        phoneAnimator.SetBool("PhoneCalling", true);
        yield return StartCoroutine(MoveRotate(MainCamera.transform, MainCamera.transform, pos3, moveTime2));
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, pos4.rotation, rotateTime));
        yield return StartCoroutine(MoveRotate(MainCamera.transform, MainCamera.transform, pos5, moveTime2));
        yield return new WaitForSeconds(6f);
        phone.SetActive(false);

        MotorTroll();
        MainCamera.transform.position = pos6.position;
        MainCamera.transform.rotation = pos6.rotation;
        yield return new WaitForSeconds(2.5f);

        MainCamera.transform.position = pos7.position;
        MainCamera.transform.rotation = pos7.rotation;
    }

    IEnumerator MoveRotate(Transform target,Transform posA, Transform posB, float duration)
    {
        float time = 0f;

        Vector3 startPos = posA.position;
        Vector3 endPos = posB.position;

        Quaternion startRot = posA.rotation;
        Quaternion endRot = posB.rotation;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;
            t = Mathf.SmoothStep(0, 1, t);

            target.position = Vector3.Lerp(startPos, endPos, t);
            target.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }
        target.position = endPos;
        target.rotation = endRot;
    }

    IEnumerator Move(Transform obj, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;

            obj.position = Vector3.Lerp(from, to, k);

            yield return null;
        }

        obj.position = to;
    }

    IEnumerator Rotate(Transform obj, Quaternion from, Quaternion to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;

            obj.rotation = Quaternion.Slerp(from, to, k);

            yield return null;
        }

        obj.rotation = to;
    }

    public void MotorTroll(){
        Animator anim = motorNpc.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("Motor", true);
        }
        
        StartCoroutine(MotorMoveForward(motor));
    }

    private IEnumerator MotorMoveForward(GameObject motor)
    {
        float moveDuration = 5.5f;

        Vector3 startPos = motor.transform.position;
        Vector3 endPos = startPos + motor.transform.forward * 19.6f; // 原本要走的距離

        float elapsed = 0f;

        while (elapsed < moveDuration && motor != null)
        {
            float t = elapsed / moveDuration;

            // 加速曲線
            t = t * t;

            motor.transform.position = Vector3.Lerp(
                startPos,
                endPos,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        motor.transform.position = endPos;

        yield return StartCoroutine(
            MoveRotate(MainCamera.transform, MainCamera.transform, pos8, moveTime3)
        );
    }
}
