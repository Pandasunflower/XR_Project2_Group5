using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

public class Stage3CamVideoControl : MonoBehaviour
{
    public Renderer screenRenderer1;
    public VideoPlayer videoPlayer1;
    public Renderer screenRenderer2;
    public VideoPlayer videoPlayer2;
    public Renderer screenRendererMid;
    [Header("Materials")]
    public Material cameraMaterial;
    public Material signMaterial;
    public Material videoMaterial;

    public AK.Wwise.Event PlayEndEvent;
    public AK.Wwise.Event PlaySignEvent;

    public FirestoreTest firestoreTest;

    public void Start()
    {
        firestoreTest.SetGameState("l3_lobby");
    }

    void  Update()
    {
        // if (Input.GetKeyDown(KeyCode.K))
        // {
        //     Debug.Log("Switching to video view");
        //     SwitchToVideo();
        // }
        // else if (Input.GetKeyDown(KeyCode.L))
        // {
        //     Debug.Log("Switching to camera view");
        //     SwitchToCamera();
        // }
    }

    public void StartGame()
    {
        // SwitchToCamera();
        StartCoroutine(VideoCamChange());
    }

    public IEnumerator VideoCamChange(){
        firestoreTest.SetGameState("l3_voting");
        yield return new WaitForSeconds(121f);
        PlaySignEvent.Post(gameObject);
        AkUnitySoundEngine.StopAll();
        Debug.Log("121 sec");
        firestoreTest.SetGameState("l3_votingend");
        yield return new WaitForSeconds(5f);
        PlayEndEvent.Post(gameObject);
        Debug.Log("5 sec");
        firestoreTest.SetGameState("l3_sign");
        screenRendererMid.material = signMaterial;
        yield return new WaitForSeconds(30f);
        Debug.Log("30 sec");
        SwitchToVideo();
    }

    public void SwitchToVideo()
    {
        Debug.Log("Switching to video view");
        screenRenderer1.material = videoMaterial;
        screenRenderer2.material = videoMaterial;
        screenRendererMid.material = videoMaterial;

        // videoPlayer1.renderMode = VideoRenderMode.MaterialOverride;
        // videoPlayer1.targetMaterialRenderer = screenRenderer1;
        
        // videoPlayer2.renderMode = VideoRenderMode.MaterialOverride;
        // videoPlayer2.targetMaterialRenderer = screenRenderer2;

        videoPlayer1.Play();
        videoPlayer2.Play();
    }

    public void SwitchToCamera()
    {
        Debug.Log("Switching to camera view");
        videoPlayer1.Stop();
        videoPlayer2.Stop();

        // 直接換回 Camera 專用的材質
        screenRenderer1.material = cameraMaterial;
        screenRenderer2.material = cameraMaterial;
    }
}
