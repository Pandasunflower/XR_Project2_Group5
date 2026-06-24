using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using TMPro;
using System.Collections.Generic;


using Debug = UnityEngine.Debug;

public class LobbySongManagerShowcase : MonoBehaviour
{
    [Header("Data Path")]
    public string songsFolderName = "Songs"; // Inside StreamingAssets
    
    [Header("Selected State")]
    public int currentSelectedIndex = 0;
    public bool useDefaultSongs = true;
    public List<string> songFolders = new List<string>();

    public List<string> songNames = new List<string>();

    [Header("UI References")]
    public TextMeshProUGUI currentSongText;
    public TextMeshProUGUI songIndexText;
    public Image songCover;
    public Sprite defaultCover;

    [Header("SongList")]
    public List<AK.Wwise.Event> songList;

    [Header("Wwise Audio Source")]
    [Tooltip("目前選中的 TV GameObject，由 LobbyInputHandlerShowcase 在選歌時自動設定")]
    public GameObject audioSourceObject;
    [Tooltip("所有 TV 的 GameObject，由 LobbyInputHandlerShowcase 在 Start 時填入，StopAll 時會逐一停止")]
    public List<GameObject> allTvAudioObjects = new List<GameObject>();

    // [Header("Preview Music Objects")]
    // public GameObject previewMusicObject;

    // private void Awake()
    // {
    //     allTvAudioObjects.Clear();
    // }

    private void Start()
    {
        // RefreshSongList();
        if (useDefaultSongs)
        {
            SetSongFolders();
            SetSongNames();
        }
        UpdateUI();
    }

    // public void RefreshSongList()
    // {
    //     songFolders.Clear();
    //     string path = Path.Combine(Application.streamingAssetsPath, songsFolderName);

    //     if (Directory.Exists(path))
    //     {
    //         // Get all subdirectories (each represents a song)
    //         string[] directories = Directory.GetDirectories(path);
    //         foreach (string dir in directories)
    //         {
    //             songFolders.Add(Path.GetFileName(dir));
    //             Debug.Log($"[Lobby] Found song folder: {dir}");
    //         }
    //         Debug.Log($"[Lobby] Found {songFolders.Count} songs.");
    //     }
    //     else
    //     {
    //         Debug.LogError($"[Lobby] Path not found: {path}");
    //     }
    // }

    public void SetSongFolders()
    {
        songFolders.Add("davewang");
        songFolders.Add("frozen");
        Debug.Log($"[Lobby] Song folders set: {string.Join(", ", songFolders)}");
    }
    public void SetSongNames()
    {
        songNames.Add("一場遊戲一場夢");
        songNames.Add("For the First Time in Forever");
        Debug.Log($"[Lobby] Song names set: {string.Join(", ", songNames)}");
    }


    public void NextSong()
    {
        if (songFolders.Count == 0) return;
        currentSelectedIndex = (currentSelectedIndex + 1) % songFolders.Count;
        OnSelectionChanged();
    }

    public void PreviousSong()
    {
        if (songFolders.Count == 0) return;
        currentSelectedIndex = (currentSelectedIndex - 1 + songFolders.Count) % songFolders.Count;
        OnSelectionChanged();
    }

    // 直接依 index 選歌（例如從 Raycast 選 TV 觸發）。會更新 UI 並播放預覽音樂。
    // index 沒有對應的歌曲時，currentSelectedIndex 會被設為 -1，並回傳 false。
    public bool SelectSong(int index)
    {
        if (index < 0 || index >= songFolders.Count)
        {
            currentSelectedIndex = -1;
            AkUnitySoundEngine.StopAll();
            Debug.Log($"[Lobby] SelectSong: index {index} 沒有對應的歌曲，已設為 -1。");
            return false;
        }

        currentSelectedIndex = index;
        OnSelectionChanged();
        UpdateUI(); // UpdateUI 內部會呼叫 PlayPreviewMusic() 播放預覽音樂

        Debug.Log($"[Lobby] SelectSong: 已選擇第 {index + 1} 首歌（{songFolders[index]}）。");
        return true;
    }

    public string GetSelectedSongPath()
    {
        if (!IsCurrentIndexValid()) return null;
        return songFolders[currentSelectedIndex];
    }

    // currentSelectedIndex 可能因為「選到沒有對應歌曲的 TV」而被設成 -1，這裡統一判斷是否能安全索引 songFolders
    private bool IsCurrentIndexValid()
    {
        return currentSelectedIndex >= 0 && currentSelectedIndex < songFolders.Count;
    }

