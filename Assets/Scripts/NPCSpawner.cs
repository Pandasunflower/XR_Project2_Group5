using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] npcPrefabs;
    public int spawnCount = 10;
    public float minDistance = 2f;
    public float fixedY = -1.544f;

    [Header("範圍參考物件")]
    public GameObject[] areaCubes;

    private List<NpcController> _spawnedNpcs = new List<NpcController>();
    private List<Vector3> _spawnedPositions = new List<Vector3>();

    public int score = 0;
    private object _scoreLock = new object();

    void Start()
    {
        ResetScore();
        if (areaCubes != null && areaCubes.Length > 0 && npcPrefabs != null && npcPrefabs.Length > 0)
        {
            SpawnNPCs();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeNPCAnimations("FanDance");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopAllNPCAnimation();
        }
    }

    void SpawnNPCs()
    {
        
        int attempts = 0;
        int currentSpawned = 0;

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
                    _spawnedNpcs.Add(controller);
                }
                _spawnedPositions.Add(spawnPos);
                currentSpawned++;
            }

            attempts++;
        }

        Debug.Log($"生成完畢！成功生成數量: {currentSpawned}");
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

    void RandomizeNPCAnimations(string animationName)
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
            selectedNpc.StartCoroutine(selectedNpc.PlayAnimation(animationName));
            Debug.Log($"隨機挑選了 {selectedNpc.gameObject.name} (索引: {randomIndex}) 開始跳舞！");
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
                    npc.ReturnToIdle();
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