using UnityEngine;
using System.Collections.Generic;
using System;

public class Stage2EventController : MonoBehaviour
{
    private bool isGameRunning = false;
    private float elapsedTime = 0f;
    private float periodicTimer = 0f;

    [Header("設定")]
    public float periodicInterval = 30f;

    public NPCSpawner npcSpawner;

    // 儲存特殊事件的時間點（秒）與對應的 Action
    private Dictionary<float, Action> specialEvents = new Dictionary<float, Action>();
    private List<float> sortedEventTimes = new List<float>();

    void Update()
    {
        if (!isGameRunning) return;

        elapsedTime += Time.deltaTime;
        periodicTimer += Time.deltaTime;

        if (periodicTimer >= periodicInterval)
        {
            TriggerPeriodicEvent();
            periodicTimer = 0f; 
        }

        CheckSpecialEvents();
    }

    public void StartGame()
    {
        isGameRunning = true;
        elapsedTime = 0f;
        periodicTimer = 0f;
        Debug.Log("遊戲開始，計時器啟動。");
    }

    public void RegisterSpecialEvent(float timeInSeconds, Action callback)
    {
        if (!specialEvents.ContainsKey(timeInSeconds))
        {
            specialEvents.Add(timeInSeconds, callback);
            sortedEventTimes.Add(timeInSeconds);
            sortedEventTimes.Sort();
        }
    }

    private void TriggerPeriodicEvent()
    {
        // Debug.Log($"[循環事件] 已過 {Mathf.FloorToInt(elapsedTime)} 秒，觸發每 {periodicInterval} 秒一次的事件。");
        npcSpawner.RandomizeNPCAnimations();
    }

    private void CheckSpecialEvents()
    {
        for (int i = 0; i < sortedEventTimes.Count; i++)
        {
            float eventTime = sortedEventTimes[i];

            if (elapsedTime >= eventTime)
            {
                Debug.Log($"[特殊事件] 時間點 {eventTime}s 到達，觸發事件！");
                specialEvents[eventTime]?.Invoke();

                specialEvents.Remove(eventTime);
                sortedEventTimes.RemoveAt(i);
                i--;
            }
            else
            {
                break;
            }
        }
    }
}