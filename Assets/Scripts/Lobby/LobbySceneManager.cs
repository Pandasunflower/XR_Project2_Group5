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
        Debug.Log($"[SceneManager] 準備加載 Stage1 場景，選定歌曲: {songName}");
        SceneManager.LoadScene("Stage1");
    }
}