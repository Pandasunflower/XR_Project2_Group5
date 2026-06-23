using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LobbyInputHandlerShowcase : MonoBehaviour
{
    [Header("Core References")]
    public LobbySongManagerShowcase songManager;
    public LobbySceneManagerShowcase sceneManager;

    [Header("Input Settings")]
    public float inputCooldown = 0.2f;
    private float _lastInputTime;

    [Header("TV Outline Settings")]
    [Tooltip("拖入 TVs 父物件，底下每個子物件（TV、TV2...）依序對應第 1、2、3... 首歌")]
    public Transform tvsParent;
    [Tooltip("套用到所有 TV Outline 的粗度")]
    public float outlineWidth = 5f;

    // 每個 TV 底下蒐集到的 Outline（TV 本身或 Screen 子物件上的都算）
    private readonly List<List<Outline>> _tvOutlines = new List<List<Outline>>();
    private int _currentTvIndex = -1;
    private bool _outlineVisible = true;

    [Header("Video Player Settings")]
    [Tooltip("拖入 VideoPlayers 父物件，底下每個子物件（VideoPlayer1、VideoPlayer2...）依序對應第 1、2、3... 首歌")]
    public bool rewindOnSelect = true;
    public Transform videoPlayersParent;

    private readonly List<VideoPlayer> _videoPlayers = new List<VideoPlayer>();

    [Header("Screen Select Settings")]
    [Tooltip("OVRCameraRig 的 trackingSpace（可不填，自動尋找）")]
    public Transform trackingSpace;
    [Tooltip("從 controller 發出射線的最大距離")]
    public float screenSelectRayDistance = 10f;
    [Tooltip("Screen Collider 所在的 Layer，留空（Everything）則不限制")]
    public LayerMask screenSelectLayerMask = ~0;

    // [Header("Controller Settings")]
    // [Range(0.1f, 0.9f)]
    // public float stickThreshold = 0.5f;

    void Awake()
    {
        BuildTvOutlineList();
        BuildVideoPlayerList();
        ApplyOutlineWidth();
    }

    void OnEnable()
    {
        if (trackingSpace == null)
        {
            var rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null)
                trackingSpace = rig.trackingSpace;
        }
    }

    private void ApplyOutlineWidth()
    {
        foreach (var outlines in _tvOutlines)
            foreach (var outline in outlines)
                if (outline != null) outline.OutlineWidth = outlineWidth;
    }

    private void ApplyOutlineMode()
    {
        foreach (var outlines in _tvOutlines)
            foreach (var outline in outlines)
                if (outline != null) outline.OutlineMode = Outline.Mode.OutlineVisible;
    }

    void Start()
    {
        ApplyOutlineMode();
        DisableAllTvOutlines();
        RegisterTvAudioObjects();

        int startIndex = songManager != null ? songManager.currentSelectedIndex : 0;
        SetActiveTv(startIndex);
    }

    // 把所有 TV GameObject 註冊到 songManager，供 PlayPreviewMusic 的 StopAll 使用
    private void RegisterTvAudioObjects()
    {
        if (songManager == null || tvsParent == null) return;

        songManager.allTvAudioObjects.Clear();
        foreach (Transform tv in tvsParent)
        {
            songManager.allTvAudioObjects.Add(tv.gameObject);
            Debug.Log($"[RegisterTvAudioObjects] 已註冊: {tv.name}");
        }

        if (songManager.allTvAudioObjects.Count > 0)
        {
            songManager.audioSourceObject = songManager.allTvAudioObjects[0];
            Debug.Log($"[RegisterTvAudioObjects] 預設音源設為: {songManager.audioSourceObject.name}");
        }

        Debug.Log($"[RegisterTvAudioObjects] 共註冊 {songManager.allTvAudioObjects.Count} 個 TV");
    }

    private void DisableAllTvOutlines()
    {
        foreach (var outlines in _tvOutlines)
            foreach (var outline in outlines)
                if (outline != null) outline.enabled = false;
    }

    private void BuildTvOutlineList()
    {
        _tvOutlines.Clear();
        if (tvsParent == null) return;

        foreach (Transform tv in tvsParent)
        {
            var outlines = new List<Outline>(tv.GetComponentsInChildren<Outline>(true));
            _tvOutlines.Add(outlines);
        }
    }

    private void BuildVideoPlayerList()
    {
        _videoPlayers.Clear();
        if (videoPlayersParent == null) return;

        foreach (Transform vp in videoPlayersParent)
        {
            _videoPlayers.Add(vp.GetComponentInChildren<VideoPlayer>(true));
        }
    }

    private void SetActiveTv(int index)
    {
        if (_tvOutlines.Count > 0)
        {
            if (_currentTvIndex >= 0 && _currentTvIndex < _tvOutlines.Count)
            {
                foreach (var outline in _tvOutlines[_currentTvIndex])
                    if (outline != null) outline.enabled = false;
            }

            if (index >= 0 && index < _tvOutlines.Count)
            {
                foreach (var outline in _tvOutlines[index])
                    if (outline != null) outline.enabled = true;
            }
        }

        _currentTvIndex = index;
        _outlineVisible = true; // 切到新的一定先點亮

        if (rewindOnSelect)
        {
            RewindVideo(index);
        }
    }

    private void RewindVideo(int index)
    {
        if (index < 0 || index >= _videoPlayers.Count) return;

        VideoPlayer vp = _videoPlayers[index];
        if (vp == null) return;

        vp.frame = 0;
    }

    private void ToggleCurrentTvOutline()
    {
        if (_currentTvIndex < 0 || _currentTvIndex >= _tvOutlines.Count) return;

        _outlineVisible = !_outlineVisible;

        foreach (var outline in _tvOutlines[_currentTvIndex])
            if (outline != null) outline.enabled = _outlineVisible;
    }

    // ──────────────────────────────────────────────────────────
    //  按下 trigger 時，若射線打到某個 TV 底下的 Screen Collider，直接選擇對應的歌
    // ──────────────────────────────────────────────────────────
    private void TrySelectSongByRaycast(OVRInput.Controller controller)
    {
        if (tvsParent == null) return;

        Ray ray = GetControllerRay(controller);

        if (!Physics.Raycast(ray, out RaycastHit hit, screenSelectRayDistance, screenSelectLayerMask))
            return;

        int index = GetTvIndexFromHit(hit.transform);
        if (index < 0) return;

        SelectSong(index);
    }

    // 從被打到的物件往上找，直到找到「tvsParent 的直接子物件」，回傳它在 tvsParent 底下的順序
    private int GetTvIndexFromHit(Transform hitTransform)
    {
        Transform current = hitTransform;
        while (current != null)
        {
            if (current.parent == tvsParent)
                return current.GetSiblingIndex();

            current = current.parent;
        }
        return -1;
    }

    private void SelectSong(int index)
    {
        _lastInputTime = Time.time;

        // 不管有沒有對應的歌曲，TV 的高亮永遠可以正常切換（純視覺，跟歌曲資料無關）
        SetActiveTv(index);

        if (songManager == null)
        {
            Debug.Log("[LobbyInputHandlerShowcase] songManager 未指定，無法選歌。");
            return;
        }

        // 告訴 songManager 要從哪個 TV 物件發出聲音
        songManager.audioSourceObject = (tvsParent != null && index >= 0 && index < tvsParent.childCount)
            ? tvsParent.GetChild(index).gameObject
            : null;

        bool success;
        try
        {
            // songManager.SelectSong 內部會設定 currentSelectedIndex、更新 UI，並播放預覽音樂
            success = songManager.SelectSong(index);
        }
        catch (System.Exception e)
        {
            songManager.currentSelectedIndex = -1;
            Debug.Log($"[LobbyInputHandlerShowcase] 選歌時發生例外，已重設為 -1。錯誤：{e.Message}");
            success = false;
        }

        if (!success)
        {
            HandleNoMatchingSong(index);
        }
    }

    private IEnumerator StopMusicThenStartGame()
    {
        if (songManager != null) songManager.StopPreviewMusic();
        yield return new WaitForSeconds(0.05f);
        sceneManager.RequestStartGame();
    }

    // 此 TV 沒有對應的歌曲時的處理入口（之後可在這裡加特殊彩蛋邏輯，目前先留空）
    private void HandleNoMatchingSong(int index)
    {
        // TODO: 之後在這裡針對「沒有對應歌曲」的 TV (index) 加上特殊彩蛋效果
    }

    private Ray GetControllerRay(OVRInput.Controller controller)
    {
        Vector3 localPos = OVRInput.GetLocalControllerPosition(controller);
        Quaternion localRot = OVRInput.GetLocalControllerRotation(controller);

        Vector3 worldPos;
        Vector3 worldForward;

        if (trackingSpace != null)
        {
            worldPos = trackingSpace.TransformPoint(localPos);
            worldForward = trackingSpace.TransformDirection(localRot * Vector3.forward);
        }
        else
        {
            worldPos = localPos;
            worldForward = localRot * Vector3.forward;
        }

        return new Ray(worldPos, worldForward);
    }

    void Update()
    {
        if (Time.time - _lastInputTime < inputCooldown) return;

        // Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        // Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        // Vector2 stickPos = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        // float h = Input.GetAxisRaw("Horizontal");
        // float h = stickPos.x;
        
        bool ButtonPressed = OVRInput.GetDown(OVRInput.RawButton.A);
        // bool ButtonPressedNext = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);
        // bool ButtonPressedPrev = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);

        // if (h < -stickThreshold || Input.GetKeyDown(KeyCode.A))
        // {
        //     songManager.PreviousSong();
        //     _lastInputTime = Time.time;
        //     songManager.UpdateUI();
        // }
        // else if (h > stickThreshold || Input.GetKeyDown(KeyCode.D))
        // {
        //     songManager.NextSong();
        //     _lastInputTime = Time.time;
        //     songManager.UpdateUI();
        // }

        // if (ButtonPressedNext || Input.GetKeyDown(KeyCode.A))
        // {
        //     songManager.NextSong();
        //     _lastInputTime = Time.time;
        //     songManager.UpdateUI();
        //     SetActiveTv(songManager.currentSelectedIndex);
        // }
        // if (ButtonPressedPrev || Input.GetKeyDown(KeyCode.D))
        // {
        //     songManager.PreviousSong();
        //     _lastInputTime = Time.time;
        //     songManager.UpdateUI();
        //     SetActiveTv(songManager.currentSelectedIndex);
        // }

        if (ButtonPressed || Input.GetKeyDown(KeyCode.Return))
        {
            _lastInputTime = Time.time;
            StartCoroutine(StopMusicThenStartGame());
        }

        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     ToggleCurrentTvOutline();
        // }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            TrySelectSongByRaycast(OVRInput.Controller.LTouch);

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            TrySelectSongByRaycast(OVRInput.Controller.RTouch);
    }
}