    public int GetSelectedSongIndex()
    {
        return currentSelectedIndex;
    }

    private void OnSelectionChanged()
    {
        if (!IsCurrentIndexValid())
        {
            Debug.Log("[Lobby] Currently Selected: 無（currentSelectedIndex 無效）");
            return;
        }

        Debug.Log($"[Lobby] Currently Selected: {songFolders[currentSelectedIndex]}");
        // Trigger UI updates here in the future
    }

    public void UpdateUI()
    {
        Debug.Log("Update UI");
        if (songFolders.Count == 0 || !IsCurrentIndexValid())
        {
            if (currentSongText != null) currentSongText.text = "No Songs Found";
            if (songIndexText != null) songIndexText.text = "--/--";
            if (songCover != null && defaultCover != null) songCover.sprite = defaultCover;
            return;
        }

        if (currentSongText != null && songNames.Count > 0)
        {
            currentSongText.text = songNames[(currentSelectedIndex+1) % songNames.Count];
        }

        if (songIndexText != null)
        {
            songIndexText.text = $"{currentSelectedIndex + 1:D2} / {songFolders.Count:D2}";
        }

        StopAllCoroutines(); 
        StartCoroutine(LoadCoverAsync());

        PlayPreviewMusic();

        Debug.Log($"[Lobby UI] Selected: {songFolders[currentSelectedIndex]}");
    }
    IEnumerator LoadCoverAsync() {
        if (!IsCurrentIndexValid())
        {
            Debug.Log("[Lobby] currentSelectedIndex 無效，略過載入封面。");
            yield break;
        }

        string folderName = songFolders[currentSelectedIndex];
        string fileName = "cover.png";
        string fullPath = Path.Combine(Application.streamingAssetsPath, songsFolderName, folderName, fileName);

        string uri = fullPath;
        if (!uri.Contains("://")) uri = "file://" + uri;
        Debug.Log($"[Lobby] Loading cover from: {uri}");

        // using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri)) {
        //     yield return uwr.SendWebRequest();

        //     if (uwr.result == UnityWebRequest.Result.Success) {
        //         Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                
        //         if (texture == null) {
        //             Debug.LogWarning("[Lobby] 雖然請求成功，但 Texture2D 物件為空 (可能是格式錯誤)");
        //             yield break;
        //         }

        //         texture.filterMode = FilterMode.Point; // 保持點陣感
                
        //         Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //         if (newSprite != null) songCover.sprite = newSprite;
                
        //         Debug.Log($"[Lobby] 封面載入成功: {texture.width}x{texture.height}");
        //     } else {
        //         Debug.LogWarning($"[Lobby] 請求失敗！錯誤訊息: {uwr.error}");
        //         Debug.LogWarning($"[Lobby] 回應代碼: {uwr.responseCode}");
        //         if (defaultCover != null) songCover.sprite = defaultCover;
        //     }
        // }
    }
    // 停止所有 TV 物件上的 Wwise 音效
    private void StopAllTvAudio()
    {
        foreach (var tv in allTvAudioObjects)
            if (tv != null) AkUnitySoundEngine.StopAll(tv);
    }

    // 🎵 處理 Wwise 預覽音樂的核心邏輯
    public void PlayPreviewMusic()
    {
        // 1. 停止所有 TV 上正在播放的音樂，避免疊音
        StopAllTvAudio();

        if (!IsCurrentIndexValid())
        {
            AkUnitySoundEngine.StopAll(); // 全域保險，確保完全安靜
            Debug.Log("[Lobby] currentSelectedIndex 無效，已停止所有音效，略過播放預覽音樂。");
            return;
        }

        // 2. 組合 Wwise Event 名稱
        string folderName = songFolders[currentSelectedIndex];
        string eventName = "Play_" + folderName;
        Debug.Log($"[Wwise Debug] 嘗試播放 Event: {eventName}");

        // 3. 從選中的 TV 物件發出聲音（由 LobbyInputHandlerShowcase 設定 audioSourceObject）
        GameObject source = audioSourceObject != null ? audioSourceObject : gameObject;
        Debug.Log($"[Wwise Debug] 音源物件: {source.name} (InstanceID: {source.GetInstanceID()})");
        AkUnitySoundEngine.PostEvent(eventName, source);
    }

    public void StopPreviewMusic()
    {
        StopAllTvAudio();
        AkUnitySoundEngine.StopAll();
    }
}