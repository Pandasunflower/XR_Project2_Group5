using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUInstancer.CrowdAnimations;
using GPUInstancer;
using Unity.VisualScripting;

[System.Serializable]
public class GridZone
{
    public bool enableSpawn = true;
    public string zoneName = "格狀區域 A";
    [Tooltip("拖入此物件，會使用它的世界座標 Bounds（Renderer 或 Collider）作為生成範圍的邊界")]
    public Transform centerTransform;
    public int rowCount = 30;
    public int columnCount = 30;
    [Range(0f, 1f)]
    [Tooltip("在每個格子內的隨機抖動比例，0 = 完全對齊格線，1 = 整格範圍內隨機")]
    public float jitter = 0.5f;
    [Tooltip("此區域 NPC 面向的目標；留空則使用 Prefab 原始旋轉")]
    public Transform lookAtTarget;
}

[System.Serializable]
public class SpawnZone
{
    public bool enableSpawn = true;
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
    [Tooltip("波浪隨時間移動的速度，越大波浪跑得越快")]
    public float waveMoveSpeed = 2f;
    [Tooltip("角度轉換成相位的縮放，越大代表繞圓心相鄰位置之間的時間差越大")]
    public float waveFrequency = 0.8f;
    [Range(0f, 1f)]
    [Tooltip("startTime 可以拉開的範圍上限（佔整個動畫長度的比例）。越大，相鄰位置之間的時間差會被拉得更開")]
    public float waveTimeSpread = 0.5f;
    [Tooltip("Wave 計算用的圓心；留空則使用 (0,0,0)。NPC 會依「繞此圓心的角度」決定波浪相位")]
    public Transform waveCenter;

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
            if (!zone.enableSpawn)
            {
                Debug.Log($"[AnimationManager] Zone「{zone.zoneName}」未啟用，跳過。");
                continue;
            }

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
            if (!zone.enableSpawn)
            {
                Debug.Log($"[AnimationManager] GridZone「{zone.zoneName}」未啟用，跳過。");
                continue;
            }

            if (zone.centerTransform == null)
            {
                Debug.LogWarning($"[AnimationManager] GridZone「{zone.zoneName}」沒有指定 centerTransform，跳過。");
                continue;
            }

            if (!TryGetWorldBounds(zone.centerTransform, out Bounds bounds))
            {
                Debug.LogWarning($"[AnimationManager] GridZone「{zone.zoneName}」的物件「{zone.centerTransform.name}」找不到 Renderer 或 Collider 來計算邊界，跳過。");
                continue;
            }

            int rowCount    = Mathf.Max(1, zone.rowCount);
            int columnCount = Mathf.Max(1, zone.columnCount);
            float cellWidth = bounds.size.x / rowCount;
            float cellDepth = bounds.size.z / columnCount;

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < columnCount; c++)
                {
                    float cellCenterX = bounds.min.x + cellWidth * (r + 0.5f);
                    float cellCenterZ = bounds.min.z + cellDepth * (c + 0.5f);

                    float jitterX = (Random.value - 0.5f) * cellWidth * zone.jitter;
                    float jitterZ = (Random.value - 0.5f) * cellDepth * zone.jitter;

                    Vector3 pos = new Vector3(
                        cellCenterX + jitterX,
                        bounds.center.y,
                        cellCenterZ + jitterZ);

                    int randomIndex = Random.Range(0, gpuiCrowdManager.prototypeList.Count);
                    GPUICrowdPrototype prototype = (GPUICrowdPrototype)gpuiCrowdManager.prototypeList[randomIndex];
                    GameObject prefabObject = prototype.prefabObject;

                    Quaternion rotation;
                    if (zone.lookAtTarget != null)
                    {
                        Vector3 dir = zone.lookAtTarget.position - pos;
                        dir.y = 0f;
                        rotation = dir != Vector3.zero ? Quaternion.LookRotation(dir) : Quaternion.identity;
                    }
                    else
                    {
                        rotation = prefabObject.transform.rotation;
                    }

                    GameObject instanceGO = Instantiate(prefabObject, pos, rotation);
                    instanceGO.GetComponent<PrefabIndex>().index = globalIndex++;
                    _instanceList.Add(instanceGO.GetComponent<GPUICrowdPrefab>());
                    totalSpawned++;
                }
            }

            Debug.Log($"[AnimationManager] GridZone「{zone.zoneName}」生成 {rowCount * columnCount} 個實體（邊界大小：{bounds.size}）。");
        }

        Debug.Log($"[AnimationManager] 格狀生成全部完成：共 {totalSpawned} 個實體，{gridZones.Count} 個 Zone。");
    }

    // 蒐集物件本身與子物件的 Renderer（沒有就用 Collider）合併出世界座標 Bounds
    private bool TryGetWorldBounds(Transform target, out Bounds bounds)
    {
        var renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }

        var colliders = target.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);
            return true;
        }

        bounds = default;
        return false;
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

                // ─── 舊版：用生成順序的 index 當「row」算波浪（多 zone / mesh 生成下已經不準確，先保留註解）───
                // int row = instance.gameObject.GetComponent<PrefabIndex>().index;
                // float wave = Mathf.Sin(row * waveFrequency + Time.time * waveMoveSpeed);

                // ─── 改版一：直接用世界座標 X 算波浪（先保留註解，改用圓心角度版）───
                // float worldX = instance.transform.position.x;
                // float wave = Mathf.Sin(worldX * waveFrequency + Time.time * waveMoveSpeed);

                // ─── 新版：以圓心為基準，用角度算波浪（繞圓心旋轉的波浪效果）───
                Vector3 centerPos = waveCenter != null ? waveCenter.position : Vector3.zero;
                Vector3 toInstance = instance.transform.position - centerPos;
                float angle = Mathf.Atan2(toInstance.z, toInstance.x); // -π ~ π
                float wave = Mathf.Sin(angle * waveFrequency + Time.time * waveMoveSpeed);

                float startTime = Mathf.Lerp(0, clipData.length * waveTimeSpread, (wave + 1f) * 0.5f);

                GPUICrowdAPI.StartAnimation(instance, clipData, startTime);
                GPUICrowdAPI.SetAnimationSpeed(instance, waveSpeed);
            }
        }
    }
}
