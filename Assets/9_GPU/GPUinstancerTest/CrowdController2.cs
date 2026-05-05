using UnityEngine;
using System.Collections.Generic;
using GPUInstancer;
using GPUInstancer.CrowdAnimations; 

public class CrowdController2 : MonoBehaviour
{
    [Header("GPUI 設定")]
    public GPUICrowdManager crowdManager;
    public GPUInstancerPrefab audiencePrefab; 
    public int crowdCount = 10000;
    
    [Header("請在此拖入你的四個動畫檔案 (AnimationClip)")]
    public AnimationClip bigWaveClip;
    public AnimationClip ameiFanMoveAClip;
    public AnimationClip keepJumpingClip;
    public AnimationClip rightLeftDanceClip;

    private List<GPUICrowdPrefab> crowdInstances = new List<GPUICrowdPrefab>();

    void Start()
    {
        // 1. 生成萬人觀眾
        for (int i = 0; i < crowdCount; i++)
        {
            Vector3 pos = new Vector3((i % 100) * 1.5f, 0, (i / 100) * 1.5f) + transform.position;
            GameObject go = Instantiate(audiencePrefab.gameObject, pos, Quaternion.identity);
            crowdInstances.Add(go.GetComponent<GPUICrowdPrefab>());
        }

        // 2. 交給 GPUI 接管並轉換為 GPU 渲染
        List<GPUInstancerPrefab> baseInstances = crowdInstances.ConvertAll(x => (GPUInstancerPrefab)x);
        GPUInstancerAPI.RegisterPrefabInstanceList(crowdManager, baseInstances);
        GPUInstancerAPI.InitializeGPUInstancer(crowdManager);
    }

    void Update()
    {
        // 檢查按鍵，並確保你有在 Inspector 拖入動畫檔案
        if (Input.GetKeyDown(KeyCode.G) && bigWaveClip != null) ChangeAnim(bigWaveClip);
        if (Input.GetKeyDown(KeyCode.H) && ameiFanMoveAClip != null) ChangeAnim(ameiFanMoveAClip);
        if (Input.GetKeyDown(KeyCode.J) && keepJumpingClip != null) ChangeAnim(keepJumpingClip);
        if (Input.GetKeyDown(KeyCode.L) && rightLeftDanceClip != null) ChangeAnim(rightLeftDanceClip);
    }

    void ChangeAnim(AnimationClip targetClip)
    {
        // 3. 透過 GPUICrowdAPI 直接命令 GPU 切換成目標動畫檔案
        foreach (GPUICrowdPrefab instance in crowdInstances)
        {
            GPUICrowdAPI.StartAnimation(instance, targetClip, 0.2f);
        }
    }
}