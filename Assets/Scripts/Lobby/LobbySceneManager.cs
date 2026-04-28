using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneManager : MonoBehaviour
{
    [Header("Dependencies")]
    public LobbySongManager songManager;

    public void RequestStartGame()
    {
        string selectedSong = songManager.GetSelectedSongPath();

        if (string.IsNullOrEmpty(selectedSong))
        {
            Debug.LogWarning("[SceneManager] No song selected!");
            return;
        }

        Debug.Log($"[SceneManager] Preparing to launch: {selectedSong}");
        ExecuteTransition(selectedSong);
    }

    private void ExecuteTransition(string songName)
    {
        // 獲取選定歌曲的索引
        int songIndex = songManager.GetSelectedSongIndex();
        
        // 根據索引決定加載 Stage1 或 Stage2
        string sceneName = songIndex == 0 ? "pitchTest" : "Stage1";
        
        Debug.Log($"[SceneManager] 加載場景: {sceneName}，歌曲索引: {songIndex}，選定歌曲: {songName}");
        SceneManager.LoadScene(sceneName);
    }
}