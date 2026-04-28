using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class SingingManager : MonoBehaviour {

    [Header("Wwise Settings")]
    public AK.Wwise.Event bgmEvent;
    public AK.Wwise.Event vocalEvent;
    public GameObject wwiseAudioObject;

    [Header("Settings")]
    public TextAsset jsonFile;
    public AudioSource bgmSource;
    public float timeScale = 2.0f;
    public float pitchScale = 0.5f;
    public Vector3 positionOffset;
    public float pitchOffset = -60f;

    [Header("References")]
    public GameObject pitchLinePrefab;
    public Transform lineContainer;
    public Transform playerIndicator;
    public MicrophoneInput micInput;

    private SongPitchData _data;
    private int _currentIndex = 0;

    [Header("Movement Settings")]
    public float smoothSpeed = 10f;
    public float minMidi = 48f;
    public float maxMidi = 84f;

    private float _currentVisualMidi = 0f;

    [Header("Visual Persistence")]
    public float hideDelay = 0.3f;
    private float _hideTimer = 0f;

    [Header("Python-Unity Alignment")]
    public float midiOffset = 0f;
    public float midiMultiplier = 1f;

    [Header("Visual Layout")]
    public float indicatorXPosition = 1f;

    [Header("Scoring")]
    public float currentTotalScore = 0f;
    private int _totalCheckedFrames = 0;
    private float _accumulatedPoints = 0f;

    public LineRenderer lineRenderer;
    public int maxPoints = 50;
    public float xSpacing = 0.2f;
    public float multiplier = 2.0f; 
    private float[] _errorHistory;

    private uint _playingID = 0;
    private bool _isPaused = false; 

    private List<string> _songList = new List<string>();

    public void Start(){
        LoadSongFolders();
        string selected = _songList[_currentIndex].Trim(); 
        string jsonPath = Path.Combine("StreamingAssets/Songs", selected, "pitch_data.json.txt");
        string folderPath = Path.Combine("StreamingAssets/Songs", selected);
        StartGame(jsonPath, folderPath);
    }

    public void StartGame(string jsonRelativePath, string mp3RelativePath) {
        StopAllCoroutines();
        // bgmSource.Stop();
        _playingID = bgmEvent.Post(wwiseAudioObject);
        if (micInput != null && micInput.testVocalSource != null) micInput.testVocalSource.Stop();

        Debug.Log($"<color=blue>開始載入歌曲資源...</color>\nJSON 路徑: {jsonRelativePath}\n");
        StartCoroutine(LoadSongResources(jsonRelativePath, mp3RelativePath));
    }

    void LoadSongFolders() {
        Debug.Log("找到歌曲數量: " + _songList.Count);
        string fullPath = Path.Combine(Application.dataPath, "StreamingAssets/Songs");

        if (Directory.Exists(fullPath)) {
            Debug.Log(fullPath  + " 內的資料夾:");
            string[] dirs = Directory.GetDirectories(fullPath, "*"); 
            foreach (string d in dirs) {
                Debug.Log("找到歌曲資料夾: " + d);
                _songList.Add(Path.GetFileName(d));
            }
        }
    }

    private IEnumerator LoadSongResources(string jsonRelativePath, string folderPath)
    {
        string fullJsonPath = Path.Combine(Application.dataPath, jsonRelativePath).Replace("\\", "/");
        // Debug.Log($"path: {fullJsonPath}");
        if (File.Exists(fullJsonPath)) {
            _data = JsonUtility.FromJson<SongPitchData>(File.ReadAllText(fullJsonPath));
            Debug.Log("<color=green>JSON 載入成功！</color>");
        } else {
            Debug.LogError($"找不到 JSON: {fullJsonPath}");
            yield break; 
        }

        string bgmPath = Path.Combine(Application.dataPath, folderPath, "bgm.wav").Replace("\\", "/");
        string bgmUrl = "file://" + bgmPath;
        yield return StartCoroutine(LoadAudio(bgmUrl, (clip) => {
            bgmSource.clip = clip;
        }, AudioType.WAV));

        string vocalPath = Path.Combine(Application.dataPath, folderPath, "vocal.wav").Replace("\\", "/");
        string vocalUrl = "file://" + vocalPath;
        yield return StartCoroutine(LoadAudio(vocalUrl, (clip) => {
            if (micInput != null && micInput.testVocalSource != null) {
                micInput.testVocalSource.clip = clip;
            }
        }, AudioType.WAV));

        if (_data != null) {
            _currentIndex = 0;
            _hasFinished = false;

            CalculateSongRange();
            SpawnPitchLines();

            // --- Wwise 播放取代 bgmSource.Play() ---
            if (bgmEvent.IsValid()) {
                bgmEvent.Post(wwiseAudioObject); 
                Debug.Log("<color=cyan>Wwise BGM Event Posted!</color>");
            } else {
                Debug.LogError("Wwise BGM Event 未設定或無效！");
            }

            // 如果有模擬人聲
            if (micInput != null && micInput.useSimulatedVocal) {
                vocalEvent.Post(wwiseAudioObject);
            }
        }
    }

    private IEnumerator LoadAudio(string url, System.Action<AudioClip> callback, AudioType type = AudioType.WAV) {
        // 處理 URL 中的空格，避免 404 或格式錯誤
        string sanitizedUrl = url.Replace(" ", "%20");

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(sanitizedUrl, type)) {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null) {
                    callback(clip);
                    Debug.Log($"<color=cyan>成功載入音訊: {sanitizedUrl}</color>");
                }
            } else {
                Debug.LogError($"音訊載入失敗!\nURL: {sanitizedUrl}\nError: {www.error}");
            }
        }
    }
    void SpawnPitchLines() {
        foreach (Transform child in lineContainer) Destroy(child.gameObject);

        GameObject lineObj = Instantiate(pitchLinePrefab, lineContainer);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();

        // Debug.Log("lineContainer位置: " + lineContainer.position);

        // lr.useWorldSpace = false; 

        lr.positionCount = _data.frames.Count;
        for (int i = 0; i < _data.frames.Count; i++) {
            float x = _data.frames[i].t * timeScale;
            float y = (_data.frames[i].m + pitchOffset) * pitchScale;

            Vector3 pos = new Vector3(x+positionOffset.x, y+positionOffset.y, 0) ;
            
            lr.SetPosition(i, pos);
        }
    }

    void Update() {
        // --- Wwise 播放狀態檢查 ---
        if (_playingID == 0) return;

        // 取得 Wwise 目前播放位置 (單位：毫秒 ms)
        int out_ms;
        AKRESULT res = AkSoundEngine.GetSourcePlayPosition(_playingID, out out_ms);
        
        // 如果 Wwise 沒在跑或是讀不到位置，就 return
        if (res != AKRESULT.AK_Success) return;

        float currentTime = out_ms / 1000f; // 轉為秒，對接你原本的 JSON 時間
        
        // 總長度檢查 (如果你 Wwise Event 有設定長度，也可以手動填入)
        // 這裡暫時維持原本 logic，但注意 Wwise 不直接提供 clip.length
        float totalDuration = (_data.frames.Count > 0) ? _data.frames[_data.frames.Count - 1].t : 999f;

        if (currentTime >= totalDuration - 0.1f && !_hasFinished) {
            OnSongFinished();
            return;
        }

        // --- 以下邏輯與你原本的基本一致，確保座標正確 ---
        lineContainer.position = Vector3.zero;

        // 更新索引：根據 Wwise 時間找到現在該唱哪一個點
        while (_currentIndex < _data.frames.Count && _data.frames[_currentIndex].t < currentTime) {
            _currentIndex++;
        }

        UpdatePitchWindow(currentTime);

        if (_currentIndex < _data.frames.Count) {
            float targetMidi = _data.frames[_currentIndex].m;
            float rawUserMidi = micInput != null ? micInput.GetCurrentPitchFiltered() : 0;
            float userMidi = (rawUserMidi > 0) ? (rawUserMidi * midiMultiplier) + midiOffset : 0;

            if (playerIndicator != null) {
                if (userMidi > 0 || _hideTimer > 0) {
                    _hideTimer = hideDelay; 
                    playerIndicator.gameObject.SetActive(true);

                    float clampedMidi = Mathf.Clamp(userMidi, minMidi, maxMidi);

                    if (_currentVisualMidi <= 0) _currentVisualMidi = clampedMidi;
                    _currentVisualMidi = Mathf.Lerp(_currentVisualMidi, clampedMidi, Time.deltaTime * smoothSpeed);

                    // 讓 Indicator 留在 X = 0 (或你設定的 indicatorXPosition)，Y 軸反應音高
                    playerIndicator.position = new Vector3(indicatorXPosition, _currentVisualMidi * pitchScale, 0);
                    EvaluateScore(targetMidi, _currentVisualMidi); 
                    
                } else {
                    if (_hideTimer > 0) {
                        _hideTimer -= Time.deltaTime;
                    } else {
                        playerIndicator.gameObject.SetActive(false);
                        _currentVisualMidi = 0; 
                    }
                }
            }
        }
    }

    void UpdatePitchWindow(float currentTime) {
        LineRenderer lr = lineContainer.GetComponentInChildren<LineRenderer>();
        if (lr == null) return;

        float windowStart = currentTime - 2.0f;
        float windowEnd = currentTime + 5.0f;

        List<Vector3> windowPoints = new List<Vector3>();

        // 這裡可以優化：從 _currentIndex 開始往前後找，不要遍歷整個 List
        for (int i = 0; i < _data.frames.Count; i++) {
            float frameTime = _data.frames[i].t;
            if (frameTime >= windowStart && frameTime <= windowEnd) {
                float relativeX = (frameTime - currentTime) * timeScale;
                windowPoints.Add(new Vector3(relativeX, _data.frames[i].m * pitchScale, 0));
            }
        }

        lr.positionCount = windowPoints.Count;
        lr.SetPositions(windowPoints.ToArray());
    }

    void EvaluateScore(float target, float user) {
        if (user <= 0) return;
        float diff = Mathf.Abs(target - user);
        float framePoint = 0f;
        // Debug.Log($"目標 MIDI: {target:F2}, 玩家 MIDI: {user:F2}, 差距: {diff:F2}");

        if (diff < 1f) {
            framePoint = 100f;
            _totalCheckedFrames++;
            // Debug.Log("<color=cyan>Perfect!</color>");
        } else if (diff < 3f) {
            framePoint = 85f;
            _totalCheckedFrames++;
            // Debug.Log("<color=yellow>Great</color>");
        } else if (diff < 5f) {
            framePoint = 70f;
            _totalCheckedFrames++;
            // Debug.Log("<color=white>Good</color>");
        }
        _accumulatedPoints += framePoint;
        currentTotalScore = _accumulatedPoints / _totalCheckedFrames;
        if (_totalCheckedFrames % 100 == 1) {
            Debug.Log("目前分數：" + currentTotalScore);
        }
        // Debug.Log("目前分數：" + currentTotalScore);
    }

    void CalculateSongRange() {
        if (_data.frames == null || _data.frames.Count == 0) return;

        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (var frame in _data.frames) {
            if (frame.m > 0) {
                if (frame.m < min) min = frame.m;
                if (frame.m > max) max = frame.m;
            }
        }

        minMidi = min - 2f;
        maxMidi = max + 2f;

        Debug.Log($"[Song Setup] 自動偵測音域: Min={min:F2}, Max={max:F2}。設定範圍為: {minMidi:F2} ~ {maxMidi:F2}");
    }

    private bool _hasFinished = false;
    void OnSongFinished() {
        if (_hasFinished) return;
        _hasFinished = true;

        UnityEngine.Debug.Log("<color=orange>🎵 音樂播放完畢！進入結算畫面</color>");

        bgmSource.Stop();
        if (micInput != null && micInput.testVocalSource != null) {
            micInput.testVocalSource.Stop();
        }
        ShowFinalScore(); 
    }

    void ShowFinalScore() {
        Debug.Log($"<color=orange>=== 演唱結束 ===</color>");
        Debug.Log($"<color=orange>最終得分: {currentTotalScore:F2} / 100</color>");

        if (currentTotalScore > 85) Debug.Log("評語: 歌神降臨！");
        else if (currentTotalScore > 60) Debug.Log("評語: 唱得不錯喔！");
        else Debug.Log("評語: 再接再厲！");

        this.enabled = false;
    }
}