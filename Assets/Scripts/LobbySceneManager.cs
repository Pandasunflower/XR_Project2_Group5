using UnityEngine;

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
        // --- PLACEHOLDER FOR FUTURE IMPLEMENTATION ---
        // 1. Save the 'songName' to a Static variable or ScriptableObject 
        //    so the next scene knows what to load.
        // 2. Call SceneManager.LoadScene("GameScene");
        // 3. Handle loading screen animations.
        
        Debug.Log($"[SYSTEM] Transition logic for '{songName}' would execute here.");
    }
}