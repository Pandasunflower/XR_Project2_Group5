using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUInstancer.CrowdAnimations;
using GPUInstancer;
using Unity.VisualScripting;

[System.Serializable]
public class GridZone
{
    public string zoneName = "格狀區域 A";
    [Tooltip("若有指定，center 會使用此物件的世界座標；留空則使用下方 center 欄位")]
    public Transform centerTransform;
    public Vector3 center = Vector3.zero;
    public int rowCount = 30;
    public int columnCount = 30;
    public float spaceMin = 1.5f;
    public float spaceMax = 2.0f;
}

[System.Serializable]
public class SpawnZone
{
    public string zoneName = "區域 A";
    public MeshFilter targetMeshFilter;
    public int population = 500;
    [Range(0f, 1f)] public float upwardThreshold = 0.5f;
    [Tooltip("此區域 NPC 面向的目標；留空則使用 Prefab 原始旋轉")]
    public Transform lookAtTarget;
}

public class AnimationManager : MonoBehaviour
{
    [Header("References")]
    public GPUICrowdManager gpuiCrowdManager;

    [Header("Spawn Mode")]
    [Tooltip("勾選啟用 Mesh Zone 隨機散佈")]
    public bool useMeshSpawn = false;
    [Tooltip("勾選啟用格狀（Center）生成")]
    public bool useGridSpawn = false;

    [Header("Mesh Spawn Settings")]
    public List<SpawnZone> zones = new List<SpawnZone>();

    [Header("Grid Spawn Settings")]
    public List<GridZone> gridZones = new List<GridZone>();

    [Header("Animation Settings")]
    public float normalSpeed = 1f;
    public float waveSpeed = 0.2f;
    public float waveMoveSpeed = 2f;
    public float waveFrequency = 0.8f;

    [SerializeField] private int globalIndex = 0;

    private List<GPUInstancerPrototype> _prototypeList;
    private List<GPUInstancerPrefab> _instanceList;

    private void Start()
    {
        if (gpuiCrowdManager == null)
            return;

        // Disabling the Crowd Manager here to change prototype settings.
        // Enabling it after this will make it re-initialize with the new settings for the prototypes
        gpuiCrowdManager.enabled = false;

        _instanceList = new List<GPUInstancerPrefab>();

        foreach (GPUICrowdPrototype prototype in gpuiCrowdManager.prototypeList)
        {
            prototype.animationData.useCrowdAnimator = true;
            prototype.enableRuntimeModifications = true;
            prototype.addRemoveInstancesAtRuntime = true;
            prototype.extraBufferSize = 10000;
        }

        if (useMeshSpawn)
            SpawnByMesh();

        if (useGridSpawn)
            SpawnByGrid();

        // Register the instantiated GOs to the Crowd Manager
        GPUInstancerAPI.RegisterPrefabInstanceList(gpuiCrowdManager, _instanceList);

        // Enabling the Crowd Manager back; this will re-initialize it with the new settings for the prototypes
        gpuiCrowdManager.enabled = true;

        _prototypeList = gpuiCrowdManager.prototypeList; // cache the prototype list on the Manager to access later
    }

