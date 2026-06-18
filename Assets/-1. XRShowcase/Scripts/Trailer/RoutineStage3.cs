using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoutineStage3 : MonoBehaviour
{
    [Header("References")]
    public GameObject MainCamera;
    public AnimationManager animationManager;
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    public Transform pos4;
    public Transform pos5;
    public Transform pos6;
    public Transform pos7;
    public Transform pos8;
    public Transform pos9;
    public GameObject singer1;
    public GameObject singer2;

    [Header("Settings")]
    public float moveTime = 4f;
    public float rotateTime = 2f;
    public float rotateTime2 = 3.5f;
    public float rotateTime3 = 4f;

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(PlaySequence());
        }
    }

    IEnumerator PlaySequence()
    {
        yield return animationManager.SetCrowdAnimators(0);
       
        yield return StartCoroutine(MoveRotate(MainCamera.transform, pos1, pos2, moveTime));
        yield return new WaitForSeconds(3f);

        MainCamera.transform.position = pos3.position;
        MainCamera.transform.rotation = pos3.rotation;
        yield return new WaitForSeconds(5f);


        yield return animationManager.SetWaveCrowdAnimators(1);
        MainCamera.transform.position = pos4.position;
        MainCamera.transform.rotation = pos4.rotation;
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, pos5.rotation, rotateTime));
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, pos4.rotation, rotateTime));
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, pos6.rotation, rotateTime));
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, pos4.rotation, rotateTime));
        yield return new WaitForSeconds(3f);

        yield return animationManager.SetCrowdAnimators(2);
        MainCamera.transform.position = pos7.position;
        MainCamera.transform.rotation = pos7.rotation;
        yield return StartCoroutine(Move(MainCamera.transform, MainCamera.transform.position, pos8.position, moveTime));
        yield return new WaitForSeconds(3f);

        singer1.SetActive(false);
        singer2.SetActive(true);
        yield return animationManager.SetCrowdAnimators(3);
        MainCamera.transform.position = pos9.position;
        MainCamera.transform.rotation = pos9.rotation;
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
}
