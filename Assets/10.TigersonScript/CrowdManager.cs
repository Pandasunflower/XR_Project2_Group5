using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    public List<CrowdAgent> agents;
    public Transform player;

    int batchSize = 200;
    int currentIndex = 0;

    void Start()
    {
        // 關掉所有 Animator 自動更新
        foreach (var agent in agents)
        {
            agent.animator.enabled = false;
        }
    }

    void Update()
    {
        int count = agents.Count;

        for (int i = 0; i < batchSize; i++)
        {
            if (count == 0) return;

            currentIndex %= count;
            var agent = agents[currentIndex];

            UpdateAgent(agent);

            currentIndex++;
        }
    }

    void UpdateAgent(CrowdAgent agent)
    {
        float distSqr = (player.position - agent.transform.position).sqrMagnitude;

        // 決定更新頻率
        if (distSqr < 10 * 10)
            agent.updateInterval = 0f;
        else if (distSqr < 30 * 30)
            agent.updateInterval = 0.05f;
        else if (distSqr < 60 * 60)
            agent.updateInterval = 0.1f;
        else
            agent.updateInterval = 0.2f;

        // 真正更新動畫
        if (agent.updateInterval == 0f)
        {
            // 每幀更新（近距離）
            agent.animator.Update(Time.deltaTime);
        }
        else
        {
            agent.timer += Time.deltaTime;

            if (agent.timer >= agent.updateInterval)
            {
                agent.animator.Update(agent.updateInterval);
                agent.timer -= agent.updateInterval;
            }
        }
    }
}
