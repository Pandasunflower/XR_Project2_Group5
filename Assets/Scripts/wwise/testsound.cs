using UnityEngine;

public class PlayWwiseEvent : MonoBehaviour
{
    [Header("👉 請從下拉選單選擇你要播放的音效")]
    public AK.Wwise.Event myWwiseEvent;

    [Header("👉 測試用的觸發按鍵 (預設為空白鍵)")]
    public KeyCode testKey = KeyCode.Space;

    // 1. 給你測試用的：按鍵盤觸發
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            PlaySound();
        }
    }

    // 2. 給你實戰用的：讓其他程式、按鈕或 XR 互動套件來呼叫
    public void PlaySound()
    {
        // 先檢查你到底有沒有在介面上選擇音效
        if (myWwiseEvent.IsValid())
        {
            // 用新寫法播放，這會直接讀取你選的 Event
            myWwiseEvent.Post(gameObject);
            Debug.Log("成功播放音效：" + myWwiseEvent.Name);
        }
        else
        {
            // 如果你忘記選了，Unity 會跳出黃色警告提醒你
            Debug.LogWarning("⚠️ 你忘記在 Inspector 選擇 Wwise Event 囉！", gameObject);
        }
    }
}