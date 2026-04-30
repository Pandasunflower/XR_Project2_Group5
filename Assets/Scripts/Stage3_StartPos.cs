using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Stage3_StartPos : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public FirestoreTest firestoreTest;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered start position trigger.");
            // firestoreTest.SetGameState("init");
            firestoreTest.SetGameState("l2");
            // firestoreTest.SetAllWaving();
            GetComponent<PlayWwiseEvent>().PlaySound();
            GetComponent<PlayWwiseEvent2>().PlaySound();
            // singingManager.realStartGame();
            videoPlayer.Play();
            // AkUnitySoundEngine.PostEvent("Play_OneGameOneDream_BGM", gameObject);
            // AkUnitySoundEngine.PostEvent("Play_OneGameOneDream_people", gameObject);
        }
        GetComponent<BoxCollider>().enabled = false; // Disable the trigger after the player enters to prevent multiple triggers
    }
}
