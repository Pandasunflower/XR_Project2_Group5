using UnityEngine;
using System.Collections;

public class CleanWave : MonoBehaviour
{
    public GameObject crowdPrefab; 
    public int rows = 100;
    public int cols = 100;
    public float spacing = 1.5f;
    
    public float jumpDuration = 1.0f; // 動畫跳躍的總秒數
    public float jumpHeight = 1.5f;   // 騰空的高度

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
                animators[index].enabled = false; 
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
                StartCoroutine(PlayAnimDelayed(animators[i], transforms[i], "Jump", delay)); 
            }
        }
        
        yield return new WaitForSeconds(5f); 
        isWaving = false;
    }

    IEnumerator PlayAnimDelayed(Animator anim, Transform t, string stateName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (anim != null && t != null) 
        {
            anim.enabled = true; 
            anim.Play(stateName);
            
            // 強制 Y 軸物理抬升
            float halfTime = jumpDuration / 2f;
            Vector3 startPos = t.position;
            Vector3 peakPos = startPos + new Vector3(0, jumpHeight, 0);

            // 往上跳
            float timer = 0f;
            while(timer < halfTime)
            {
                if(t != null) t.position = Vector3.Lerp(startPos, peakPos, timer / halfTime);
                timer += Time.deltaTime;
                yield return null;
            }

            // 往下掉
            timer = 0f;
            while(timer < halfTime)
            {
                if(t != null) t.position = Vector3.Lerp(peakPos, startPos, timer / halfTime);
                timer += Time.deltaTime;
                yield return null;
            }

            // 確保完美落地並釋放 CPU
            if(t != null) t.position = startPos;
            if (anim != null) anim.enabled = false; 
        }
    }
}