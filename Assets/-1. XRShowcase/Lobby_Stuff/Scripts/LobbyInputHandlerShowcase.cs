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

    // [Header("Controller Settings")]
    // [Range(0.1f, 0.9f)]
    // public float stickThreshold = 0.5f;

    void Awake()
    {
        BuildTvOutlineList();
        BuildVideoPlayerList();
        ApplyOutlineWidth();
    }

    private void ApplyOutlineWidth()
    {
        foreach (var outlines in _tvOutlines)
            foreach (var outline in outlines)
                if (outline != null) outline.OutlineWidth = outlineWidth;
    }

    void Start()
    {
        DisableAllTvOutlines();

        int startIndex = songManager != null ? songManager.currentSelectedIndex : 0;
        SetActiveTv(startIndex);
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

    void Update()
    {
        if (Time.time - _lastInputTime < inputCooldown) return;

        // Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        // Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        // Vector2 stickPos = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        // float h = Input.GetAxisRaw("Horizontal");
        // float h = stickPos.x;
        
        bool ButtonPressed = OVRInput.GetDown(OVRInput.RawButton.A);
        bool ButtonPressedNext = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);
        bool ButtonPressedPrev = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);

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

        if (ButtonPressedNext || Input.GetKeyDown(KeyCode.A))
        {
            songManager.NextSong();
            _lastInputTime = Time.time;
            songManager.UpdateUI();
            SetActiveTv(songManager.currentSelectedIndex);
        }
        if (ButtonPressedPrev || Input.GetKeyDown(KeyCode.D))
        {
            songManager.PreviousSong();
            _lastInputTime = Time.time;
            songManager.UpdateUI();
            SetActiveTv(songManager.currentSelectedIndex);
        }

        if (ButtonPressed || Input.GetKeyDown(KeyCode.Return))
        {
            sceneManager.RequestStartGame();
            _lastInputTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleCurrentTvOutline();
        }
    }
}