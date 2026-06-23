using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 在 Awake（其他腳本 Start 之前）根據 GameConfig.SelectedGame 設定場景內各元件的歌曲資料。
/// 在 Inspector 依照 GameConfig.SelectedGame 的 index 填入每首歌的設定即可。
/// </summary>
public class GeneralSceneSongSelector : MonoBehaviour
{
    [System.Serializable]
    public class SongEntry
    {
        [Tooltip("對應 SingingManager.songFolderName，StreamingAssets/Songs 底下的資料夾名稱")]
        public string songFolderName;

        [Tooltip("對應 KaraokeScrollViewer.songFolder")]
        public string karaokeFolder;

        [Tooltip("對應 PlayWwiseEvent.myWwiseEvent")]
        public AK.Wwise.Event wwiseEvent;
        public AK.Wwise.Event wwiseEvent2;

        [Tooltip("對應 LyricSystem.lrcFile")]
        public TextAsset lrcFile;
    }

    [Header("Song Entries（index = GameConfig.SelectedGame）")]
    public List<SongEntry> songs = new List<SongEntry>();

    [Header("Target Components")]
    public SingingManager singingManager;
    public KaraokeScrollViewer karaokeScrollViewer;
    public PlayWwiseEvent playWwiseEvent;
    public PlayWwiseEvent2 playWwiseEvent2;
    public LyricSystem lyricSystem;

    void Awake()
    {
        int Originindex = GameConfig.SelectedGame;

        int index = 0;

        switch (Originindex)
        {
            case 0:
                index = 0;
                break;
            case 1:
                index = 1;
                break;
            case 2:
                index = 2;
                break;
            case 3:
                index = 3;
                break;
            default:
                index = 0;
                Debug.LogWarning($"[GeneralSceneSongSelector] Originindex={Originindex} 不在預期範圍，使用 index=0");
                break;
        }
        Debug.Log($"[GeneralSceneSongSelector] Originindex={Originindex}，對應 index={index}，準備套用歌曲資料。");

        if (index < 0 || index >= songs.Count)
        {
            Debug.LogWarning($"[GeneralSceneSongSelector] SelectedGame={index} 超出 songs 範圍（共 {songs.Count} 筆），略過套用。");
            return;
        }

        SongEntry entry = songs[index];

        if (singingManager != null)
        {
            singingManager.songFolderName = entry.songFolderName;
            Debug.Log($"[GeneralSceneSongSelector] SingingManager.songFolderName = {entry.songFolderName}");
        }

        if (karaokeScrollViewer != null)
        {
            karaokeScrollViewer.songFolder = entry.karaokeFolder;
            Debug.Log($"[GeneralSceneSongSelector] KaraokeScrollViewer.songFolder = {entry.karaokeFolder}");
        }

        if (playWwiseEvent != null)
        {
            playWwiseEvent.myWwiseEvent = entry.wwiseEvent;
            playWwiseEvent2.myWwiseEvent = entry.wwiseEvent2;
            Debug.Log($"[GeneralSceneSongSelector] PlayWwiseEvent.myWwiseEvent = {entry.wwiseEvent?.Name}");
            Debug.Log($"[GeneralSceneSongSelector] PlayWwiseEvent2.myWwiseEvent = {entry.wwiseEvent2?.Name}");
        }

        if (lyricSystem != null)
        {
            lyricSystem.lrcFile = entry.lrcFile;
            Debug.Log($"[GeneralSceneSongSelector] LyricSystem.lrcFile = {entry.lrcFile?.name}");
        }

        Debug.Log($"[GeneralSceneSongSelector] 套用完成，SelectedGame={index}，歌曲資料夾：{entry.songFolderName}");
    }
}
