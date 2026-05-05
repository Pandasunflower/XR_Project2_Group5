using UnityEngine;
using System.Collections.Generic;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;

public class GPUICrowdController : MonoBehaviour
{
    [Header("GPUI 核心設定")]
    public GPUICrowdManager crowdManager; 
    public GPUICrowdPrefab[] audiencePrototypes; 
    
    [Header("生成設定")]
    public int totalCount = 10000; // Matrix 模式開 1 萬人也沒問題
    public Vector2 areaSize = new Vector2(100, 100); 
    public LayerMask surfaceLayer; 
    public float yOffset = 0f;

    [Header("手勢動畫 (放入 Project 裡的原始 Clip)")]
    public AnimationClip gesture1Clip; 
    public AnimationClip gesture2Clip; 
    public AnimationClip gesture3Clip; 
    public AnimationClip gesture4Clip; 

    void Start()
    {
        if (crowdManager == null || audiencePrototypes == null) return;

        foreach (var prototype in audiencePrototypes)
        {
            if (prototype == null) continue;
            
            List<Matrix4x4> matrices = new List<Matrix4x4>();
            int countPerType = totalCount / audiencePrototypes.Length;

            for (int i = 0; i < countPerType; i++)
            {
                float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
                float z = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
                Vector3 origin = transform.position + new Vector3(x, 50f, z);

                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 100f, surfaceLayer))
                {
                    Vector3 finalPos = hit.point + new Vector3(0, yOffset, 0);
                    matrices.Add(Matrix4x4.TRS(finalPos, Quaternion.Euler(0, Random.Range(0, 360f), 0), Vector3.one));
                }
            }
            // 💡 關鍵：Matrix 註冊模式，效能最強
            GPUInstancerAPI.InitializeWithMatrix4x4Array(crowdManager, prototype.prefabPrototype, matrices.ToArray());
        }
    }

    private void PlayAnimationOnAll(AnimationClip clip)
    {
        if (clip == null || crowdManager == null) return;
        
        foreach (var prototype in audiencePrototypes)
        {
            if (prototype != null)
            {
                // 💡 修正 NullReferenceException 的寫法
                // 直接透過 API 告訴全場這一型號的觀眾要換動作
                GPUICrowdAPI.StartAnimation(prototype, clip, 0.0f); 
            }
        }
    }

    [ContextMenu("測試 1")] public void PlayGesture1() { PlayAnimationOnAll(gesture1Clip); }
    [ContextMenu("測試 2")] public void PlayGesture2() { PlayAnimationOnAll(gesture2Clip); }
    [ContextMenu("測試 3")] public void PlayGesture3() { PlayAnimationOnAll(gesture3Clip); }
    [ContextMenu("測試 4")] public void PlayGesture4() { PlayAnimationOnAll(gesture4Clip); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayGesture1();
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayGesture2();
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayGesture3();
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayGesture4();
    }
}