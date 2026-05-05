using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider))]
public class TutorController : MonoBehaviour
{
    private PlayTutorStage2 TutorCS;
    private Animator _animator;
    private bool isDead = false;
    private void Awake()
    {
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.isTrigger = true;
        }
        _animator = GetComponent<Animator>();
        TutorCS = Object.FindAnyObjectByType<PlayTutorStage2>();
    }

    void Update()
    {
        Vector3 targetPosition = Camera.main.transform.position;
        
        targetPosition.y = transform.position.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        Debug.Log($"{gameObject.name} Tag: {gameObject.tag}");
        if (other.CompareTag("bullet"))
        {
            Debug.Log($"{gameObject.name} 被射中了！");
            TutorCS.StartCoroutine(TutorCS.TutorGotShot());
        }
    }
}
