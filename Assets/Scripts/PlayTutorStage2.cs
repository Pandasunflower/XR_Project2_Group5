using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
public class PlayTutorStage2 : MonoBehaviour
{
    public GameObject TutorPrefabs;

    public AK.Wwise.Event SpawnEvent;
    private GameObject TutorGuy;
    public GameObject TutorPosition;
    public GameObject hitEffectPrefab;
    public GameObject SHOOTHIM;
    private Stage3_StartPos StartController;

    private bool gotShot = false;

    // void awake()
    // {
        
    // }

    public IEnumerator SpawnTutor()
    {
        Vector3 customVector = new Vector3(TutorPosition.transform.position.x, -0.081f, TutorPosition.transform.position.z);
        GameObject effect = null;
        SpawnEvent.Post(gameObject);
        
        // customVector.z += 0.5f; 
        if (hitEffectPrefab != null)
        {
            effect = Instantiate(hitEffectPrefab, customVector,  Quaternion.Euler(0, 0, 0));
        }
        // if (effect != null)
        // {
        //     Destroy(effect, 1f);
        // }
        yield return new WaitForSeconds(0.3f);
        // customVector.z -= 0.5f; 
        if (TutorPrefabs != null && TutorPosition != null)
        {
            TutorGuy = Instantiate(TutorPrefabs, customVector, Quaternion.Euler(0, 180f, 0));
        }
        yield return null;
    }

    public IEnumerator TutorGotShot()
    {
        if (TutorGuy != null)
        {
            Animator animator = TutorGuy.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Spin");
            }
            else
            {
                Debug.LogError("Tutor prefab does not have an Animator component.");
            }
        }
        else
        {
            Debug.LogError("No GameObject with tag 'Tutor' found in the scene.");
        }
        yield return new WaitForSeconds(1.5f);
        AkSoundEngine.StopAll(gameObject); 
        DestroyTutor();
    }

    public void DestroyTutor()
    {
        Vector3 customVector = new Vector3(TutorPosition.transform.position.x, -0.081f, TutorPosition.transform.position.z);
        GameObject effect = null;   
        if (hitEffectPrefab != null)
        {
            effect = Instantiate(hitEffectPrefab, customVector,  Quaternion.Euler(0, 0, 0));
        }
        if (TutorGuy != null)
        {
            Destroy(TutorGuy);
        }
        if (SHOOTHIM != null)
        {
            Destroy(SHOOTHIM);
        }
        else
        {
            Debug.LogError("No GameObject with tag 'Tutor' found in the scene.");
        }
        StartController = Object.FindAnyObjectByType<Stage3_StartPos>();
        
        if (gotShot) return; // 如果已經被射擊過，則不再執行
        gotShot = true; // 標記為已被射擊
        StartController.RealStart();
    }
}
