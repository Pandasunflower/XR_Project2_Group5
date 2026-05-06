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

        [Header("散佈與朝向")]
        public MeshFilter targetMeshFilter;
        public int totalPopulation = 10000;
        
        [Range(0.1f, 1f)] 
        [Tooltip("過濾強度：0.5 代表面要朝上且傾斜度小於 45 度；0.9 代表只長在非常平坦的頂面。")]
        public float upwardThreshold = 0.8f; // 提高初始值以過濾底面
        
        public Transform lookAtTarget;

        [Header("動畫設定")]
        public AnimationClip bigWaveClip;       
        public AnimationClip ameiFanMoveAClip;  
        public AnimationClip keepJumpingClip;   
        public AnimationClip rightLeftDanceClip; 
        
        [Header("隨機偏移控制")]
        public bool useRandomOffsetForOthers = true; 
        [Range(0f, 1f)] public float offsetRatio = 0.5f; 

        private List<GPUICrowdPrefab> allInstances = new List<GPUICrowdPrefab>();

        void Start()
        {
            if (crowdManager == null || targetMeshFilter == null || audiencePrefabs.Count == 0) return;
            GenerateCrowdOnTopSurfaceOnly();
        }

        void GenerateCrowdOnTopSurfaceOnly()
        {
            Mesh mesh = targetMeshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;

            int spawnedCount = 0;
            int attempts = 0;
            int maxAttempts = totalPopulation * 20; // 增加嘗試次數以找尋符合的面

            while (spawnedCount < totalPopulation && attempts < maxAttempts)
            {
                attempts++;
                int triIndex = Random.Range(0, triangles.Length / 3) * 3;
                int i1 = triangles[triIndex], i2 = triangles[triIndex + 1], i3 = triangles[triIndex + 2];

                // 取得面法線並轉為世界坐標
                Vector3 avgNormal = (normals[i1] + normals[i2] + normals[i3]).normalized;
                Vector3 worldNormal = targetMeshFilter.transform.TransformDirection(avgNormal);

                // 核心判定：只有當世界法線的 Y 軸 > upwardThreshold 時才生成
                // 頂面 Y ~ 1, 底面 Y ~ -1。設定 > 0.5 即可完全排除底面。
                if (worldNormal.y > upwardThreshold)
                {
                    Vector3 a = vertices[i1], b = vertices[i2], c = vertices[i3];
                    float r1 = Random.value, r2 = Random.value;
                    if (r1 + r2 > 1) { r1 = 1 - r1; r2 = 1 - r2; }
                    Vector3 worldPos = targetMeshFilter.transform.TransformPoint(a + r1 * (b - a) + r2 * (c - a));

                    Quaternion finalRotation = Quaternion.identity;
                    if (lookAtTarget != null)
                    {
                        Vector3 direction = (lookAtTarget.position - worldPos);
                        direction.y = 0;
                        finalRotation = Quaternion.LookRotation(direction);
                    }

                    GPUICrowdPrefab selectedPrefab = audiencePrefabs[Random.Range(0, audiencePrefabs.Count)];
                    GPUICrowdPrefab go = Instantiate(selectedPrefab, worldPos, finalRotation);
                    allInstances.Add(go);
                    spawnedCount++;
                }
            }

            GPUInstancerAPI.RegisterPrefabInstanceList(crowdManager, allInstances.ConvertAll(x => (GPUInstancerPrefab)x));
            GPUInstancerAPI.InitializeGPUInstancer(crowdManager);
            
            Debug.Log($"生成完畢：在朝上面成功生成 {spawnedCount} 人，過濾嘗試了 {attempts} 次。");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G) && bigWaveClip != null) ChangeAnim(bigWaveClip, false);
            if (Input.GetKeyDown(KeyCode.H) && ameiFanMoveAClip != null) ChangeAnim(ameiFanMoveAClip, useRandomOffsetForOthers);
            if (Input.GetKeyDown(KeyCode.J) && keepJumpingClip != null) ChangeAnim(keepJumpingClip, useRandomOffsetForOthers);
            if (Input.GetKeyDown(KeyCode.L) && rightLeftDanceClip != null) ChangeAnim(rightLeftDanceClip, useRandomOffsetForOthers);
        }

        void ChangeAnim(AnimationClip targetClip, bool isRandom)
        {
            foreach (GPUICrowdPrefab instance in allInstances)
            {
                float startTime = isRandom ? Random.Range(0f, offsetRatio) : -1.0f;
                GPUICrowdAPI.StartAnimation(instance, targetClip, startTime, 1.0f, 0.2f);
            }
        }
    }
}