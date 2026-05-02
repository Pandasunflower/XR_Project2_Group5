using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationInstancing; // 確保有這行

public class PlayInstancing : MonoBehaviour
{
    public AnimationInstancing.AnimationInstancing targetNPC;

    void Start()
    {
        // 1. 先確認這個物件有被喚醒
        if (targetNPC != null)
        {
            // 2. 執行播放 (名稱一定要對，例如 "Boxing" 或 "Dance")
            targetNPC.PlayAnimation("AmeiFanMoveA"); 
        }
    }
}