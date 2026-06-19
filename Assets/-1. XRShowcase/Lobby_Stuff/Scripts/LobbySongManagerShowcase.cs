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
    public List<string> songFolders = new List<string>();

    public List<string> songNames = new List<string>();

    [Header("UI References")]
    public TextMeshProUGUI currentSongText;
    public TextMeshProUGUI songIndexText;
    public Image songCover;
    public Sprite defaultCover;

    private void Start()
    {
        RefreshSongList();
        SetSongNames();
        UpdateUI();
    }

    public void RefreshSongList()
    {
        songFolders.Clear();
        string path = Path.Combine(Application.streamingAssetsPath, songsFolderName);

        if (Directory.Exists(path))
        {
            // Get all subdirectories (each represents a song)
            string[] directories = Directory.GetDirectories(path);
            foreach (string dir in directories)
            {
                songFolders.Add(Path.GetFileName(dir));
                Debug.Log($"[Lobby] Found song folder: {dir}");
            }
            Debug.Log($"[Lobby] Found {songFolders.Count} songs.");
        }
        else
        {
            Debug.LogError($"[Lobby] Path not found: {path}");
        }
    }
    public void SetSongNames()
    {
         songNames.Add("For the First Time in Forever");
         songNames.Add("一場遊戲一場夢");
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

    public string GetSelectedSongPath()
    {
        if (songFolders.Count == 0) return null;
        return songFolders[currentSelectedIndex];
    }

    public int GetSelectedSongIndex()
    {
        return currentSelectedIndex;
    }

    private void OnSelectionChanged()
    {
        Debug.Log($"[Lobby] Currently Selected: {songFolders[currentSelectedIndex]}");
        // Trigger UI updates here in the future
    }

    public void UpdateUI()
    {
        Debug.Log("Update UI");
        if (songFolders.Count == 0)
        {
            if (currentSongText != null) currentSongText.text = "No Songs Found";
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
        string folderName = songFolders[currentSelectedIndex];
        string fileName = "cover.png";
        string fullPath = Path.Combine(Application.streamingAssetsPath, songsFolderName, folderName, fileName);

        string uri = fullPath;
        if (!uri.Contains("://")) uri = "file://" + uri;
        Debug.Log($"[Lobby] Loading cover from: {uri}");

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri)) {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success) {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                
                if (texture == null) {
                    Debug.LogError("[Lobby] 雖然請求成功，但 Texture2D 物件為空 (可能是格式錯誤)");
                    yield break;
                }

                texture.filterMode = FilterMode.Point; // 保持點陣感
                
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                songCover.sprite = newSprite;
                
                Debug.Log($"[Lobby] 封面載入成功: {texture.width}x{texture.height}");
            } else {
                Debug.LogError($"[Lobby] 請求失敗！錯誤訊息: {uwr.error}");
                Debug.LogError($"[Lobby] 回應代碼: {uwr.responseCode}");
                songCover.sprite = defaultCover;
            }
        }
    }
    // 🎵 新增：處理 Wwise 預覽音樂的核心邏輯
    private void PlayPreviewMusic()
    {
        // 1. 先把這個物件上「正在播放的所有音樂」暴力停掉，避免兩首歌疊在一起
        AkUnitySoundEngine.StopAll(gameObject); 

        // 2. 組合你的 Wwise Event 名稱
        // 假設你的資料夾叫 "frozen"，你的 Wwise Event 叫 "Play_frozen"
        string folderName = songFolders[currentSelectedIndex];
        string eventName = "Play_" + folderName; 

        // 🔍 加這行在 Console 看名字對不對
        Debug.Log($"[Wwise Debug] 嘗試播放 Event: {eventName}");

        // 3. 呼叫 Wwise 播放
        AkUnitySoundEngine.PostEvent(eventName, gameObject);
    }

    public void StopPreviewMusic()
    {
        AkUnitySoundEngine.StopAll(gameObject);
    }
}