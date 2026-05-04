using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class NPCSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] npcPrefabs;
    public GameObject[] goodnpcPrefabs;
    public GameObject[] speicialPrefabs;
    public GameObject MotorPrefabs;
    public Vector3 motorSpawnPosition = new Vector3(-0.043f, -0.049f, -26.12f);
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
    private NpcController motorNpc;
    private List<Vector3> _spawnedPositions = new List<Vector3>();

    public int score = 0;
    private object _scoreLock = new object();

    private int currentSpawned = 0;
    private int currentGoodSpawned = 0;
    
    public Stage2EventController eventController;

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
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartGame();
        }
    }

    void StartGame()
    {
        eventController.RegisterSpecialEvent(10f, () => {
            MotorTroll();
            Debug.Log("10 sec");
        });

        eventController.RegisterSpecialEvent(65f, () => {
            Debug.Log("65 sec");
        });

        eventController.StartGame();
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
                    controller.prefabIndex = i;
                    controller.isGoodNpc = true;
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

            int selectedIndex = Random.Range(0, npcPrefabs.Length);
            GameObject selectedPrefab = npcPrefabs[selectedIndex];

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
                    controller.prefabIndex = selectedIndex;
                    controller.isGoodNpc = false;
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

            int selectedIndex = Random.Range(0, goodnpcPrefabs.Length);
            GameObject selectedPrefab = goodnpcPrefabs[selectedIndex];

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
                    controller.prefabIndex = selectedIndex;
                    controller.isGoodNpc = true;
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

    public void RandomizeNPCAnimations()
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

    public void MotorTroll(){
        if (MotorPrefabs == null) return;
        
        Vector3 spawnPos = motorSpawnPosition;
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f);
        
        GameObject motor = Instantiate(MotorPrefabs, spawnPos, spawnRotation);
        
        motorNpc = motor.GetComponent<NpcController>();
        Animator anim = motor.GetComponent<Animator>();
        if (motorNpc != null)
        {
            motorNpc.isFacingSinger = false;
        }
        if (anim != null)
        {
            anim.SetBool("Motor", true);
            motorNpc.is_trolling = true;
        }
        
        StartCoroutine(MoveForward(motor));
    }
    
    private IEnumerator MoveForward(GameObject motor)
    {
        float moveSpeed = 2f; // 移動速度
        float moveDuration = 5f; // 移動持續時間
        float elapsed = 0f;
        
        while (elapsed < moveDuration && motor != null)
        {
            motor.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 停止動畫
        Animator anim = motor.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("Motor", false);
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
                    npc.Gotshot();
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

    public void RespawnNPC(NpcController npc)
    {
        if (npc == null) return;
        if (goodnpcPrefabs == null || goodnpcPrefabs.Length == 0)
        {
            Debug.LogWarning("無法重生：goodnpcPrefabs 尚未配置或為空。" );
            return;
        }

        int prefabIndex = npc.prefabIndex;
        if (prefabIndex < 0 || prefabIndex >= goodnpcPrefabs.Length)
        {
            Debug.LogWarning($"{npc.gameObject.name} 的 prefabIndex 無效，無法重生為對應 good NPC。" );
            return;
        }

        Vector3 oldPosition = npc.transform.position;

        _spawnedNpcs.Remove(npc);

        Destroy(npc.gameObject);

        GameObject newNpc = Instantiate(goodnpcPrefabs[prefabIndex], oldPosition, Quaternion.Euler(0f, 180f, 0f));
        NpcController controller = newNpc.GetComponent<NpcController>();
        if (controller != null)
        {
            controller.prefabIndex = prefabIndex;
            controller.isGoodNpc = true;
            controller.RandomizeAnimatorSpeed();
            _spawnedGoodNpcs.Add(controller);
        }

        _spawnedPositions.Add(oldPosition);
        Debug.Log($"已將 NPC 重生為 good NPC：{newNpc.name}，goodnpcPrefabs[{prefabIndex}]。" );
    }

    public IEnumerator SpinAndRespawnNPC(NpcController npc)
    {
        if (npc == null) yield break;

        npc.GoToSpin();

        yield return new WaitForSeconds(1.5f);

        RespawnNPC(npc);
    }
}