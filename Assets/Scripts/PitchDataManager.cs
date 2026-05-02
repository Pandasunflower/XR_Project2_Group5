using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class NoteSegment {
    public float startTime;
    public float midi;
    public float duration;
}

[System.Serializable]
public class SongPitchDataTest {
    public List<NoteSegment> notes;
}

public class PitchDataManager : MonoBehaviour {
    public static PitchDataManager Instance;
    public SongPitchDataTest CurrentSongNotes;
    public bool isDataLoaded = false;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void LoadSongData(string songFolder, string jsonName) {
        isDataLoaded = false;
        
        // 確保路徑拼接正確
        string relativePath = Path.Combine(songFolder, jsonName);
        string fullJsonPath = Path.Combine(Application.dataPath, relativePath);
        
        // 統一斜線並處理 Windows 的反斜線問題
        fullJsonPath = fullJsonPath.Replace("\\", "/");

        if (File.Exists(fullJsonPath)) {
            try {
                // 讀取檔案內容
                string jsonContent = File.ReadAllText(fullJsonPath);
                
                // 使用你指定的 SongPitchDataTest 結構進行解析
                CurrentSongNotes = JsonUtility.FromJson<SongPitchDataTest>(jsonContent);
                
                if (CurrentSongNotes != null && CurrentSongNotes.notes != null) {
                    isDataLoaded = true;
                    Debug.Log($"<color=green>成功讀取 JSON:</color> {fullJsonPath}, 共 {CurrentSongNotes.notes.Count} 筆資料");
                } else {
                    Debug.LogError("JSON 解析成功但資料內容為空，請檢查 JSON 格式是否符合 SongPitchDataTest 結構");
                }
            } catch (System.Exception e) {
                Debug.LogError($"解析 JSON 時發生錯誤: {e.Message}");
            }
        } else {
            Debug.LogError($"找不到檔案: {fullJsonPath}");
        }
    }
}