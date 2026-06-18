using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RoutineStage1 : MonoBehaviour
{
    [Header("References")]
    public GameObject MainCamera;
    public Light spotLight;
    public Light directionalLight;
    public LyricSystem lyricSystem;
    public TrailerAudience trailerAudience;

    public MeshRenderer lyrics;
    public GameObject album;
    public GameObject singer;

    public Transform pos1;
    public Transform pos2;
    public Transform angle1;
    public Transform angle2;
    public Transform angle3;
    public Transform angle4;
    public Transform pos3;
    public Transform pos4;
    public Transform angle5;
    public Transform angle6;

    [Header("Settings")]
    public float lightTime = 1f;
    public float moveTime = 3f;
    public float rotateTime = 2f;
    public float rotateTime2 = 3.5f;
    public float rotateTime3 = 4f;
    public float lightTime2 = 3f;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(PlaySequence());
        }
    }

    IEnumerator PlaySequence()
    {
        // 1. 開燈
        yield return StartCoroutine(LightOn(spotLight, lightTime));

        // 2. 移動 camera
        yield return StartCoroutine(Move(MainCamera.transform, pos1.position, pos2.position, moveTime));

        // 3. 旋轉 camera
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, angle1.rotation, rotateTime));

        yield return StartCoroutine(ChangeLightIntensity(directionalLight, 0.01f, 0.1f, lightTime2));

        lyricSystem.Play();

        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, angle3.rotation, rotateTime));


        yield return trailerAudience.ShowAudienceCoroutine();

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, angle2.rotation, rotateTime2));
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, angle4.rotation, rotateTime2));
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, angle3.rotation, rotateTime2));

        trailerAudience.GenerateCrowd();

        yield return new WaitForSeconds(4f);
        lyrics.enabled = false;
        album.SetActive(false);
        singer.SetActive(true);
        MainCamera.transform.position = pos3.position;
        MainCamera.transform.rotation = pos3.rotation;
        yield return new WaitForSeconds(10f);
        
        lyrics.enabled = true;
        MainCamera.transform.position = pos4.position;
        MainCamera.transform.rotation = pos4.rotation;
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, angle5.rotation, rotateTime3));
        yield return StartCoroutine(Rotate(MainCamera.transform, MainCamera.transform.rotation, angle6.rotation, rotateTime3 * 2));
    }

    IEnumerator LightOn(Light light, float duration)
    {
        float t = 0f;
        float start = 0f;
        float end = 35f;

        light.intensity = 0;
        light.enabled = true;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            light.intensity = Mathf.Lerp(start, end, k);
            yield return null;
        }

        light.intensity = end;
    }

    IEnumerator ChangeLightIntensity(Light light, float from, float to, float duration)
    {
        float t = 0f;

        light.intensity = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;

            light.intensity = Mathf.Lerp(from, to, k);

            yield return null;
        }

        light.intensity = to;
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
