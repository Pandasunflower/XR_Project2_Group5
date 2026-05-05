using UnityEngine;
using System.Collections;

public class CleanWave : MonoBehaviour
{
    public GameObject crowdPrefab; // 直接放你的 Elsa Prefab
    public int rows = 100;
    public int cols = 100;
    public float spacing = 1.5f;

    private Animator[] animators;
    private Transform[] transforms;
    private bool isWaving = false;

    void Start()
    {
        int total = rows * cols;
        animators = new Animator[total];
        transforms = new Transform[total];

        int index = 0;
        for (int x = 0; x < cols; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 pos = new Vector3(x * spacing, 0, z * spacing);
                GameObject go = Instantiate(crowdPrefab, pos, Quaternion.identity);
                animators[index] = go.GetComponent<Animator>();
                transforms[index] = go.transform;
                index++;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isWaving)
        {
            StartCoroutine(DoWave());
        }
    }

    IEnumerator DoWave()
    {
        isWaving = true;
        
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                float delay = Vector3.Distance(transforms[i].position, Vector3.zero) * 0.05f;
                StartCoroutine(PlayAnimDelayed(animators[i], "Jump", delay)); // "Jump" 請換成實際動畫名
            }
        }
        
        yield return new WaitForSeconds(5f); // 5秒後波浪結束，可再次按空白鍵
        isWaving = false;
    }

    IEnumerator PlayAnimDelayed(Animator anim, string stateName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (anim != null) anim.Play(stateName);
    }
}