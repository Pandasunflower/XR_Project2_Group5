using UnityEngine;

public class AudienceMicController : MonoBehaviour
{
    public enum MicAxis { Forward, Up, Right }

    [Header("麥克風指向設定")]
    public MicAxis micHeadAxis = MicAxis.Up;
    public bool invertAxis = false;

    [Header("所有的觀眾區塊")]
    public GameObject[] audienceZones;

    void Update()
    {
        if (audienceZones == null) return;

        // 1. 取得麥克風頭部目前的向量
        Vector3 micHeadDir = GetMicHeadVector();
        Vector3 micPos = transform.position;

        // 2. 針對每一個觀眾區塊計算夾角
        foreach (GameObject zone in audienceZones)
        {
            if (zone != null)
            {
                // 計算從麥克風指向該觀眾區塊的向量
                Vector3 dirToZone = (zone.transform.position - micPos).normalized;
                
                // 計算麥克風頭部有沒有對準該區塊
                float angle = Vector3.Angle(micHeadDir, dirToZone);

                // 3. 將夾角傳給 Wwise (只影響該區塊的 GameObject)
                AkSoundEngine.SetRTPCValue("audience_pickup", angle, zone);
            }
        }
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