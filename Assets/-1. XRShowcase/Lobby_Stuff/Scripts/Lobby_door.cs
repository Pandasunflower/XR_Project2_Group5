using UnityEngine;

public class Lobby_door : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("拖入場景中的 LobbySceneManager")]
    public LobbySceneManagerShowcase lobbySceneManager;
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (lobbySceneManager == null)
        {
            Debug.LogError("[Lobby_door] LobbySceneManager 未指定！請在 Inspector 拖入。");
            return;
        }

        if (hasBeenTriggered) return;
        hasBeenTriggered = true;

        Debug.Log("[Lobby_door] Player 進入門觸發區，啟動遊戲。");
        lobbySceneManager.RequestStartGame();
    }
}
