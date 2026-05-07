using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

public class Stage3_StartPos : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public FirestoreTest firestoreTest;
    private PlayTutorStage2 TutorCS;
    private NPCSpawner NS;

    private bool hasBeenTriggered = false;
    void Start()
    {
        TutorCS = Object.FindAnyObjectByType<PlayTutorStage2>();
        NS = Object.FindAnyObjectByType<NPCSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return; // 如果已經觸發過，則不再執行
        
        hasBeenTriggered = true; // 標記為已觸發
        TutorCS.StartCoroutine(TutorCS.SpawnTutor());
        GetComponent<BoxCollider>().enabled = false;
    }

    public void RealStart()
    {
        // if (other.CompareTag("Player"))
        // {
        //     Debug.Log("Player entered start position trigger.");
        //     // firestoreTest.SetGameState("init");
        //     firestoreTest.SetGameState("l2");
        //     // firestoreTest.SetAllWaving();
        //     GetComponent<PlayWwiseEvent>().PlaySound();
        //     // singingManager.realStartGame();
        //     videoPlayer.Play();
        //     // AkUnitySoundEngine.PostEvent("Play_OneGameOneDream_BGM", gameObject);
        //     // AkUnitySoundEngine.PostEvent("Play_OneGameOneDream_people", gameObject);
        // }
        NS.StartGame();
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
        // GetComponent<BoxCollider>().enabled = false; // Disable the trigger after the player enters to prevent multiple triggers
    }
}
