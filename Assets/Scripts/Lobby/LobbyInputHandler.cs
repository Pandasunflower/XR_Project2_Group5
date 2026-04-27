using UnityEngine;

public class LobbyInputHandler : MonoBehaviour
{
    [Header("Core References")]
    public LobbySongManager songManager;
    public LobbySceneManager sceneManager;

    [Header("Input Settings")]
    public float inputCooldown = 0.2f;
    private float _lastInputTime;

    void Update()
    {
        if (Time.time - _lastInputTime < inputCooldown) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            songManager.PreviousSong();
            _lastInputTime = Time.time;
            songManager.UpdateUI();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            songManager.NextSong();
            _lastInputTime = Time.time;
            songManager.UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            sceneManager.RequestStartGame();
            _lastInputTime = Time.time;
        }
    }
}