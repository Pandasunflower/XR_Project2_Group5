using UnityEngine;

public class AudienceMicrophone : MonoBehaviour
{
    public enum MicAxis { Forward, Up, Right }

    [Header("1. 測量目標 (玩家 Camera)")]
    public Transform playerCamera;
    
    [Header("2. 發聲目標 (喇叭)")]
    public GameObject[] speakerOutputs;

    [Header("麥克風物理設定")]
    public MicAxis micHeadAxis = MicAxis.Up; 
    public bool invertAxis = false;
    public float mouthVerticalOffset = 0.15f;

    void Update()
    {
        if (playerCamera == null || speakerOutputs == null) return;

        // --- A. 定義位置 ---
        Vector3 mouthPos = playerCamera.position - (playerCamera.up * mouthVerticalOffset);
        Vector3 micPos = transform.position;

        // --- B. 計算距離 ---
        float distance = Vector3.Distance(micPos, mouthPos);

        // --- C. 雙向量角度計算 ---

        // 1. 麥克風指向夾角 (麥克風頭有沒有對準嘴巴)
        Vector3 micHeadDir = GetMicHeadVector();
        Vector3 dirMicToMouth = (mouthPos - micPos).normalized;
        float micAngle = Vector3.Angle(micHeadDir, dirMicToMouth);

        // 2. 嘴巴指向夾角 (玩家的臉有沒有對準麥克風)
        Vector3 mouthForward = playerCamera.forward; // 嘴巴朝向通常等於視角朝向
        Vector3 dirMouthToMic = (micPos - mouthPos).normalized;
        float mouthAngle = Vector3.Angle(mouthForward, dirMouthToMic);

        // 3. 融合角度 (這就是你要的「比較好」的邏輯)
        // 我們將兩個偏移角結合，只要其中一個歪掉，數值就會變大
        // 你也可以根據需求調整權重，例如臉轉開的影響比麥克風拿歪的影響更大
        float totalCombinedAngle = (micAngle + mouthAngle) * 0.5f; 

        // --- D. 傳送至 Wwise ---
        foreach (GameObject speaker in speakerOutputs)
        {
            if (speaker != null)
            {
                // 用這個融合後的角度來控制你的 LPF (紅線與藍線)
                AkSoundEngine.SetRTPCValue("mic_direct", totalCombinedAngle, speaker);
            }
        }

        // --- 視覺化除錯 ---
        Debug.DrawRay(micPos, micHeadDir * 0.3f, Color.blue);   // 麥克風指向
        Debug.DrawRay(mouthPos, mouthForward * 0.3f, Color.green); // 嘴巴指向
        Debug.DrawLine(micPos, mouthPos, Color.red);           // 兩者間的連線
    }

    private Vector3 GetMicHeadVector()
    {
        Vector3 dir = Vector3.up;
        switch (micHeadAxis) {
            case MicAxis.Forward: dir = transform.forward; break;
            case MicAxis.Up: dir = transform.up; break;
            case MicAxis.Right: dir = transform.right; break;
        }
        return invertAxis ? -dir : dir;
    }
}