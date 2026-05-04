using UnityEngine;

public class PlayWwiseEvent2 : MonoBehaviour
{
    [Header("👉 請從下拉選單選擇你要播放的音效")]
    public AK.Wwise.Event myWwiseEvent;

    [Header("2. 發聲目標 (陣列喇叭)")]
    public GameObject[] speakerOutputs;
    
    void Start()
    {
        // PlaySound();
    }

    public void PlaySound()
    {
        if (myWwiseEvent.IsValid())
        {
            if (speakerOutputs != null && speakerOutputs.Length > 0)
            {
                AkPositionArray positionArray = new AkPositionArray((uint)speakerOutputs.Length);

                foreach (GameObject speaker in speakerOutputs)
                {
                    if (speaker != null)
                    {
                        positionArray.Add(speaker.transform.position, speaker.transform.forward, speaker.transform.up);
                    }
                }

                // 🌟 終極大絕招：直接寫 (AkMultiPositionType)1 
                // 強制指定為多點發聲模式，徹底無視 Wwise 2024.1 的命名 Bug！
                AkUnitySoundEngine.SetMultiplePositions(
                    gameObject, 
                    positionArray, 
                    (ushort)positionArray.Count, 
                    (AkMultiPositionType)1 
                );
                
                positionArray.Dispose();
            }

            uint id = myWwiseEvent.Post(gameObject);
            
            
            Debug.Log($"成功播放音效：{myWwiseEvent.Name}，啟用了 {speakerOutputs.Length} 顆喇叭！");
        }
        else
        {
            Debug.LogWarning("⚠️ 你忘記在 Inspector 選擇 Wwise Event 囉！", gameObject);
        }
    }
}