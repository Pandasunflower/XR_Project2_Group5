using UnityEngine;
using System.Collections.Generic;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;

namespace MyCrowdSystem
{
    public class GPUI_CrowdGenerator : MonoBehaviour
    {
        [Header("GPUI 設定")]
        public GPUICrowdManager crowdManager;
        public List<GPUICrowdPrefab> audiencePrefabs; 

        [Header("散佈設定")]
        public MeshFilter targetMeshFilter;
        public int totalPopulation = 10000;
        [Range(0, 1)] public float upwardThreshold = 0.9f;

        [Header("朝向設定")]
        public Transform lookAtTarget; // 拖入舞台中心點
        public bool randomYOffset = true; // 是否允許每個人有稍微不同的左右偏移，看起來更自然

        private List<GPUICrowdPrefab> allInstances = new List<GPUICrowdPrefab>();

        void Start()
        {
            if (crowdManager == null || targetMeshFilter == null || audiencePrefabs.Count == 0) return;
            GenerateCrowdWithOrientation();
        }

        void GenerateCrowdWithOrientation()
        {
            Mesh mesh = targetMeshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            int spawnedCount = 0;
            int maxAttempts = totalPopulation * 15;
            int attempts = 0;

            while (spawnedCount < totalPopulation && attempts < maxAttempts)
            {
                attempts++;
                int triIndex = Random.Range(0, triangles.Length / 3) * 3;
                int i1 = triangles[triIndex], i2 = triangles[triIndex + 1], i3 = triangles[triIndex + 2];

                Vector3 worldNormal = targetMeshFilter.transform.TransformDirection((normals[i1] + normals[i2] + normals[i3]).normalized);

                if (worldNormal.y > upwardThreshold)
                {
                    Vector3 a = vertices[i1], b = vertices[i2], c = vertices[i3];
                    float r1 = Random.value, r2 = Random.value;
                    if (r1 + r2 > 1) { r1 = 1 - r1; r2 = 1 - r2; }
                    Vector3 worldPos = targetMeshFilter.transform.TransformPoint(a + r1 * (b - a) + r2 * (c - a));

                    // --- 新增：計算朝向邏輯 ---
                    Quaternion finalRotation;
                    if (lookAtTarget != null)
                    {
                        Vector3 direction = (lookAtTarget.position - worldPos);
                        direction.y = 0; // 鎖定 Y 軸，防止觀眾「仰頭」或「低頭」看舞台，保持站姿垂直
                        finalRotation = Quaternion.LookRotation(direction);
                        
                        if (randomYOffset)
                        {
                            // 稍微加上 -5 到 5 度的隨機偏移，讓人群不那麼死板
                            finalRotation *= Quaternion.Euler(0, Random.Range(-5f, 5f), 0);
                        }
                    }
                    else
                    {
                        finalRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    }

                    GPUICrowdPrefab selectedPrefab = audiencePrefabs[Random.Range(0, audiencePrefabs.Count)];
                    GPUICrowdPrefab go = Instantiate(selectedPrefab, worldPos, finalRotation);
                    allInstances.Add(go);
                    spawnedCount++;
                }
            }

            // 使用昨天的成功邏輯：Register 並 Initialize
            List<GPUInstancerPrefab> baseInstances = allInstances.ConvertAll(x => (GPUInstancerPrefab)x);
            GPUInstancerAPI.RegisterPrefabInstanceList(crowdManager, baseInstances);
            GPUInstancerAPI.InitializeGPUInstancer(crowdManager);
            
            Debug.Log($"萬人生成完畢！所有人已朝向：{(lookAtTarget != null ? lookAtTarget.name : "隨機方向")}");
        }
    }
}