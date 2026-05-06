using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;

namespace MyCrowdSystem
{
    // 定義一個「區域」的資料結構，讓你在 Inspector 可以無限新增
    [System.Serializable]
    public class CrowdZone
    {
        public string zoneName = "看台區 A"; // 方便你在 Inspector 辨識
        public MeshFilter targetMeshFilter;
        public int population = 5000; // 這個區域的人數
        [Range(0.1f, 1f)] public float upwardThreshold = 0.8f;
    }

    public class GPUI_CrowdGenerator : MonoBehaviour
    {
        [Header("GPUI 設定")]
        public GPUICrowdManager crowdManager;
        public List<GPUICrowdPrefab> audiencePrefabs; 

        [Header("多區域散佈設定")]
        public List<CrowdZone> zones; // 替換掉原本單一的 MeshFilter
        public Transform lookAtTarget;

        [Header("動畫設定")]
        public AnimationClip bigWaveClip;       
        public AnimationClip ameiFanMoveAClip;  
        public AnimationClip keepJumpingClip;   
        public AnimationClip rightLeftDanceClip; 
        
        [Header("大波浪設定")]
        public float waveDelay = 3.0f; 

        [Header("隨機偏移控制")]
        public bool useRandomOffsetForOthers = true; 
        [Range(0f, 1f)] public float offsetRatio = 0.5f; 

        private List<GPUICrowdPrefab> allInstances = new List<GPUICrowdPrefab>();
        private float minX = float.MaxValue;
        private float maxX = float.MinValue;
        private Coroutine waveCoroutine;

        void Start()
        {
            if (crowdManager == null || zones.Count == 0 || audiencePrefabs.Count == 0) return;
            GenerateAllZones();
        }

        void GenerateAllZones()
        {
            // 迴圈跑遍每一個你設定的區域
            foreach (var zone in zones)
            {
                if (zone.targetMeshFilter == null) continue;

                Mesh mesh = zone.targetMeshFilter.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                Vector3[] normals = mesh.normals;

                int spawnedCount = 0;
                int attempts = 0;
                int maxAttempts = zone.population * 20;

                while (spawnedCount < zone.population && attempts < maxAttempts)
                {
                    attempts++;
                    int triIndex = Random.Range(0, triangles.Length / 3) * 3;
                    int i1 = triangles[triIndex], i2 = triangles[triIndex + 1], i3 = triangles[triIndex + 2];

                    Vector3 worldNormal = zone.targetMeshFilter.transform.TransformDirection((normals[i1] + normals[i2] + normals[i3]).normalized);

                    if (worldNormal.y > zone.upwardThreshold)
                    {
                        Vector3 a = vertices[i1], b = vertices[i2], c = vertices[i3];
                        float r1 = Random.value, r2 = Random.value;
                        if (r1 + r2 > 1) { r1 = 1 - r1; r2 = 1 - r2; }
                        Vector3 worldPos = zone.targetMeshFilter.transform.TransformPoint(a + r1 * (b - a) + r2 * (c - a));

                        // 統一計算所有區域的極左與極右，讓波浪能橫跨全場
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
            }

            allInstances.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            // 所有區域生成完畢後，統一只向 GPUI 註冊與初始化一次 (效能最高)
            GPUInstancerAPI.RegisterPrefabInstanceList(crowdManager, allInstances.ConvertAll(x => (GPUInstancerPrefab)x));
            GPUInstancerAPI.InitializeGPUInstancer(crowdManager);
        }

        // ---------- 以下動畫控制與波浪邏輯完全不變 ----------
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

            if (bigWaveClip != null && ameiFanMoveAClip != null)
            {
                yield return new WaitForSeconds(bigWaveClip.length - 0.2f);
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