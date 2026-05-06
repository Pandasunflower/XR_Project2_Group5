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
        public Transform lookAtTarget;
        public bool randomYOffset = true;

        [Header("動畫按鍵設定 (請拖入 AnimationClip)")]
        public AnimationClip bigWaveClip;       // 建議綁定：G
        public AnimationClip ameiFanMoveAClip;  // 建議綁定：H
        public AnimationClip keepJumpingClip;   // 建議綁定：J
        public AnimationClip rightLeftDanceClip; // 建議綁定：L

        private List<GPUICrowdPrefab> allInstances = new List<GPUICrowdPrefab>();

        void Start()
        {
            if (crowdManager == null || targetMeshFilter == null || audiencePrefabs.Count == 0) return;
            GenerateCrowdWithOrientation();
        }

        void Update()
        {
            // 監聽按鍵觸發動畫切換
            if (Input.GetKeyDown(KeyCode.G) && bigWaveClip != null) ChangeAnim(bigWaveClip);
            if (Input.GetKeyDown(KeyCode.H) && ameiFanMoveAClip != null) ChangeAnim(ameiFanMoveAClip);
            if (Input.GetKeyDown(KeyCode.J) && keepJumpingClip != null) ChangeAnim(keepJumpingClip);
            if (Input.GetKeyDown(KeyCode.L) && rightLeftDanceClip != null) ChangeAnim(rightLeftDanceClip);
        }

        void ChangeAnim(AnimationClip targetClip)
        {
            // 透過 GPUICrowdAPI 命令所有生成的實例切換動畫
            foreach (GPUICrowdPrefab instance in allInstances)
            {
                // 使用 0.2f 的過渡時間讓動作切換更平滑
                GPUICrowdAPI.StartAnimation(instance, targetClip, -1.0f, 1.0f, 0.2f);
            }
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

                    Quaternion finalRotation;
                    if (lookAtTarget != null)
                    {
                        Vector3 direction = (lookAtTarget.position - worldPos);
                        direction.y = 0;
                        finalRotation = Quaternion.LookRotation(direction);
                        if (randomYOffset) finalRotation *= Quaternion.Euler(0, Random.Range(-5f, 5f), 0);
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

            List<GPUInstancerPrefab> baseInstances = allInstances.ConvertAll(x => (GPUInstancerPrefab)x);
            GPUInstancerAPI.RegisterPrefabInstanceList(crowdManager, baseInstances); //
            GPUInstancerAPI.InitializeGPUInstancer(crowdManager); //
        }
    }
}