    // ──────────────────────────────────────────────────────────
    //  Mesh-based spawning（同 GPUI_CrowdGenerator 的做法）
    // ──────────────────────────────────────────────────────────
    private void SpawnByMesh()
    {
        if (zones == null || zones.Count == 0)
        {
            Debug.LogWarning("[AnimationManager] useMeshSpawn=true 但 zones 列表是空的。");
            return;
        }

        int totalSpawned = 0;

        foreach (var zone in zones)
        {
            if (zone.targetMeshFilter == null || zone.targetMeshFilter.sharedMesh == null)
            {
                Debug.LogWarning($"[AnimationManager] Zone「{zone.zoneName}」沒有有效的 MeshFilter，跳過。");
                continue;
            }

            Mesh      mesh      = zone.targetMeshFilter.sharedMesh;
            Vector3[] vertices  = mesh.vertices;
            int[]     triangles = mesh.triangles;
            Vector3[] normals   = mesh.normals;

            int spawnedCount = 0;
            int attempts     = 0;
            int maxAttempts  = zone.population * 20;

            while (spawnedCount < zone.population && attempts < maxAttempts)
            {
                attempts++;

                // 隨機挑一個三角形
                int triIndex = Random.Range(0, triangles.Length / 3) * 3;
                int i1 = triangles[triIndex], i2 = triangles[triIndex + 1], i3 = triangles[triIndex + 2];

                // 平均法線轉世界空間
                Vector3 worldNormal = zone.targetMeshFilter.transform.TransformDirection(
                    (normals[i1] + normals[i2] + normals[i3]).normalized);

                // 過濾非水平面
                if (worldNormal.y <= zone.upwardThreshold)
                    continue;

                // 重心座標隨機點
                Vector3 a = vertices[i1], b = vertices[i2], c = vertices[i3];
                float r1 = Random.value, r2 = Random.value;
                if (r1 + r2 > 1f) { r1 = 1f - r1; r2 = 1f - r2; }
                Vector3 worldPos = zone.targetMeshFilter.transform.TransformPoint(a + r1 * (b - a) + r2 * (c - a));

                // 計算朝向
                Quaternion rotation;
                if (zone.lookAtTarget != null)
                {
                    Vector3 dir = zone.lookAtTarget.position - worldPos;
                    dir.y = 0f;
                    rotation = dir != Vector3.zero ? Quaternion.LookRotation(dir) : Quaternion.identity;
                }
                else
                {
                    rotation = ((GPUICrowdPrototype)gpuiCrowdManager.prototypeList[0]).prefabObject.transform.rotation;
                }

                // 隨機挑 prototype
                int protoIndex = Random.Range(0, gpuiCrowdManager.prototypeList.Count);
                GPUICrowdPrototype prototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[protoIndex];
                GameObject prefabObject = prototype.prefabObject;

                GameObject instanceGO = Instantiate(prefabObject, worldPos, rotation);
                instanceGO.GetComponent<PrefabIndex>().index = globalIndex++;
                _instanceList.Add(instanceGO.GetComponent<GPUICrowdPrefab>());
                spawnedCount++;
            }

            totalSpawned += spawnedCount;
            Debug.Log($"[AnimationManager] Zone「{zone.zoneName}」生成 {spawnedCount}/{zone.population} 個實體（嘗試 {attempts} 次）");
        }

        if (totalSpawned == 0)
            Debug.LogWarning("[AnimationManager] 所有 Zone 合計生成 0 個實體（法線門檻可能太高？）。");
        else
            Debug.Log($"[AnimationManager] Mesh 生成全部完成：共 {totalSpawned} 個實體，{zones.Count} 個 Zone。");
    }

    // ──────────────────────────────────────────────────────────
    //  原本的格狀生成
    // ──────────────────────────────────────────────────────────
    private void SpawnByGrid()
    {
        if (gridZones == null || gridZones.Count == 0)
        {
            Debug.LogWarning("[AnimationManager] useGridSpawn=true 但 gridZones 列表是空的。");
            return;
        }

        int totalSpawned = 0;

        foreach (var zone in gridZones)
        {
            Vector3 spawnCenter = zone.centerTransform != null ? zone.centerTransform.position : zone.center;
            float space = Random.Range(zone.spaceMin, zone.spaceMax);
            float width  = (zone.rowCount    - 1) * space;
            float height = (zone.columnCount - 1) * space;

            for (int r = 0; r < zone.rowCount; r++)
            {
                for (int c = 0; c < zone.columnCount; c++)
                {
                    space = Random.Range(zone.spaceMin, zone.spaceMax);

                    Vector3 pos = spawnCenter;
                    pos.x += r * space - width  * 0.5f;
                    pos.z += c * space - height * 0.5f;

                    int randomIndex = Random.Range(0, gpuiCrowdManager.prototypeList.Count);
                    GPUICrowdPrototype prototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[randomIndex];
                    GameObject prefabObject = prototype.prefabObject;

                    GameObject instanceGO = Instantiate(prefabObject, pos, prefabObject.transform.rotation);
                    instanceGO.GetComponent<PrefabIndex>().index = globalIndex++;
                    _instanceList.Add(instanceGO.GetComponent<GPUICrowdPrefab>());
                    totalSpawned++;
                }
            }

            Debug.Log($"[AnimationManager] GridZone「{zone.zoneName}」生成 {zone.rowCount * zone.columnCount} 個實體。");
        }

        Debug.Log($"[AnimationManager] 格狀生成全部完成：共 {totalSpawned} 個實體，{gridZones.Count} 個 Zone。");
    }

