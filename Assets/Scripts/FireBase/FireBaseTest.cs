using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class FirestoreTest : MonoBehaviour
{
    FirebaseFirestore db;

    Dictionary<string, GameObject> playerCubes = new Dictionary<string, GameObject>();
    public GameObject cubePrefab;
    public GameObject cubePrefab2;

    public GameObject[] playerPrefabs;
    public TextMeshPro resultText;
    public int stage = 1; //1: stage1, 3: stage3
    public int option = 0;

    public Transform[] NPCpos;
    public Transform target;

    public SingingManager singingManager;

    public AK.Wwise.Event wwiseScoreEvent;

    public AK.Wwise.Event wwiseEndEvent;

    public GameObject hostObject;
    


    public BoxCollider endTrigger; // 階段1結束碰撞器

    public GameObject signPrefab;
    public Transform wallParent;
    public int index = 0;

    public TrailerAudience trailerAudience;

    private string currentGameState = "";

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        ListenPlayers();
        if (resultText != null)
            resultText.text = "";
        SetOption(option);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetGameState("init");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetGameState("l1_lobby");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // SetGameState("l1_voting");
            singingManager.ShowFinalScore();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetGameState("l1_end");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetGameState("l2");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetGameState("l3_lobby");
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SetGameState("l3_voting");
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SetGameState("l3_votingend");
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SetGameState("l3_sign");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SetOption(1);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            SetOption(2);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SetGameState("l2_2");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            SetGameState("l2_3");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnTestSigns(30);
        }
    }

    public void SetGameState(string state)
    {
        DocumentReference docRef = db.Collection("game").Document("state");

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "value", state }
        };

        docRef.SetAsync(data).ContinueWithOnMainThread(task =>
        {
            Debug.Log("State changed to: " + state);
        });

        if (state == "init") ClearPlayers();
        else if (state == "l1_lobby") StartCoroutine(trailerAudience.SetCrowdAnimators(1));
        else if (state == "l1_voting")
        {
            StartCoroutine(trailerAudience.SetCrowdAnimators(3));
        }
        else if (state == "l1_end") CalculateAverageLevel1();

        currentGameState = state;
    }

    public void SetOption(int option)
    {
        if (option == -1) return;
        DocumentReference docRef = db.Collection("game").Document("option");

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "value", option }
        };

        docRef.SetAsync(data).ContinueWithOnMainThread(task =>
        {
            Debug.Log("Option changed to: " + option);
        });
    }

    void ListenPlayers()
    {
        db.Collection("players").Listen(snapshot =>
        {
            foreach (var change in snapshot.GetChanges())
            {
                string id = change.Document.Id;
                var data = change.Document.ToDictionary();

                if (change.ChangeType == DocumentChange.Type.Added)
                {
                    if (stage == 1)
                        SpawnPlayer(id, data);
                    else if (stage == 3)
                        SpawnPlayer2(id, data);
                }

                if (change.ChangeType == DocumentChange.Type.Modified)
                {
                    UpdatePlayer(id, data);
                }

            }
        });
    }

    void SpawnPlayer(string id, IDictionary<string, object> data)
    {
        int index = playerCubes.Count;

        Vector3 spawnPos = NPCpos[index].position;

        GameObject cube;
        quaternion rot = Quaternion.Euler(0, 180, 0);
        if (data["level1Character"].ToString() == "0")
        { 
            cube = Instantiate(playerPrefabs[0], spawnPos, rot);
            if (currentGameState == "l1_lobby")
                cube.GetComponent<Animator>().SetBool("Waving", true);
            
        }
        else if (data["level1Character"].ToString() == "1")
        {
            cube = Instantiate(playerPrefabs[1], spawnPos, rot);
            if (currentGameState == "l1_lobby")
                cube.GetComponent<Animator>().SetBool("Waving", true);
        }
        else if (data["level1Character"].ToString() == "2")
        {
            cube = Instantiate(playerPrefabs[2], spawnPos, rot);
            if (currentGameState == "l1_lobby")
                cube.GetComponent<Animator>().SetBool("Waving", true);
        }
        else if (data["level1Character"].ToString() == "3")
        {
            cube = Instantiate(playerPrefabs[3], spawnPos, rot);
            if (currentGameState == "l1_lobby")
                cube.GetComponent<Animator>().SetBool("Waving", true);
        }
        else
        {
            cube = Instantiate(cubePrefab, spawnPos, rot);
        }
        
        cube.name = id;
        playerCubes[id] = cube;
        TextMeshPro text = cube.GetComponentInChildren<TextMeshPro>();
        text.text = data["name"].ToString();

        Debug.Log("Spawn player: " + id);
    }

    void SpawnPlayer2(string id, IDictionary<string, object> data)
    {
        int index = playerCubes.Count;

        Vector3 spawnPos = NPCpos[index].position;
        Vector3 dir = (target.position - spawnPos);
        dir.y = 0;

        GameObject cube;
        Quaternion rot = Quaternion.Euler(0, -35, 0);
        // Quaternion rot2 = Quaternion.LookRotation(dir) * rot;
        Quaternion rot2 = Quaternion.LookRotation(dir) * rot;
        if (data["level1Character"].ToString() == "0")
        {
            cube = Instantiate(playerPrefabs[0], spawnPos, rot2);
        }
        else if (data["level1Character"].ToString() == "1")
        {
            cube = Instantiate(playerPrefabs[1], spawnPos, rot2);
        }
        else if (data["level1Character"].ToString() == "2")
        {
            cube = Instantiate(playerPrefabs[2], spawnPos, rot2);
        }
        else if (data["level1Character"].ToString() == "3")
        {
            cube = Instantiate(playerPrefabs[3], spawnPos, rot2);
        }
        else
        {
            cube = Instantiate(cubePrefab, spawnPos, rot2);
        }
        
        cube.name = id;
        playerCubes[id] = cube;
        TextMeshPro text = cube.GetComponentInChildren<TextMeshPro>();
        text.text = data["name"].ToString();

        Debug.Log("Spawn player: " + id);
    }

    // void SpawnPlayer2(string id, IDictionary<string, object> data)
    // {
    //     int index = playerCubes.Count;

    //     Vector3 spawnPos = new Vector3(index * 2f, 0, 0); // 每個間隔2

    //     GameObject cube = Instantiate(cubePrefab, spawnPos, Quaternion.identity);

    //     cube.name = id;

    //     playerCubes[id] = cube;

    //     TextMeshPro text = cube.GetComponentInChildren<TextMeshPro>();
    //     text.text = data["name"].ToString();

    //     Debug.Log("Spawn player: " + id);
    // }

    void UpdatePlayer(string id, IDictionary<string, object> data)
    {
        if (!playerCubes.ContainsKey(id)) return;

        GameObject cube = playerCubes[id];

        // Renderer r = cube.GetComponentInChildren<Renderer>();

        bool level1HasVoted = data.ContainsKey("level1HasVoted") && (bool)data["level1HasVoted"];
        bool level3HasVoted = data.ContainsKey("level3HasVoted") &&(bool)data["level3HasVoted"];
        string level3sign = data.ContainsKey("level3Sign") ? data["level3Sign"].ToString() : "";

        if (level1HasVoted)
        {
            // r.material.color = Color.red;
            cube.GetComponent<Animator>().SetBool("Voting", true);
        }

        if (level3HasVoted)
        {
            // r.material.color = Color.blue;
        }
        if (level3sign != "")
        {
            int vote3 = System.Convert.ToInt32(data["level3"]);
            CreateSign(Base64ToTexture(level3sign), vote3);
            // float scale = GetScale(vote3);
            // Texture2D tex = Base64ToTexture(level3sign);

            // GameObject obj = Instantiate(signPrefab, wallParent);
            // int col = 5;
            // obj.GetComponent<Renderer>().material.mainTexture = tex;
            // obj.transform.localScale = new Vector3(scale * obj.transform.localScale.x, scale * obj.transform.localScale.y, obj.transform.localScale.z);
            // float spacingX = 0.9f * obj.transform.localScale.x;
            // float spacingY = 1.5f * obj.transform.localScale.y;

            // float xIndex = 0;
            // if (index == 0)
            // {
            //     xIndex = 0;
            // }
            // else
            // {
            //     int offsetIndex = (index + 1) / 2;
            //     int direction = (index % 2 == 0) ? 1 : -1;
            //     xIndex = offsetIndex * direction;
            // }

            // obj.transform.localPosition = new Vector3(
            //     xIndex * spacingX,
            //     -(index / col) * spacingY,
            //     0
            // );

            // index++;
        }
    }

    float GetScale(int vote)
    {
        if (vote < 10) return 0.8f;
        if (vote < 60) return 1.0f;
        if (vote < 200) return 1.5f;
        return 2.2f;
    }

    Texture2D Base64ToTexture(string base64)
    {
        if (base64.Contains(","))
            base64 = base64.Split(',')[1];

        byte[] bytes = System.Convert.FromBase64String(base64);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        return tex;
    }


    public void SetAllWaving()
    {
        foreach (var kv in playerCubes)
        {
            kv.Value.GetComponent<Animator>().SetBool("Waving", true);
        }
    }

    public void SetAllClapping()
    {
        foreach (var kv in playerCubes)
        {
            kv.Value.GetComponent<Animator>().SetBool("Clapping", true);
        }
    }

    public void ClearPlayers()
    {
        db.Collection("players").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError(task.Exception.Flatten());
                return;
            }

            QuerySnapshot snapshot = task.Result;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                doc.Reference.DeleteAsync();
            }

            Debug.Log("All players deleted");
        });
    }

    void CalculateAverageLevel1()
    {
        db.Collection("players").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to get players");
                return;
            }

            QuerySnapshot snapshot = task.Result;

            float total = 0f;
            int count = 0;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                if (data.ContainsKey("level1"))
                {
                    float score = System.Convert.ToSingle(data["level1"]);

                    if (score > 0)
                    {
                        total += score;
                        count++;
                    }
                }
            }

            float voteScore = count > 0 ? total / count : 0;
            float singingScore = singingManager.GetFinalScore();
            float finalScore = (voteScore + singingScore) / 2f;
            Debug.Log("平均分數: " + voteScore);
            Debug.Log("演唱分數: " + singingScore);
            Debug.Log("最終分數: " + finalScore);
            wwiseScoreEvent.Post(gameObject); // 播放分數揭曉音效
            StartCoroutine(ShowAnimatedResult(finalScore));
        });
    }

    void ShowResult(float avg)
    {
        resultText.text = avg.ToString("F1");
    }

    IEnumerator ShowAnimatedResult(float realScore)
    {
        int targetInt = Mathf.RoundToInt(realScore);

        // 第一階段：亂數跳動
        for (int i = 0; i < 30; i++)
        {
            int random = UnityEngine.Random.Range(1, 11);

            ShowResult(random);

            float delay = Mathf.Lerp(0.015f, 0.18f, i / 30f);

            yield return new WaitForSeconds(delay);
        }

        // 第二階段：接近目標
        int start = UnityEngine.Random.Range(1, 6);

        for (int i = start; i <= targetInt; i++)
        {
            ShowResult(i);

            yield return new WaitForSeconds(0.15f);
        }

        // 第三階段：顯示真實分數
        yield return new WaitForSeconds(0.3f);

        ShowResult(realScore);
        yield return StartCoroutine(FinalPunchEffect());
        wwiseEndEvent.Post(gameObject); // 播放結束音效
        PlayHostAnimation();
        if (endTrigger != null)
            endTrigger.enabled = true; // 啟用碰撞器
    }

    void PlayHostAnimation() {
        // Debug.Log($"Triggering host animation {hostObject.name}");
        hostObject.GetComponent<Animator>().SetTrigger("point");
    }

    IEnumerator FinalPunchEffect()
    {
        Transform t = resultText.transform; // 你的分數 UI
        Vector3 original = Vector3.one;

        // 放大
        float time = 0;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.5f, time / 0.2f);
            t.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        // 彈回
        time = 0;
        while (time < 0.15f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1.5f, 1f, time / 0.15f);
            t.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        t.localScale = original;
    }

    [SerializeField] private Texture2D testTexture;

    public void SpawnTestSigns(int count = 30)
    {
        for (int i = 0; i < count; i++)
        {
            int vote = UnityEngine.Random.Range(1, 300);

            CreateSign(testTexture, vote);
        }
    }

    void CreateSign(Texture2D tex, int vote3)
    {
        float scale = GetScale(vote3);

        GameObject obj = Instantiate(signPrefab, wallParent);

        obj.GetComponent<Renderer>().material.mainTexture = tex;

        // ===== 縮小倍率 =====
        scale *= 0.01f;

        obj.transform.localScale = new Vector3(
            scale*2,
            scale,
            0.000001f
        );

        // ===== Grid 設定 =====
        int col = 5;

        float spacingX = 1.2f;
        float spacingY = 1.5f;

        // ===== 算 row / col =====
        int row = index / col;
        int column = index % col;

        // 置中
        float startX = -(col - 1) * spacingX * 0.5f;

        float x = startX + column * spacingX;
        float y = -row * spacingY;

        obj.transform.localPosition = new Vector3(x*scale*1.2f, 0, y*scale*2);

        index++;
    }
}