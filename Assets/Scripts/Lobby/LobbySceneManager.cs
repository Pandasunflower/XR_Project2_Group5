using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneManager : MonoBehaviour
{
    [Header("Dependencies")]
    public LobbySongManager songManager;
    public SceneTransition transitionManager;
    public FirestoreTest firestoreTest;

    void Start()
    {
        // firestoreTest.SetOption(0);
    }

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
        songManager.StopPreviewMusic();
        
        // 根據索引決定加載 Stage1 或 Stage2
        string sceneName = songIndex == 0 ? "davewang" : "frozen";
        int finalSongIndex = songIndex;
        if (songIndex == 0)
        {
            finalSongIndex = 1;
        }
        else if (songIndex == 1)
        {
            finalSongIndex = 2;
        }

        firestoreTest.SetOption(finalSongIndex);
        firestoreTest.SetGameState("init");
        Debug.Log($"[SceneManager] 加載場景: {sceneName}，歌曲索引: {finalSongIndex}，選定歌曲: {songName}");
        // SceneManager.LoadScene(sceneName);
        transitionManager.goToSceneAsync(finalSongIndex); // 加載 Stage1 或 Stage2
    }
}