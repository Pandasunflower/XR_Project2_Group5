using Cinemachine.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbySceneManagerShowcase : MonoBehaviour
{
    [Header("Dependencies")]
    public LobbySongManagerShowcase songManager;
    public SceneTransition transitionManager;
    public FirestoreTest firestoreTest;
    public List<int> songIndexMapping;

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
        string sceneName = "";
        switch (songIndex)
        {
            case 0:
                sceneName = "davewang";
                break;
            case 1:
                sceneName = "frozen";
                break;
            default:
                sceneName = "";
                break;
        }
        int finalSongIndex;
        // int finalSongIndex = songIndex;
        // if (songIndex == 0)
        // {
        //     finalSongIndex = 1;
        // }
        // else if (songIndex == 1)
        // {
        //     finalSongIndex = 2;
        // }
        switch (sceneName)
        {
            case "davewang":
                finalSongIndex = songIndexMapping.Count > 0 ? songIndexMapping[0] : 1;
                break;
            case "frozen":
                finalSongIndex = songIndexMapping.Count > 1 ? songIndexMapping[1] : 2;
                break;
            default:
                finalSongIndex = -1;
                break;
        }

        if (finalSongIndex == -1)
        {
            Debug.Log($"[SceneManager] Invalid song index");
            return;
        }

        firestoreTest.SetOption(finalSongIndex);
        firestoreTest.SetGameState("init");
        Debug.Log($"[SceneManager] 加載場景: {sceneName}，歌曲索引: {finalSongIndex}，選定歌曲: {songName}");
        // SceneManager.LoadScene(sceneName);
        GameConfig.SelectedGame = songIndex;
        transitionManager.goToSceneAsync(finalSongIndex); // 加載 Stage1 或 Stage2
    }
}