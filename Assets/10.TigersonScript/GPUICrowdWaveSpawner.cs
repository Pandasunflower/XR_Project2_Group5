using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUInstancer; 

public class GPUICrowdWaveSpawner : MonoBehaviour
{
    [Header("GPU Instancer 設定")]
    public GPUInstancerPrefabManager prefabManager; 
    public GPUInstancerPrefab gpulPrefab; 

    [Header("生成數量與範圍")]
    public int totalCount = 300;
    public Vector2 areaSize = new Vector2(30, 30);
    public LayerMask surfaceLayer; 
    public float yOffset = 0f;

    [Header("空間演算法")]
    public float personalSpaceRadius = 1.0f; 
    public int maxRetriesPerSpawn = 20;

    [Header("朝向與波浪設定")]
    public Transform lookTarget; 
    public string jumpAnimationName = "Jump";
    public float jumpHeight = 1.5f;
    public float jumpDuration = 1.0f;
    public float waveDelayMultiplier = 0.05f;

    private List<GPUInstancerPrefab> instances = new List<GPUInstancerPrefab>();
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private bool isWaving = false;

    void Start()
    {
        if (prefabManager == null || gpulPrefab == null) return;
        SpawnCrowd();
    }

    void SpawnCrowd()
    {
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
                    Quaternion rotation = (lookTarget != null) 
                        ? Quaternion.LookRotation(new Vector3(lookTarget.position.x, finalPos.y, lookTarget.position.z) - finalPos)
                        : Quaternion.Euler(0, Random.Range(0, 360f), 0);

                    // 直接生成，並預設關閉 Animator 節省 CPU
                    GPUInstancerPrefab instance = Instantiate(gpulPrefab, finalPos, rotation);
                    Animator anim = instance.GetComponent<Animator>();
                    if (anim != null) anim.enabled = false; 

                    instances.Add(instance);
                    spawnedPositions.Add(finalPos);
                    spawnedCount++;
                }
            }
        }

        // 使用最通用的 API 初始化
        GPUInstancerAPI.RegisterPrefabInstanceList(prefabManager, instances);
        GPUInstancerAPI.InitializeGPUInstancer(prefabManager);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isWaving)
        {
            StartCoroutine(TriggerWave());
        }
    }

    IEnumerator TriggerWave()
    {
        isWaving = true;
        Vector3 waveOrigin = lookTarget != null ? lookTarget.position : transform.position;

        for (int i = 0; i < instances.Count; i++)
        {
            if (instances[i] != null)
            {
                float dist = Vector3.Distance(instances[i].transform.position, waveOrigin);
                StartCoroutine(IndividualJump(instances[i], dist * waveDelayMultiplier));
            }
        }
        yield return new WaitForSeconds(jumpDuration + 2f);
        isWaving = false;
    }

    IEnumerator IndividualJump(GPUInstancerPrefab gpuiObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Animator anim = gpuiObj.GetComponent<Animator>();
        Transform t = gpuiObj.transform;

        if (anim != null && t != null)
        {
            anim.enabled = true;
            anim.Play(jumpAnimationName);

            Vector3 startPos = t.position;
            Vector3 peakPos = startPos + Vector3.up * jumpHeight;
            float halfTime = jumpDuration / 2f;

            float elapsed = 0;
            while (elapsed < halfTime) {
                if(t != null) t.position = Vector3.Lerp(startPos, peakPos, elapsed / halfTime);
                elapsed += Time.deltaTime; yield return null;
            }
            elapsed = 0;
            while (elapsed < halfTime) {
                if(t != null) t.position = Vector3.Lerp(peakPos, startPos, elapsed / halfTime);
                elapsed += Time.deltaTime; yield return null;
            }
            if(t != null) t.position = startPos;
            if(anim != null) anim.enabled = false; 
        }
    }

    bool IsValidPosition(Vector3 pos)
    {
        foreach (Vector3 spawnedPos in spawnedPositions)
            if (Vector3.Distance(pos, spawnedPos) < personalSpaceRadius) return false; 
        return true; 
    }
}