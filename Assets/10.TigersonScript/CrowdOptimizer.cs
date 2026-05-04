using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdOptimizer : MonoBehaviour
{
    [Header("請把你的 VR 攝影機 (Main Camera) 拖進來")]
    public Transform playerCamera;

    private List<Animator> allAnimators = new List<Animator>();
    private float[] timers;
    private float[] updateIntervals;

    void Start()
    {
        // 自動去場景裡找出「所有」的 Animator
        Animator[] foundAnimators = FindObjectsOfType<Animator>();
        
        foreach (Animator anim in foundAnimators)
        {
            // 直接關掉它們的自動播放，由我們手動接管
            anim.enabled = false; 
            allAnimators.Add(anim);
        }

        // 準備計時器陣列
        timers = new float[allAnimators.Count];
        updateIntervals = new float[allAnimators.Count];

        // 如果你忘了拖攝影機，幫你自動找 MainCamera
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        if (playerCamera == null || allAnimators.Count == 0) return;

        // 每一幀都去檢查所有 NPC 跟你的距離
        for (int i = 0; i < allAnimators.Count; i++)
        {
            Animator anim = allAnimators[i];
            if (anim == null) continue;

            // 計算 NPC 到攝影機的距離
            float distSqr = (playerCamera.position - anim.transform.position).sqrMagnitude;

            // 根據距離決定動畫更新的頻率 (越遠越卡，越省效能)
            if (distSqr < 100)      // 距離 10 以內
                updateIntervals[i] = 0f;
            else if (distSqr < 900) // 距離 30 以內
                updateIntervals[i] = 0.05f;
            else if (distSqr < 3600)// 距離 60 以內
                updateIntervals[i] = 0.1f;
            else                    // 距離 60 以外
                updateIntervals[i] = 0.2f;

            // 真正執行推動動畫的邏輯
            if (updateIntervals[i] == 0f)
            {
                anim.Update(Time.deltaTime); // 順暢播放
            }
            else
            {
                timers[i] += Time.deltaTime;
                if (timers[i] >= updateIntervals[i])
                {
                    anim.Update(updateIntervals[i]); // 降幀播放
                    timers[i] -= updateIntervals[i];
                }
            }
        }
    }
}