    private void OnDestroy()
    {
        if (_prototypeList == null)
            return;

        // We reset the protoypes back to using the Crowd Animator workflow, since changes would persist in the prototype data.
        foreach (GPUICrowdPrototype prototype in _prototypeList)
        {
            prototype.animationData.useCrowdAnimator = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) StartCoroutine(SetCrowdAnimators(0));
        else if (Input.GetKeyDown(KeyCode.Alpha2)) StartCoroutine(SetWaveCrowdAnimators(1));
        else if (Input.GetKeyDown(KeyCode.Alpha3)) StartCoroutine(SetCrowdAnimators(2));
        else if (Input.GetKeyDown(KeyCode.Alpha4)) StartCoroutine(SetCrowdAnimators(3));
    }

    public IEnumerator SetCrowdAnimators(int index)
    {
        if (gpuiCrowdManager != null)
        {
            while (!gpuiCrowdManager.isInitialized)
                yield return null;
            SetAnimations(index);
        }
    }

    public IEnumerator SetWaveCrowdAnimators(int index)
    {
        if (gpuiCrowdManager != null)
        {
            while (!gpuiCrowdManager.isInitialized)
                yield return null;
            SetWaveAnimations(index);
        }
    }

    public void SetAnimations(int index)
    {
        if (gpuiCrowdManager != null)
        {
            Dictionary<GPUInstancerPrototype, List<GPUInstancerPrefab>> registeredPrefabInstances = gpuiCrowdManager.GetRegisteredPrefabsRuntimeData();
            GPUIAnimationClipData clipData;
            float startTime;
            if (registeredPrefabInstances != null)
            {
                foreach (GPUICrowdPrototype crowdPrototype in registeredPrefabInstances.Keys)
                {
                    if (crowdPrototype.animationData != null && crowdPrototype.animationData.useCrowdAnimator)
                    {
                        foreach (GPUICrowdPrefab crowdInstance in registeredPrefabInstances[crowdPrototype])
                        {
                            clipData = crowdPrototype.animationData.clipDataList[index];
                            startTime = UnityEngine.Random.Range(0, clipData.length);

                            GPUICrowdAPI.StartAnimation(crowdInstance, clipData, startTime);
                            GPUICrowdAPI.SetAnimationSpeed(crowdInstance, normalSpeed);
                        }
                    }
                }
            }
        }
    }

    public void SetWaveAnimations(int index)
    {
        if (gpuiCrowdManager == null) return;

        var registered = gpuiCrowdManager.GetRegisteredPrefabsRuntimeData();

        if (registered == null) return;

        foreach (GPUICrowdPrototype prototype in registered.Keys)
        {
            if (prototype.animationData == null ||
                !prototype.animationData.useCrowdAnimator)
                continue;

            GPUIAnimationClipData clipData =
                prototype.animationData.clipDataList[index];

            List<GPUInstancerPrefab> instances = registered[prototype];

            for (int i = 0; i < instances.Count; i++)
            {
                GPUICrowdPrefab instance = (GPUICrowdPrefab)instances[i];

                int row = instance.gameObject.GetComponent<PrefabIndex>().index;
                float wave = Mathf.Sin(row * waveFrequency + Time.time * waveMoveSpeed);

                float startTime = Mathf.Lerp(0, clipData.length * 0.5f, (wave + 1f) * 0.5f);

                GPUICrowdAPI.StartAnimation(instance, clipData, startTime);
                GPUICrowdAPI.SetAnimationSpeed(instance, waveSpeed);
            }
        }
    }
}
