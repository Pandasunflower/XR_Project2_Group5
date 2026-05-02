using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;

public class CrowdSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] personPrefabs; // 改為陣列以支援多個 Prefab
    public int spawnCount = 50;
    public Vector3 spawnAreaSize = new Vector3(20, 0, 20); // 生成範圍
    public float personRadius = 0.5f; // 防碰撞檢測半徑
    public int maxSpawnAttempts = 30; // 每個物件尋找空位的最大嘗試次數

    [Header("朝向目標 (選填)")]
    public Transform lookTarget; // 新增朝向目標欄位

    [Header("地板設定")]
    public GameObject floor;
    public string groundLayerName = "Ground";

    // 儲存所有生成的 Animator 以便統一控制
    private List<Animator> crowdAnimators = new List<Animator>();

    void Start()
    {
        SetFloorLayer();
        SpawnCrowd();
    }

    // 1. 自動把地板設定成 Ground Layer
    private void SetFloorLayer()
    {
        if (floor != null)
        {
            int groundLayer = LayerMask.NameToLayer(groundLayerName);
            if (groundLayer == -1)
            {
                Debug.LogError($"請先在 Unity Editor 的右上角 Layers 中手動新增 '{groundLayerName}' 圖層！");
                return;
            }
            floor.layer = groundLayer;
        }
    }

    // 2. 生成人群並防重疊、處理朝向
    private void SpawnCrowd()
    {
        if (personPrefabs == null || personPrefabs.Length == 0)
        {
            Debug.LogError("請至少在 Person Prefabs 陣列中放入一個物件！");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = Vector3.zero;
            bool validPositionFound = false;

            // 嘗試尋找沒有碰撞的隨機位置
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
                float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
                spawnPos = transform.position + new Vector3(randomX, 0, randomZ);

                // 核心防重疊：檢查該點半徑內是否已有碰撞體
                if (!Physics.CheckSphere(spawnPos, personRadius))
                {
                    validPositionFound = true;
                    break;
                }
            }

            if (validPositionFound)
            {
                // 隨機挑選陣列中的一個 Prefab 來生成
                GameObject prefabToSpawn = personPrefabs[Random.Range(0, personPrefabs.Length)];
                GameObject newPerson = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                newPerson.transform.SetParent(this.transform);

                // 如果有設定朝向目標，則讓角色面向它 (鎖定 Y 軸避免傾斜)
                if (lookTarget != null)
                {
                    Vector3 targetPosition = new Vector3(lookTarget.position.x, newPerson.transform.position.y, lookTarget.position.z);
                    newPerson.transform.LookAt(targetPosition);
                }
                
                // 抓取 Animator 並存入 List (改用 GetComponentInChildren 避免 Prefab 結構問題)
                Animator anim = newPerson.GetComponentInChildren<Animator>();
                if (anim != null) crowdAnimators.Add(anim);
            }
            else
            {
                Debug.LogWarning("空間不足，無法生成更多角色。");
            }
        }
    }

    // 3. 統一執行動作
    public void PerformUniformAction(string triggerName)
    {
        foreach (Animator anim in crowdAnimators)
        {
            anim.SetTrigger(triggerName);
        }
    }

    // 在編輯器中畫出範圍
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}