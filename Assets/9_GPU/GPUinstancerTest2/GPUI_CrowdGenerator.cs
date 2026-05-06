using UnityEngine;
using System.Collections;
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
        [Range(0.1f, 1f)] public float upwardThreshold = 0.8f;
        public Transform lookAtTarget;

        [Header("動畫設定")]
        public AnimationClip bigWaveClip;       
        public AnimationClip ameiFanMoveAClip;  
        public AnimationClip keepJumpingClip;   
        public AnimationClip rightLeftDanceClip; 
        
        [Header("大波浪設定 (針對 G 鍵)")]
        public float waveDelay = 3.0f; 

        [Header("隨機偏移控制 (針對 H, J, L)")]
        public bool useRandomOffsetForOthers = true; 
        [Range(0f, 1f)] public float offsetRatio = 0.5f; 

        private List<GPUICrowdPrefab> allInstances = new List<GPUICrowdPrefab>();
        private float minX = float.MaxValue;
        private float maxX = float.MinValue;
        private Coroutine waveCoroutine;

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
            int maxAttempts = totalPopulation * 20;

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

                    if (worldPos.x < minX) minX = worldPos.x;
                    if (worldPos.x > maxX) maxX = worldPos.x;

                    Quaternion finalRotation = Quaternion.identity;
                    if (lookAtTarget != null)
                    {
                        Vector3 direction = (lookAtTarget.position - worldPos);
                        direction.y = 0;
                        finalRotation = Quaternion.LookRotation(direction);
                    }

                    GPUICrowdPrefab go = Instantiate(audiencePrefabs[Random.Range(0, audiencePrefabs.Count)], worldPos, finalRotation);
                    allInstances.Add(go);
                    spawnedCount++;
                }
            }

            allInstances.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            GPUInstancerAPI.RegisterPrefabInstanceList(crowdManager, allInstances.ConvertAll(x => (GPUInstancerPrefab)x));
            GPUInstancerAPI.InitializeGPUInstancer(crowdManager);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G) && bigWaveClip != null)
            {
                if (waveCoroutine != null) StopCoroutine(waveCoroutine);
                waveCoroutine = StartCoroutine(WaveSequence());
            }
            
            if (Input.GetKeyDown(KeyCode.H) && ameiFanMoveAClip != null) ChangeAnim(ameiFanMoveAClip, useRandomOffsetForOthers);
            if (Input.GetKeyDown(KeyCode.J) && keepJumpingClip != null) ChangeAnim(keepJumpingClip, useRandomOffsetForOthers);
            if (Input.GetKeyDown(KeyCode.L) && rightLeftDanceClip != null) ChangeAnim(rightLeftDanceClip, useRandomOffsetForOthers);
        }

        IEnumerator WaveSequence()
        {
            float timer = 0f;
            int currentIndex = 0;
            float rangeX = maxX - minX;
            if (rangeX <= 0) rangeX = 1f;

            while (currentIndex < allInstances.Count)
            {
                timer += Time.deltaTime;
                float currentWaveX = minX + (timer / waveDelay) * rangeX;

                while (currentIndex < allInstances.Count && allInstances[currentIndex].transform.position.x <= currentWaveX)
                {
                    GPUICrowdAPI.StartAnimation(allInstances[currentIndex], bigWaveClip, 0.0f, 1.0f, 0.1f);
                    currentIndex++;
                }
                yield return null;
            }

            // --- 新增：等待波浪到底，然後自動切換 ---
            if (bigWaveClip != null && ameiFanMoveAClip != null)
            {
                // 等待最後一個人的跳躍動畫快播完 (預留 0.2 秒做平滑過渡)
                yield return new WaitForSeconds(bigWaveClip.length - 0.2f);
                
                // 全場自動接續 AmeiFanMoveA，並套用隨機偏移讓動作自然
                ChangeAnim(ameiFanMoveAClip, useRandomOffsetForOthers);
            }
        }

        void ChangeAnim(AnimationClip targetClip, bool isRandom)
        {
            if (waveCoroutine != null) StopCoroutine(waveCoroutine); 

            foreach (GPUICrowdPrefab instance in allInstances)
            {
                float startTime = isRandom ? Random.Range(0f, offsetRatio) : 0.0f;
                GPUICrowdAPI.StartAnimation(instance, targetClip, startTime, 1.0f, 0.2f);
            }
        }
    }
}