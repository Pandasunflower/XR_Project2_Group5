using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Stage1_EndPos : MonoBehaviour
{
    public SceneTransition transitionManager;
    public int finalSongIndex; // 用於決定加載哪個場景
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered end position trigger.");
            transitionManager.goToSceneAsync(finalSongIndex);
        }
        GetComponent<BoxCollider>().enabled = false; // Disable the trigger after the player enters to prevent multiple triggers
    }
}
