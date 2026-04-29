using UnityEngine;

public class PlayWwiseEvent2 : MonoBehaviour
{
    [Header("👉 請從下拉選單選擇你要播放的音效")]
    public AK.Wwise.Event myWwiseEvent;
    
    void Start()
    {
        
        // PlaySound();
    }

    public void PlaySound()
    {
        // 先檢查你到底有沒有在介面上選擇音效
        if (myWwiseEvent.IsValid())
        {
            // 這會直接讀取你選的 Event
            myWwiseEvent.Post(gameObject);
            Debug.Log("成功播放音效：" + myWwiseEvent.Name);
        }
        else
        {
            Debug.LogWarning("⚠️ 你忘記在 Inspector 選擇 Wwise Event 囉！", gameObject);
        }
    }
}