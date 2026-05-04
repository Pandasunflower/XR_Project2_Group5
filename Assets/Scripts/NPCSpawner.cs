using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

public class NPCSpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject[] npcPrefabs;
    public GameObject[] NonGangNpcPrefabs;
    public GameObject[] goodNpcPrefabs;
    public GameObject[] NonGangGoodNpcPrefabs;
    public GameObject[] speicialPrefabs;
    public GameObject MotorPrefabs;
    public GameObject motorSpawnPosition;
    public GameObject AdamPrefabs;
    public GameObject AdamSpawnPosition;
    public float specialScale = 1f;
    public int spawnCount = 10;
    public int spawnGangCount = 10;
    public int spawnGoodCount = 10;
    public float minDistance = 2f;
    public float fixedY = -1.544f;
    public float firstTrollTime = 0f;
    public float trollInterval = 30f;

    [Header("範圍參考物件")]
    public GameObject[] areaCubes;
    public GameObject[] GangAreaCubes;

    private List<NpcController> _spawnedSpecialNpcs = new List<NpcController>();
    private List<NpcController> _spawnedGangNpcs = new List<NpcController>();
    private List<NpcController> _spawnedNpcs = new List<NpcController>();
    private List<NpcController> _spawnedGoodNpcs = new List<NpcController>();
    private NpcController motorNpc;
    private NpcController adamNpc;
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
            SpawnAdam();
            SpawnMotor();
            SpawnSpecialNPCs();
            SpawnGangNPCs();
            SpawnNonGangNPCs();
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
        eventController.RegisterSpecialEvent(5f, () => {
            GangNPCAnimations();
            Debug.Log("5 sec");
        });

        eventController.RegisterSpecialEvent(10f, () => {
            MotorTroll();
            Debug.Log("10 sec");
        });

        eventController.RegisterSpecialEvent(15f, () => {
            RandomizeNPCAnimations();
            Debug.Log("15 sec");
        });

        eventController.RegisterSpecialEvent(20f, () => {
            AdamWalk();
            Debug.Log("20 sec");
        });

        eventController.StartGame();
    }

    void SpawnNonGangNPCs()
    {
        
        int attempts = 0;

        while (currentSpawned < spawnCount && attempts < spawnCount * 10)
        {
            GameObject selectedCube = areaCubes[Random.Range(0, areaCubes.Length)];
            Bounds bounds = selectedCube.GetComponent<Renderer>().bounds;

            int selectedIndex = Random.Range(0, NonGangNpcPrefabs.Length);
            GameObject selectedPrefab = NonGangNpcPrefabs[selectedIndex];

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
                    controller.isGangNpc = false;
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

    void SpawnAdam()
    {
        if (AdamPrefabs == null || AdamSpawnPosition == null) return;
        
        Vector3 spawnPos = AdamSpawnPosition.transform.position;
        Quaternion spawnRotation = Quaternion.Euler(0f, 180f, 0f);
        spawnPos = new Vector3(spawnPos.x, fixedY, spawnPos.z);
        
        GameObject adam = Instantiate(AdamPrefabs, spawnPos, spawnRotation);
        
        adamNpc = adam.GetComponent<NpcController>();
        if (adamNpc != null)
        {
            adamNpc.isFacingSinger = false;
        }
        
        _spawnedPositions.Add(spawnPos);
    }

    void SpawnMotor()
    {
        if (MotorPrefabs == null || motorSpawnPosition == null) return;
        
        Vector3 spawnPos = motorSpawnPosition.transform.position;
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f);
        
        GameObject motor = Instantiate(MotorPrefabs, spawnPos, spawnRotation);
        
        motorNpc = motor.GetComponent<NpcController>();
        if (motorNpc != null)
        {
            motorNpc.isFacingSinger = false;
        }
        
        _spawnedPositions.Add(spawnPos);
    }

    void SpawnGangNPCs()
    {
        
        int attempts = 0;

        while (currentSpawned < spawnGangCount && attempts < spawnGangCount * 10)
        {
            GameObject selectedCube = GangAreaCubes[Random.Range(0, GangAreaCubes.Length)];
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
                    controller.isGangNpc = true;
                    controller.RandomizeAnimatorSpeed();
                    _spawnedGangNpcs.Add(controller);
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
        if (goodNpcPrefabs == null || goodNpcPrefabs.Length == 0) return;

        int attempts = 0;

        while (currentGoodSpawned < spawnGoodCount && attempts < spawnGoodCount * 10)
        {
            GameObject selectedCube = areaCubes[Random.Range(0, areaCubes.Length)];
            Bounds bounds = selectedCube.GetComponent<Renderer>().bounds;

            int selectedIndex = Random.Range(0, goodNpcPrefabs.Length);
            GameObject selectedPrefab = goodNpcPrefabs[selectedIndex];

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

    public void GangNPCAnimations()
    {
        if (_spawnedNpcs == null || _spawnedNpcs.Count == 0)
        {
            Debug.LogWarning("目前沒有生成的 NPC 可以控制！");
            return;
        }
        // 所有gang一起同時執行動畫
        foreach (NpcController npc in _spawnedGangNpcs)
        {
            if (npc != null)
            {
                npc.StopAllCoroutines(); 
                npc.StartCoroutine(npc.PlayRandomAnimation());
            }
        }
    }

    public void MotorTroll(){
        if (motorNpc == null) return;
        
        Animator anim = motorNpc.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("Motor", true);
            motorNpc.is_trolling = true;
        }
        
        StartCoroutine(MotorMoveForward(motorNpc.gameObject));
    }

    public void AdamWalk()
    {
        if (adamNpc == null) return;
        
        Animator anim = adamNpc.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("FanDance", true);
            adamNpc.is_trolling = true;
        }

        StartCoroutine(AdamMoveForward(adamNpc.gameObject));
    }
    
    private IEnumerator MotorMoveForward(GameObject motor, string animBoolName = "Motor")
    {
        float moveSpeed = 2f;
        float moveDuration = 10f;
        float elapsed = 0f;
        
        while (elapsed < moveDuration && motor != null)
        {
            motor.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(30f - moveDuration);

        Animator anim = motor.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Idle");
            motorNpc.is_trolling = false;
        }
    }

    private IEnumerator AdamMoveForward(GameObject motor, string animBoolName = "Motor")
    {
        // float moveSpeed = 2f;
        // float moveDuration = 10f;
        // float elapsed = 0f;
        
        // while (elapsed < moveDuration && motor != null)
        // {
        //     motor.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        //     elapsed += Time.deltaTime;
        //     yield return null;
        // }

        yield return new WaitForSeconds(30f);

        Animator anim = motor.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Idle");
            adamNpc.is_trolling = false;
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
        foreach (NpcController npc in _spawnedGangNpcs)
        {
            if (npc != null)
            {
                if (npc.is_trolling)
                {
                    npc.Gotshot();
                }
            }
        }
        if (motorNpc != null && motorNpc.is_trolling)
        {
            motorNpc.Gotshot();
        }
        if (adamNpc != null && adamNpc.is_trolling)
        {
            adamNpc.Gotshot();
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

        Vector3 oldPosition = npc.transform.position;
        _spawnedPositions.RemoveAll(p => Vector3.Distance(p, oldPosition) < 0.001f);

        _spawnedNpcs.Remove(npc);
        _spawnedGangNpcs.Remove(npc);
        _spawnedSpecialNpcs.Remove(npc);
        _spawnedGoodNpcs.Remove(npc);

        if (npc == motorNpc)
        {
            if (MotorPrefabs == null)
            {
                Debug.LogWarning("無法重生 Motor：MotorPrefabs 尚未配置或為空。");
                return;
            }

            Destroy(npc.gameObject);
            GameObject newMotor = Instantiate(MotorPrefabs, oldPosition, Quaternion.Euler(0f, 180f, 0f));
            motorNpc = newMotor.GetComponent<NpcController>();
            if (motorNpc != null)
            {;
                motorNpc.RandomizeAnimatorSpeed();
                motorNpc.isGoodNpc = true;
            }
            _spawnedPositions.Add(oldPosition);
            Debug.Log($"已將 Motor 重生：{newMotor.name}。");
            return;
        }

        if (npc == adamNpc)
        {
            if (AdamPrefabs == null)
            {
                Debug.LogWarning("無法重生 Adam：AdamPrefabs 尚未配置或為空。");
                return;
            }

            Destroy(npc.gameObject);
            GameObject newAdam = Instantiate(AdamPrefabs, oldPosition, Quaternion.Euler(0f, 180f, 0f));
            adamNpc = newAdam.GetComponent<NpcController>();
            if (adamNpc != null)
            {
                adamNpc.RandomizeAnimatorSpeed();
                adamNpc.isGoodNpc = true;
            }
            _spawnedPositions.Add(oldPosition);
            Debug.Log($"已將 Adam 重生：{newAdam.name}。");
            return;
        }

        GameObject[] targetGoodPrefabs = npc.isGangNpc ? goodNpcPrefabs : NonGangGoodNpcPrefabs;
        string targetName = npc.isGangNpc ? "goodNpcPrefabs" : "NonGangGoodNpcPrefabs";

        if (targetGoodPrefabs == null || targetGoodPrefabs.Length == 0)
        {
            Debug.LogWarning($"無法重生：{targetName} 尚未配置或為空。" );
            return;
        }

        int prefabIndex = npc.prefabIndex;
        if (prefabIndex < 0 || prefabIndex >= targetGoodPrefabs.Length)
        {
            Debug.LogWarning($"{npc.gameObject.name} 的 prefabIndex 無效，無法重生為對應 {targetName}。" );
            return;
        }

        Debug.Log($"正在重生 NPC: {npc.gameObject.name}，Prefab 索引: {prefabIndex}，目標陣列: {targetName}" );
        Destroy(npc.gameObject);

        GameObject newNpc = Instantiate(targetGoodPrefabs[prefabIndex], oldPosition, Quaternion.Euler(0f, 180f, 0f));
        NpcController controller = newNpc.GetComponent<NpcController>();
        if (controller != null)
        {
            controller.prefabIndex = prefabIndex;
            controller.isGoodNpc = true;
            controller.isGangNpc = npc.isGangNpc;
            controller.RandomizeAnimatorSpeed();
            _spawnedGoodNpcs.Add(controller);
        }

        _spawnedPositions.Add(oldPosition);
        Debug.Log($"已將 NPC 重生為 good NPC：{newNpc.name}，{targetName}[{prefabIndex}]。" );
    }

    public IEnumerator SpinAndRespawnNPC(NpcController npc)
    {
        if (npc == null) yield break;

        if (npc.isSpinning) yield break;

        if (npc.isGangNpc)
        {
            foreach (NpcController gangNpc in _spawnedGangNpcs)
            {
                gangNpc.GoToSpin();
            }
        }
        else
        {
            npc.GoToSpin();
        }

        yield return new WaitForSeconds(1.5f);

        if (npc.isGangNpc)
        {
            var gangListCopy = _spawnedGangNpcs.ToList(); 
            foreach (NpcController gangNpc in gangListCopy)
            {
                RespawnNPC(gangNpc);
            }
        }
        else
        {
            RespawnNPC(npc);
        }
    }
}