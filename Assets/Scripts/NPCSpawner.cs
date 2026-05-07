using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;
// using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.SceneManagement;

public class NPCSpawner : MonoBehaviour
{
    [Header("結束的聲音")]
    public AK.Wwise.Event EndEvent;

    [Header("結束的場景")]
    public SceneTransition transitionManager;
    public int finalSongIndex;

    [Header("生成設定")]
    public GameObject[] npcPrefabs;
    public GameObject[] NonGangNpcPrefabs;
    public GameObject[] goodNpcPrefabs;
    public GameObject[] NonGangGoodNpcPrefabs;
    public GameObject[] speicialPrefabs;
    public AK.Wwise.Event[] sounds;
    public GameObject MotorPrefabs;
    public GameObject MotorPigPrefabs;
    public GameObject BikePrefabs;
    public GameObject motorSpawnPosition;
    public AK.Wwise.Event motorTrollSound;
    public GameObject AdamPrefabs;
    public GameObject AdamPigPrefabs;
    public GameObject AdamSpawnPosition;
    public AK.Wwise.Event adamTrollSound;
    public GameObject[] AdamTargetPositions;
    public GameObject hitEffectPrefab;
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
    public FirestoreTest firestoreTest;

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
        // firestoreTest.SetGameState("l2");
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     RandomizeNPCAnimations();
        // }
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     StopAllNPCAnimation();
        // }
        // if (Input.GetKeyDown(KeyCode.S))
        // {
        //     StartGame();
        // }
    }

    public void StartGame()
    {
        // eventController.RegisterSpecialEvent(10f, () => {
        //     AdamWalk();
        //     Debug.Log("10 sec");
        // });
        // eventController.RegisterSpecialEvent(15f, () => {
        //     MotorTroll();
        //     Debug.Log("15 sec");
        // });
        eventController.RegisterSpecialEvent(25f, () => {
            RandomizeNPCAnimations();
            Debug.Log("25 sec");
        });

        eventController.RegisterSpecialEvent(45f, () => {
            GangNPCAnimations();
            Debug.Log("Gang start 45 sec");
        });

        eventController.RegisterSpecialEvent(72f, () => {
            AdamWalk();
            Debug.Log("72 sec");
        });

        eventController.RegisterSpecialEvent(87f, () => {
            RandomizeNPCAnimations();
            Debug.Log("87 sec");
        });

        eventController.RegisterSpecialEvent(100f, () => {
            RandomizeNPCAnimations();
            RandomizeNPCAnimations();
            Debug.Log("100 sec");
        });

        eventController.RegisterSpecialEvent(132f, () => {
            MotorTroll();
            Debug.Log("132 sec");
        });

        eventController.RegisterSpecialEvent(150f, () => {
            EndEvent.Post(gameObject);
            Debug.Log("150 sec");
        });
        eventController.RegisterSpecialEvent(163f, () => {
            Debug.Log("遊戲結束，總計時間：" + Mathf.FloorToInt(163f) + " 秒");
            AkUnitySoundEngine.StopAll(); // 停止當前所有 Wwise 音效，確保不會重疊播放
            transitionManager.goToSceneAsync(finalSongIndex);
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

    // void SpawnMotor()
    // {
    //     if (MotorPrefabs == null || motorSpawnPosition == null) return;
        
    //     Vector3 spawnPos = motorSpawnPosition.transform.position;
    //     Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f);
        
    //     GameObject motor = Instantiate(MotorPrefabs, spawnPos, spawnRotation);
        
    //     motorNpc = motor.GetComponent<NpcController>();
    //     if (motorNpc != null)
    //     {
    //         motorNpc.isFacingSinger = false;
    //     }
        
    //     _spawnedPositions.Add(spawnPos);
    // }
    void SpawnMotor()
    {
        if (MotorPrefabs == null || BikePrefabs == null || motorSpawnPosition == null) return;
        
        Vector3 spawnPos = motorSpawnPosition.transform.position;
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f);
        
        GameObject motor = Instantiate(MotorPrefabs, spawnPos, spawnRotation);
        GameObject bike = Instantiate(BikePrefabs, motor.transform.position, motor.transform.rotation, motor.transform);
        
        bike.transform.localPosition = Vector3.zero; 
        bike.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

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
            sounds[selectedNpc.prefabIndex + npcPrefabs.Length].Post(selectedNpc.gameObject);
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

        firestoreTest.SetGameState("l2");
        // 所有gang一起同時執行動畫
        foreach (NpcController npc in _spawnedGangNpcs)
        {
            if (npc != null)
            {
                npc.StopAllCoroutines(); 
                npc.StartCoroutine(npc.PlayRandomAnimation());
                sounds[npc.prefabIndex].Post(npc.gameObject);
            }
        }
    }

    public void MotorTroll(){
        if (motorNpc == null) return;
        firestoreTest.SetGameState("l2_3");
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
        firestoreTest.SetGameState("l2_2");
        Animator anim = adamNpc.GetComponent<Animator>();
        if (anim != null)
        {
            // adamNpc.is_trolling = true;
        }

        StartCoroutine(AdamMoveForward(adamNpc.gameObject, adamNpc));
    }
    
    private IEnumerator MotorMoveForward(GameObject motor, string animBoolName = "Motor")
    {
        float moveSpeed = 2f;
        float moveDuration = 10f;
        float elapsed = 0f;

        motorTrollSound.Post(motor);
        
        while (elapsed < moveDuration && motor != null)
        {
            motor.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(200);

        // Animator anim = motor.GetComponent<Animator>();
        // if (anim != null)
        // {
        //     anim.SetTrigger("Idle");
        //     motorNpc.is_trolling = false;
        // }
        motorNpc.is_trolling = false;
    }

    private IEnumerator AdamMoveForward(GameObject adam, NpcController adamController)
    {
        if (adam == null || AdamTargetPositions == null || AdamTargetPositions.Length == 0) yield break;

        Animator anim = adam.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("Running", true);
        }

        float moveSpeed = 3f;

        float groundY = -0.081f;
        float correctionThreshold = 0.05f; 

        foreach (GameObject targetObj in AdamTargetPositions)
        {
            if (targetObj == null || adam == null) break;

            Vector3 targetPos = targetObj.transform.position;

            while (adam != null && Vector3.Distance(adam.transform.position, targetPos) > 0.5f)
            {
                Vector3 currentPos = adam.transform.position;
                Vector3 direction = (targetPos - currentPos).normalized;
                
                // 1. 計算下一個位置
                Vector3 nextPos = currentPos + direction * moveSpeed * Time.deltaTime;

                // 2. 判斷是否需要修正高度
                // 如果目標點本身就在地板高度附近，且 Adam 目前也接近地板高度
                if (Mathf.Abs(targetPos.y - groundY) < 0.01f && Mathf.Abs(nextPos.y - groundY) < correctionThreshold)
                {
                    nextPos.y = groundY; // 強制吸附回地板
                }

                adam.transform.position = nextPos;

                // 面向處理
                if (direction.magnitude > 0.01f)
                {
                    Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
                    if (flatDirection.sqrMagnitude > 0.001f)
                    {
                        adam.transform.rotation = Quaternion.LookRotation(flatDirection);
                    }
                }
                
                yield return null;
            }
        }

        // 停止 Run 動畫，播放 Dance 動畫
        if (anim != null)
        {
            adamNpc.is_trolling = true;
            adamTrollSound.Post(adam);
            anim.SetBool("Running", false);
            anim.SetBool("FanDance", true);
        }

        // 持續 Dance 30 秒
        yield return new WaitForSeconds(200f);

        
        adamController.is_trolling = false;
    }

    // void StopAllNPCAnimation()
    // {
    //     foreach (NpcController npc in _spawnedNpcs)
    //     {
    //         if (npc != null)
    //         {
    //             if (npc.is_trolling)
    //             {
    //                 npc.GotShot();
    //             }
    //         }
    //     }
    //     foreach (NpcController npc in _spawnedGangNpcs)
    //     {
    //         if (npc != null)
    //         {
    //             if (npc.is_trolling)
    //             {
    //                 npc.GotShot();
    //             }
    //         }
    //     }
    //     if (motorNpc != null && motorNpc.is_trolling)
    //     {
    //         motorNpc.GotShot();
    //     }
    //     if (adamNpc != null && adamNpc.is_trolling)
    //     {
    //         adamNpc.GotShot();
    //     }
    // }

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

    public void StopSounds(NpcController npc)
    {
        if (npc == null) return;

        if (npc == motorNpc)
        {
            motorTrollSound.Stop(npc.gameObject);
        }
        else if (npc == adamNpc)
        {
            adamTrollSound.Stop(npc.gameObject);
        }
        else if (npc.isGangNpc)
        {
            sounds[npc.prefabIndex].Stop(npc.gameObject);
        }
        else
        {
            sounds[npc.prefabIndex + npcPrefabs.Length].Stop(npc.gameObject);
        }
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

        // smoke
        GameObject effect = null;       
        if (hitEffectPrefab != null)
        {
            effect = Instantiate(hitEffectPrefab, oldPosition,  Quaternion.Euler(0, 0, 0));
        }

        if (npc == motorNpc)
        {
            if (MotorPrefabs == null)
            {
                Debug.LogWarning("無法重生 Motor：MotorPrefabs 尚未配置或為空。");
                return;
            }

            Destroy(npc.gameObject);
            GameObject newMotor;
            oldPosition.y = -0.081f;
            if (MotorPigPrefabs != null) {
                newMotor = Instantiate(MotorPigPrefabs, oldPosition, Quaternion.Euler(0f, 180f, 0f));
            }
            else {
                newMotor = Instantiate(MotorPrefabs, oldPosition, Quaternion.Euler(0f, 180f, 0f));
            }
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
            GameObject newAdam;
            oldPosition.y = -0.081f;
            if (AdamPigPrefabs != null) {
                newAdam = Instantiate(AdamPigPrefabs, oldPosition, Quaternion.Euler(0f, 180f, 0f));
            }
            else {
                newAdam = Instantiate(AdamPrefabs, oldPosition, Quaternion.Euler(0f, 180f, 0f));
            }
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

        // if (npc.isGangNpc)
        // {
        //     foreach (NpcController gangNpc in _spawnedGangNpcs)
        //     {
        //         gangNpc.GoToSpin();
        //     }
        // }
        // else
        // {
        //     npc.GoToSpin();
        // }
        StopSounds(npc);
        npc.GoToSpin();

        yield return new WaitForSeconds(1.5f);

        // if (npc.isGangNpc)
        // {
        //     var gangListCopy = _spawnedGangNpcs.ToList(); 
        //     foreach (NpcController gangNpc in gangListCopy)
        //     {
        //         RespawnNPC(gangNpc);
        //     }
        // }
        // else
        // {
        //     RespawnNPC(npc);
        // }
        RespawnNPC(npc);
    }
}