using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class TestSpawner100 : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] originalPrefabs;
    public int totalCount = 300;
    public Vector2 areaSize = new Vector2(30, 30);
    
    [Header("物理與位置")]
    public LayerMask surfaceLayer; 
    public float yOffset = 0f;

    [Header("純數學防重疊")]
    public float personalSpaceRadius = 1.0f; 
    public int maxRetriesPerSpawn = 20;

    [Header("朝向目標 (選填)")]
    public Transform lookTarget; 

    [Header("波浪舞設定 (Wave Effect)")]
    public bool enableWave = true; // 打勾開啟波浪效果
    public float waveLength = 20f; // 波浪的寬度，數字越大波浪越寬緩

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
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
                    GameObject go = Instantiate(originalPrefabs[Random.Range(0, originalPrefabs.Length)], finalPos, Quaternion.identity);
                    
                    if (lookTarget != null)
                    {
                        Vector3 targetPosition = new Vector3(lookTarget.position.x, go.transform.position.y, lookTarget.position.z);
                        go.transform.LookAt(targetPosition);
                    }
                    else
                    {
                        go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    }

                    Animator anim = go.GetComponentInChildren<Animator>();
                    if (anim != null)
                    {
                        anim.Update(0);
                        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

                        // --- 波浪舞核心邏輯 ---
                        float animationStartTime;
                        if (enableWave)
                        {
                            // 根據 X 座標計算時間差，產生規律波浪
                            animationStartTime = Mathf.Repeat(finalPos.x / waveLength, 1.0f);
                            anim.speed = 1.0f; // 為了保持波浪整齊，強制統一播放速度
                        }
                        else
                        {
                            // 原本的完全隨機
                            animationStartTime = Random.value;
                            anim.speed = Random.Range(0.8f, 1.2f);
                        }

                        anim.Play(state.fullPathHash, -1, animationStartTime);
                    }

                    spawnedPositions.Add(finalPos);
                    spawnedCount++;
                }
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
}