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
            case 2:
                sceneName = "eatMe";
                break;
            case 3:
                sceneName = "forgetUfogetMe";
                break;
            case 4:
                sceneName = "ILikeU";
                break;
            case 5:
                sceneName = "weiweimonmon";
                break;
            case 6:
                sceneName = "Silence";
                break;
            default:
                sceneName = "(no name)";
                break;
        }
        int finalSongIndex;
        finalSongIndex = 5;
        // int finalSongIndex = songIndex;
        // if (songIndex == 0)
        // {
        //     finalSongIndex = 1;
        // }
        // else if (songIndex == 1)
        // {
        //     finalSongIndex = 2;
        // }
        // switch (songIndex)
        // {
        //     case 0:
        //         finalSongIndex = songIndexMapping.Count > 0 ? songIndexMapping[0] : 1;
        //         break;
        //     case 1:
        //         finalSongIndex = songIndexMapping.Count > 1 ? songIndexMapping[1] : 2;
        //         break;
        //     case 2:
        //         finalSongIndex = songIndexMapping.Count > 2 ? songIndexMapping[2] : 3;
        //         break;
        //     case 3:
        //         finalSongIndex = songIndexMapping.Count > 3 ? songIndexMapping[3] : 4;
        //         break;
        //     case 4:
        //         finalSongIndex = songIndexMapping.Count > 4 ? songIndexMapping[4] : 5;
        //         break;
        //     case 5:
        //         finalSongIndex = songIndexMapping.Count > 5 ? songIndexMapping[5] : 6;
        //         break;
        //     case 6:
        //         finalSongIndex = songIndexMapping.Count > 6 ? songIndexMapping[6] : 7;
        //         break;
        //     default:
        //         finalSongIndex = -1;
        //         break;
        // }

        // if (finalSongIndex == -1)
        // {
        //     Debug.Log($"[SceneManager] Invalid song index");
        //     return;
        // }
        firestoreTest.SetOption(finalSongIndex);
        firestoreTest.SetGameState("init");
        Debug.Log($"[SceneManager] 加載場景: {sceneName}，歌曲索引: {finalSongIndex}，選定歌曲: {songName}");
        // SceneManager.LoadScene(sceneName);
        GameConfig.SelectedGame = songIndex;
        transitionManager.goToSceneAsync(finalSongIndex); // 加載 Stage1 或 Stage2
    }
}