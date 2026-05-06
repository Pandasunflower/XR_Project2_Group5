using UnityEngine;
using System.Collections.Generic;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;

public class GPUI_CrowdGenerator : MonoBehaviour
{
    [Header("GPUI 設定")]
    public GPUICrowdManager crowdManager; // 拖入 GPUICrowdManager

    [Header("觀眾 Prefab 清單 (請拖入 10 個已 Bake 的物件)")]
    // 這裡我們直接用 GameObject，讓你能在 Inspector 看到 10 個框框拖進去
    public List<GPUICrowdPrefab> audiencePrefabs; 

    [Header("散佈設定")]
    public MeshFilter targetMeshFilter; // 拖入目標 Mesh
    public int totalPopulation = 10000;

    // 用來儲存生成的實例，以便之後控制動畫
    private List<GPUICrowdPrefab> allInstances = new List<GPUICrowdPrefab>();

    void Start()
    {
        if (crowdManager == null || targetMeshFilter == null || audiencePrefabs.Count == 0)
        {
            Debug.LogError("Inspector 欄位有空缺！");
            return;
        }

        GenerateCrowdOnMesh();
    }

    void GenerateCrowdOnMesh()
    {
        Mesh mesh = targetMeshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < totalPopulation; i++)
        {
            // 1. 取得 Mesh 表面隨機位置
            Vector3 randomPoint = GetRandomPointOnMeshSurface(vertices, triangles);
            Vector3 worldPos = targetMeshFilter.transform.TransformPoint(randomPoint);
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360f), 0);

            // 2. 隨機選一個 Prefab (從你那 10 個裡面選)
            GPUICrowdPrefab selectedPrefab = audiencePrefabs[Random.Range(0, audiencePrefabs.Count)];

            // 3. 仿照昨天成功的邏輯：直接 Instantiate
            GPUICrowdPrefab go = Instantiate(selectedPrefab, worldPos, randomRot);
            allInstances.Add(go);
        }

        // 4. 統一交給 GPUI 接管 (這部分邏輯與你昨天的成功版一模一樣)
        List<GPUInstancerPrefab> baseInstances = allInstances.ConvertAll(x => (GPUInstancerPrefab)x);
        GPUInstancerAPI.RegisterPrefabInstanceList(crowdManager, baseInstances);
        
        // 5. 啟動 GPU 渲染
        GPUInstancerAPI.InitializeGPUInstancer(crowdManager);

        Debug.Log($"成功生成 {totalPopulation} 人，分佈於 {targetMeshFilter.name} 表面。");
    }

    // 三角形隨機取點 (讓分佈更均勻)
    Vector3 GetRandomPointOnMeshSurface(Vector3[] v, int[] t)
    {
        int triIndex = Random.Range(0, t.Length / 3) * 3;
        Vector3 a = v[t[triIndex]], b = v[t[triIndex + 1]], c = v[t[triIndex + 2]];
        float r1 = Random.value, r2 = Random.value;
        if (r1 + r2 > 1) { r1 = 1 - r1; r2 = 1 - r2; }
        return a + r1 * (b - a) + r2 * (c - a);
    }
}