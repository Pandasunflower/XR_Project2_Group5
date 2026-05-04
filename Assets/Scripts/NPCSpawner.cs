using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] npcPrefabs;
    public GameObject[] goodnpcPrefabs;
    public GameObject[] speicialPrefabs;
    public float specialScale = 1f;
    public int spawnCount = 10;
    public int spawnGoodCount = 10;
    public float minDistance = 2f;
    public float fixedY = -1.544f;
    public float firstTrollTime = 0f;
    public float trollInterval = 30f;

    [Header("範圍參考物件")]
    public GameObject[] areaCubes;

    private List<NpcController> _spawnedSpecialNpcs = new List<NpcController>();
    private List<NpcController> _spawnedNpcs = new List<NpcController>();
    private List<NpcController> _spawnedGoodNpcs = new List<NpcController>();
    private List<Vector3> _spawnedPositions = new List<Vector3>();

    public int score = 0;
    private object _scoreLock = new object();

    private int currentSpawned = 0;
    private int currentGoodSpawned = 0;

    void Start()
    {
        ResetScore();
        if (areaCubes != null && areaCubes.Length > 0 && npcPrefabs != null && npcPrefabs.Length > 0)
        {
            SpawnSpecialNPCs();
            SpawnNPCs();
            SpawnGoodNPCs();
            // InvokeRepeating("RandomizeNPCAnimations", firstTrollTime, trollInterval);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeNPCAnimations();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopAllNPCAnimation();
        }
    }

    void SpawnSpecialNPCs()
    {
        
        int attempts = 0;

        for (int i = 0; i < speicialPrefabs.Length;)
        {
            GameObject selectedCube = areaCubes[Random.Range(0, areaCubes.Length)];
            Bounds bounds = selectedCube.GetComponent<Renderer>().bounds;

            GameObject selectedPrefab = speicialPrefabs[i];

            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 spawnPos = new Vector3(randomX, fixedY, randomZ);
            Quaternion spawnRotation = Quaternion.Euler(0f, 180f, 0f);

            if (IsPositionValid(spawnPos))
            {
                GameObject newNpc = Instantiate(selectedPrefab, spawnPos, spawnRotation);

                NpcController controller = newNpc.GetComponent<NpcController>();
                newNpc.transform.localScale = new Vector3(specialScale, specialScale, specialScale);
                if (controller != null)
                {
                    controller.RandomizeAnimatorSpeed();
                    _spawnedSpecialNpcs.Add(controller);
                }
                _spawnedPositions.Add(spawnPos);
                i++;
            }
            else
            {
                Debug.Log($"嘗試生成特殊 NPC 失敗，位置不合法。嘗試次數: {attempts}");
            }
        }
    }

    void SpawnNPCs()
    {
        
        int attempts = 0;

        while (currentSpawned < spawnCount && attempts < spawnCount * 10)
        {
            GameObject selectedCube = areaCubes[Random.Range(0, areaCubes.Length)];
            Bounds bounds = selectedCube.GetComponent<Renderer>().bounds;

            GameObject selectedPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];

            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 spawnPos = new Vector3(randomX, fixedY, randomZ);
            Quaternion spawnRotation = Quaternion.Euler(0f, 180f, 0f);

            if (IsPositionValid(spawnPos))
            {
                GameObject newNpc = Instantiate(selectedPrefab, spawnPos, spawnRotation);
                NpcController controller = newNpc.GetComponent<NpcController>();
                if (controller != null)
                {
                    controller.RandomizeAnimatorSpeed();
                    _spawnedNpcs.Add(controller);
                }
                _spawnedPositions.Add(spawnPos);
                currentSpawned++;
            }

            attempts++;
        }

        Debug.Log($"生成完畢！成功生成數量: {currentSpawned}");
    }

    void SpawnGoodNPCs()
    {
        if (goodnpcPrefabs == null || goodnpcPrefabs.Length == 0) return;

        int attempts = 0;

        while (currentGoodSpawned < spawnGoodCount && attempts < spawnGoodCount * 10)
        {
            GameObject selectedCube = areaCubes[Random.Range(0, areaCubes.Length)];
            Bounds bounds = selectedCube.GetComponent<Renderer>().bounds;

            GameObject selectedPrefab = goodnpcPrefabs[Random.Range(0, goodnpcPrefabs.Length)];

            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 spawnPos = new Vector3(randomX, fixedY, randomZ);
            Quaternion spawnRotation = Quaternion.Euler(0f, 180f, 0f);

            if (IsPositionValid(spawnPos))
            {
                GameObject newNpc = Instantiate(selectedPrefab, spawnPos, spawnRotation);
                NpcController controller = newNpc.GetComponent<NpcController>();
                if (controller != null)
                {
                    controller.RandomizeAnimatorSpeed();
                    _spawnedGoodNpcs.Add(controller);
                }
                _spawnedPositions.Add(spawnPos);
                currentGoodSpawned++;
            }

            attempts++;
        }

        Debug.Log($"生成完畢！成功生成數量: {currentGoodSpawned}");
    }

    bool IsPositionValid(Vector3 pos)
    {
        foreach (Vector3 otherPos in _spawnedPositions)
        {
            if (Vector3.Distance(pos, otherPos) < minDistance)
            {
                return false;
            }
        }

        return true;
    }

    void RandomizeNPCAnimations()
    {
        if (_spawnedNpcs == null || _spawnedNpcs.Count == 0)
        {
            Debug.LogWarning("目前沒有生成的 NPC 可以控制！");
            return;
        }

        int randomIndex = Random.Range(0, _spawnedNpcs.Count);
        NpcController selectedNpc = _spawnedNpcs[randomIndex];

        if (selectedNpc != null)
        {
            selectedNpc.StopAllCoroutines(); 
            selectedNpc.StartCoroutine(selectedNpc.PlayRandomAnimation());
            Debug.Log($"隨機挑選了 {selectedNpc.gameObject.name} (索引: {randomIndex}) 開始TROLL！");
        }
    }

    void StopAllNPCAnimation()
    {
        foreach (NpcController npc in _spawnedNpcs)
        {
            if (npc != null)
            {
                if (npc.is_trolling)
                {
                    // npc.StartCoroutine(npc.GoToTurn());
                    npc.GoToSpin();
                }
            }
        }
    }

    public void ResetScore()
    {
        lock (_scoreLock)
        {
            score = 0;
            Debug.Log("分數已重置！");
        }
    }

    public IEnumerator AddScore()
    {
        lock (_scoreLock)
        {
            score++;
            Debug.Log($"得分增加！當前分數: {score}");
        }
        yield return null;
    }
}