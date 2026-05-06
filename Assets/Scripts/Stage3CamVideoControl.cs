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
    [Header("Materials")]
    public Material cameraMaterial;
    public Material videoMaterial;

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

    void Start()
    {
        // SwitchToCamera();
        StartCoroutine(VideoCamChange());
    }

    public IEnumerator VideoCamChange(){
        yield return new WaitForSeconds(20f);
        SwitchToVideo();
        // yield return new WaitForSeconds(10f);
        // SwitchToCamera();
        // yield return new WaitForSeconds(10f);
        // SwitchToVideo();
        // yield return new WaitForSeconds(10f);
        // SwitchToCamera();
        // yield return new WaitForSeconds(10f);
        // SwitchToVideo();
        // yield return new WaitForSeconds(10f);
        // SwitchToCamera();
        // yield return new WaitForSeconds(10f);
        // SwitchToVideo();
        // yield return new WaitForSeconds(10f);
        // SwitchToCamera();
    }

    public void SwitchToVideo()
    {
        screenRenderer1.material = videoMaterial;
        screenRenderer2.material = videoMaterial;

        // videoPlayer1.renderMode = VideoRenderMode.MaterialOverride;
        // videoPlayer1.targetMaterialRenderer = screenRenderer1;
        
        // videoPlayer2.renderMode = VideoRenderMode.MaterialOverride;
        // videoPlayer2.targetMaterialRenderer = screenRenderer2;

        videoPlayer1.Play();
        videoPlayer2.Play();
    }

    public void SwitchToCamera()
    {
        videoPlayer1.Stop();
        videoPlayer2.Stop();

        // 直接換回 Camera 專用的材質
        screenRenderer1.material = cameraMaterial;
        screenRenderer2.material = cameraMaterial;
    }
}
