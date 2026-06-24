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
    public float midiXOffset = 1.92f;
    [Space(10)]
    public float visualHeight = 0.3f;
    public float visualDepth = 0.3f;
    public float additionalYOffset = 0f; // 整體垂直位移（以視覺範圍中心為基準）
    public float additionalZOffset = 2.0f;

    [Header("音高自動範圍")]
    [Tooltip("所有音符 Y 軸映射的總高度（世界單位），上下各一半")]
    public float visualYRange = 5f;
    [Tooltip("讓音高範圍上下各多留一點餘白（semitone）")]
    public float midiPadding = 1f;

    [Header("偵測結果（唯讀）")]
    [SerializeField] private float _detectedMidiMin;
    [SerializeField] private float _detectedMidiMax;

    [Header("音高條顏色狀態")]
    public Color nextNoteColor = new Color(0.7f, 0.7f, 0.7f, 1f);   
    public Color currentNoteColor = new Color(1f, 0f, 0.6f, 1f);    
    public Color playedNoteColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
    public Color wrongNoteColor = new Color(1f, 0f, 0f, 1f);
    public Color correctNoteColor = new Color(0f, 1f, 0f, 1f);

    private class NoteVisual
    {
        public GameObject go;
        public Renderer renderer;
        public Material material;
        public float startTime;
        public float endTime;
        public float midi;
        public bool wasCorrect = false;
        public bool wasEvaluated = false;
        public Color currentColor = Color.clear;
    }

    private List<NoteVisual> spawnedNotes = new List<NoteVisual>();
    private AudioSource _unityAudio;
    private bool isInitialized = false;
    private float _currentTargetMidi = 0f;
    private float _midiTolerance = 3f;

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
        foreach (var n in spawnedNotes) if (n != null && n.go != null) Destroy(n.go);
        spawnedNotes.Clear();

        if (PitchDataManager.Instance == null || PitchDataManager.Instance.CurrentSongNotes == null) return;

        var notes = PitchDataManager.Instance.CurrentSongNotes.notes;
        if (notes == null || notes.Count == 0) return;

        // ── 第一遍：掃出整首歌的音高上下限 ──
        float midiMin = float.MaxValue;
        float midiMax = float.MinValue;
        foreach (var note in notes)
        {
            if (note.midi < midiMin) midiMin = note.midi;
            if (note.midi > midiMax) midiMax = note.midi;
        }
        midiMin -= midiPadding;
        midiMax += midiPadding;
        _detectedMidiMin = midiMin;
        _detectedMidiMax = midiMax;
        Debug.Log($"[KaraokeScrollViewer] 偵測音高範圍：{midiMin:F1} ~ {midiMax:F1}，visualYRange={visualYRange}");

        // ── 第二遍：用偵測範圍 remap Y 位置 ──
        float midiRange = Mathf.Max(midiMax - midiMin, 1f);

        foreach (var note in notes)
        {
            float width = note.duration * timeToXScale;
            float xPosStart = note.startTime * timeToXScale;

            float normalizedMidi = (note.midi - midiMin) / midiRange; // 0 ~ 1
            float yPos = (normalizedMidi - 0.5f) * visualYRange + additionalYOffset;

            GameObject go = Instantiate(notePrefab, transform);
            go.transform.localScale = new Vector3(width, visualHeight, visualDepth);
            float xPosCentered = xPosStart + (width / 2f) + midiXOffset;
            go.transform.localPosition = new Vector3(xPosCentered, yPos, additionalZOffset);

            Renderer r = go.GetComponent<Renderer>();
            Material mat = null;
            if (r != null)
            {
                mat = r.material;
                mat.color = nextNoteColor;
            }

            spawnedNotes.Add(new NoteVisual
            {
                go = go,
                renderer = r,
                material = mat,
                startTime = note.startTime,
                endTime = note.startTime + note.duration,
                midi = note.midi,
                currentColor = nextNoteColor
            });
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
        float midiDiff = float.NaN;
        bool hasMidiDiff = false;

        if (singingManager != null)
        {
            midiDiff = singingManager.GetMidiDiff();
            hasMidiDiff = !float.IsNaN(midiDiff);
        }

        foreach (var note in spawnedNotes)
        {
            if (note == null || note.go == null) continue;

            Color targetColor;
            if (currentTime < note.startTime)
            {
                targetColor = nextNoteColor;
            }
            else if (currentTime >= note.startTime && currentTime <= note.endTime)
            {
                if (hasMidiDiff && Mathf.Abs(midiDiff) < _midiTolerance)
                {
                    targetColor = correctNoteColor;
                    note.wasCorrect = true;
                    note.wasEvaluated = true;
                }
                else
                {
                    targetColor = currentNoteColor;
                    if (hasMidiDiff)
                    {
                        note.wasEvaluated = true;
                    }
                }
            }
            else
            {
                if (!note.wasEvaluated)
                {
                    // 如果已經經過該 note 但尚未評估，視作未命中
                    targetColor = wrongNoteColor;
                    note.wasEvaluated = true;
                }
                else if (note.wasCorrect)
                {
                    targetColor = correctNoteColor;
                }
                else
                {
                    targetColor = wrongNoteColor;
                }
            }

            SetNoteColor(note, targetColor);
        }
    }

    void SetNoteColor(NoteVisual note, Color col)
    {
        if (note.material == null || note.currentColor == col) return;
        note.material.color = col;
        note.currentColor = col;
    }
}