using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

public class Stage3_Starts : MonoBehaviour
{
    public FirestoreTest firestoreTest;

    private bool hasBeenTriggered = false;

    private Stage3CamVideoControl Stage3CamVideoControlCS;
    void Start()
    {
        Stage3CamVideoControlCS = Object.FindAnyObjectByType<Stage3CamVideoControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return; // 如果已經觸發過，則不再執行
        Debug.Log(gameObject.tag + " trigger entered by object: " + other.name);
        if (!other.CompareTag("Player"))
        {
            return;
        }

        hasBeenTriggered = true; // 標記為已觸發
        Stage3CamVideoControlCS.StartGame();
        GetComponent<PlayWwiseEvent>().PlaySound();
        GetComponent<PlayWwiseEvent2>().PlaySound();
        GetComponent<PlayWwiseEvent3>().PlaySound();
        GetComponent<BoxCollider>().enabled = false;
    }
}
