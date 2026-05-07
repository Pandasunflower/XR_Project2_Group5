using UnityEngine;
using System.Collections.Generic;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;

public class GPUICrowdController : MonoBehaviour
{
    [Header("GPUI 核心設定")]
    [Tooltip("場景上的 GPU Instancer Crowd Manager")]
    public GPUICrowdManager crowdManager; 
    
    // 💡 修正 1：完美對齊 API，使用 Crowd 專屬的 GPUICrowdPrefab 型別
    [Tooltip("你 Bake 好的觀眾 Prefab (可以放多種)")]
    public GPUICrowdPrefab[] audiencePrototypes; 
    
    [Header("生成設定")]
    public int totalCount = 10000; 
    public Vector2 areaSize = new Vector2(100, 100); 
    
    [Header("物理與位置")]
    public LayerMask surfaceLayer; 
    public float yOffset = 0f;

    [Header("防重疊設定")]
    public float personalSpaceRadius = 0.8f; 
    public int maxRetriesPerSpawn = 20;

    [Header("朝向目標 (選填)")]
    public Transform lookTarget; 

    [Header("手勢對應動畫 (放入 Clip)")]
    public AnimationClip gesture1Clip; 
    public AnimationClip gesture2Clip; 
    public AnimationClip gesture3Clip; 
    public AnimationClip gesture4Clip; 

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        // 💡 修正 2：字典的 Key 也同步改為 GPUICrowdPrefab
        Dictionary<GPUICrowdPrefab, List<Matrix4x4>> spawnData = new Dictionary<GPUICrowdPrefab, List<Matrix4x4>>();
        foreach (var prefab in audiencePrototypes)
        {
            spawnData[prefab] = new List<Matrix4x4>();
        }

        int spawnedCount = 0;
        int currentAttempt = 0;
        int totalAttemptsLimit = totalCount * maxRetriesPerSpawn;

        while (spawnedCount < totalCount && currentAttempt < totalAttemptsLimit)
        {
            currentAttempt++;

            float randomX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
            float randomZ = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
            Vector3 origin = transform.position + new Vector3(randomX, 50f, randomZ); 

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 100f, surfaceLayer))
            {
                Vector3 finalPos = hit.point + new Vector3(0, yOffset, 0);

                if (IsValidPosition(finalPos))
                {
                    Quaternion rotation = Quaternion.identity;
                    if (lookTarget != null)
                    {
                        Vector3 targetPos = new Vector3(lookTarget.position.x, finalPos.y, lookTarget.position.z);
                        Vector3 direction = (targetPos - finalPos).normalized;
                        if (direction != Vector3.zero) rotation = Quaternion.LookRotation(direction);
                    }
                    else
                    {
                        rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    }

                    // 💡 修正 3：抽籤取出的型別同步為 GPUICrowdPrefab
                    GPUICrowdPrefab selectedPrefab = audiencePrototypes[Random.Range(0, audiencePrototypes.Length)];
                    
                    spawnData[selectedPrefab].Add(Matrix4x4.TRS(finalPos, rotation, Vector3.one));

                    spawnedPositions.Add(finalPos);
                    spawnedCount++;
                }
            }
        }

        foreach (var kvp in spawnData)
        {
            if (kvp.Value.Count > 0)
            {
                // 💡 修正 4：補上 .prefabPrototype，給予 API 正確的資料層級
                GPUInstancerAPI.InitializeWithMatrix4x4Array(crowdManager, kvp.Key.prefabPrototype, kvp.Value.ToArray());
            }
        }
    }

    bool IsValidPosition(Vector3 pos)
    {
        foreach (Vector3 spawnedPos in spawnedPositions)
        {
            if (Vector3.Distance(pos, spawnedPos) < personalSpaceRadius) return false; 
        }
        return true; 
    }

    public void PlayGesture1() { PlayAnimationOnAll(gesture1Clip); }
    public void PlayGesture2() { PlayAnimationOnAll(gesture2Clip); }
    public void PlayGesture3() { PlayAnimationOnAll(gesture3Clip); }
    public void PlayGesture4() { PlayAnimationOnAll(gesture4Clip); }

    private void PlayAnimationOnAll(AnimationClip clip)
    {
        if (clip == null) return;
        foreach (var prefab in audiencePrototypes)
        {
            // 💡 這裡傳入的 prefab 現在已經是 GPUICrowdPrefab，完全符合 API 規定
            GPUICrowdAPI.StartAnimation(prefab, clip, 0.2f);
        }
    }

    // 鍵盤測試區 (按 Q, W, E, R)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) PlayGesture1();
        if (Input.GetKeyDown(KeyCode.W)) PlayGesture2();
        if (Input.GetKeyDown(KeyCode.E)) PlayGesture3();
        if (Input.GetKeyDown(KeyCode.R)) PlayGesture4();
    }
}