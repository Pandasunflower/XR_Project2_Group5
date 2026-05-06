using UnityEngine;
using System.Collections.Generic;
using AK.Wwise;

public class KaraokeScrollViewer : MonoBehaviour
{
    [Header("資料來源")]
    public PitchDataManager pitchDataManager; 

    [Header("JSON 路徑控制")]
    [Tooltip("StreamingAssets 下的子資料夾路徑")]
    public string songFolder = "StreamingAssets/Songs/davewang"; 
    [Tooltip("JSON 檔案名稱 (需含 .txt)")]
    public string jsonName = "note_segments.json.txt";

    [Header("播放源設定")]
    [Tooltip("拖入發音源物件 (例如人聲物件)，會自動偵測 AudioSource")]
    public GameObject audioProviderObject;
    [Tooltip("若使用 Wwise，可由外部設定此 PlayingID")]
    public uint wwisePlayingID = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;

    [Header("音準檢測")]
    [Tooltip("拖入 SingingManager 來取得音準差異")]
    public SingingManager singingManager;

    [Header("視覺映射參數")]
    public GameObject notePrefab;
    public float timeToXScale = 10f;  
    public float midiToYScale = 1.0f; 
    public float midiYOffset = 60f;
    public float midiXOffset = 1.92f;
    [Space(10)]
    public float visualHeight = 0.3f;    // 方塊厚度
    public float visualDepth = 0.3f;     // 方塊深度
    public float additionalYOffset = 2.0f; // 整體垂直位移
    public float additionalZOffset = 2.0f;

    [Header("音高條顏色狀態")]
    public Color nextNoteColor = new Color(0.7f, 0.7f, 0.7f, 1f);   
    public Color currentNoteColor = new Color(1f, 0f, 0.6f, 1f);    
    public Color playedNoteColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
    public Color wrongNoteColor = new Color(1f, 0f, 0f, 1f);
    public Color correctNoteColor = new Color(0f, 1f, 0f, 1f);

    private List<GameObject> spawnedNotes = new List<GameObject>();
    private AudioSource _unityAudio;
    private bool isInitialized = false;
    private float _currentTargetMidi = 0f;  // 存储当前应唱的目标 MIDI 音高
    private float _midiTolerance = 3f;  // 允许的 MIDI 差异 (1 = 正确)

    void Start()
    {
        // 嘗試獲取拖入物件的 AudioSource
        if (audioProviderObject != null)
            _unityAudio = audioProviderObject.GetComponent<AudioSource>();

        // 自動啟動加載流程
        StartKaraoke();
    }

    public void StartKaraoke()
    {
        if (PitchDataManager.Instance != null)
        {
            PitchDataManager.Instance.LoadSongData(songFolder, jsonName);
            StartCoroutine(WaitAndSetup());
        }
        else
        {
            Debug.LogError("場景中找不到 PitchDataManager Instance！");
        }
    }

    System.Collections.IEnumerator WaitAndSetup()
    {
        // 等待 DataManager 標記為已載入
        while (!PitchDataManager.Instance.isDataLoaded)
        {
            yield return null; 
        }
        SetupVisuals();
    }

    void SetupVisuals()
    {
        GenerateVisualNotes();
        isInitialized = true;
    }

    // 提供給外部腳本 (如 MusicManager) 設定 Wwise ID 的接口
    public void SetWwisePlayingID(uint id)
    {
        wwisePlayingID = id;
    }

    // 設定音準容差 (預設為 1 semitone = 100 cents)
    public void SetMidiTolerance(float tolerance)
    {
        _midiTolerance = tolerance;
    }

    void GenerateVisualNotes()
    {
        foreach (var n in spawnedNotes) if (n != null) Destroy(n);
        spawnedNotes.Clear();

        if (PitchDataManager.Instance == null || PitchDataManager.Instance.CurrentSongNotes == null) return;

        var notes = PitchDataManager.Instance.CurrentSongNotes.notes;

        foreach (var note in notes)
        {
            float width = note.duration * timeToXScale;
            float xPosStart = note.startTime * timeToXScale;
            // 計算 MIDI 音高位置並加上額外垂直位移
            float yPos = ((note.midi - midiYOffset) * midiToYScale) + additionalYOffset;

            GameObject go = Instantiate(notePrefab, transform);

            // 設定大小 (變小變扁)
            go.transform.localScale = new Vector3(width, visualHeight, visualDepth);

            // 位移邏輯：補償 Pivot 在中心點的問題 (Start + 寬度一半)
            float xPosCentered = xPosStart + (width / 2f) + midiXOffset;
            go.transform.localPosition = new Vector3(xPosCentered, yPos, additionalZOffset);

            SetNoteColor(go, nextNoteColor);
            spawnedNotes.Add(go);
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        float currentTime = GetCurrentTimeFromSources();

        // 移動捲簾 (Parent 物件向左移動)
        float targetX = -currentTime * timeToXScale;
        transform.localPosition = new Vector3(targetX, transform.localPosition.y, transform.localPosition.z);

        UpdateNoteColors(currentTime);
    }

    float GetCurrentTimeFromSources()
    {
        // 1. 優先檢查 Wwise
        if (wwisePlayingID != AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
        {
            int out_pos;
            AKRESULT result = AkUnitySoundEngine.GetSourcePlayPosition(wwisePlayingID, out out_pos);
            if (result == AKRESULT.AK_Success) return out_pos / 1000f;
        }

        // 2. 其次檢查 Unity AudioSource
        if (_unityAudio != null && _unityAudio.isPlaying)
        {
            return _unityAudio.time;
        }

        return 0f;
    }

    void UpdateNoteColors(float currentTime)
    {
        foreach (var note in spawnedNotes)
        {
            if (note == null) continue;

            // 根據 localPosition.x 逆推起點時間 (需補償 Pivot 造成的位移)
            float width = note.transform.localScale.x;
            float noteStart = (note.transform.localPosition.x - (width / 2f)) / timeToXScale;
            float noteEnd = noteStart + (width / timeToXScale);

            if (currentTime < noteStart)
            {
                SetNoteColor(note, nextNoteColor);
            }
            else if (currentTime >= noteStart && currentTime <= noteEnd)
            {
                // 当前正在播放的音符
                SetNoteColor(note, currentNoteColor);
            }
            else
            {
                // 已播放的音符：通過 SingingManager 檢查當時是否音準正確
                if (singingManager != null)
                {
                    float midiDiff = singingManager.GetMidiDiff();
                    if (midiDiff != 0 && Mathf.Abs(midiDiff) < _midiTolerance)
                        SetNoteColor(note, correctNoteColor);  // 音準正確時顯示綠色
                    else
                        SetNoteColor(note, wrongNoteColor);
                }
                else
                {
                    SetNoteColor(note, playedNoteColor);
                }
            }
        }
    }

    void SetNoteColor(GameObject go, Color col)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.material.color = col;
    }
}