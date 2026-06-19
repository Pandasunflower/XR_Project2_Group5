using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 掛在任意 GameObject 上。
/// 按下左或右控制器板機時，找出 objectsParent 底下距離控制器最近的子物件，
/// 播放其 VisualEffect 持續 vfxDuration 秒後停止。
/// 重複按下同一個 VFX 時，計時器重設。
/// </summary>
public class FireWorkController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("階層為 objects > Object1 > VFX1 的 objects 空物件")]
    public Transform objectsParent;

    [Tooltip("OVRCameraRig 的 trackingSpace（可不填，自動尋找）")]
    public Transform trackingSpace;

    [Header("Settings")]
    [Tooltip("VFX 持續秒數")]
    public float vfxDuration = 3f;

    private readonly Dictionary<VisualEffect, Coroutine> _activeCoroutines =
        new Dictionary<VisualEffect, Coroutine>();

    private void Start()
    {
        if (trackingSpace == null)
        {
            var rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null)
                trackingSpace = rig.trackingSpace;
            else
                Debug.LogWarning("[Stage3InputHandler] 找不到 OVRCameraRig，控制器位置可能不準確。");
        }

        StopAllVFX();
    }

    private void StopAllVFX()
    {
        if (objectsParent == null) return;

        foreach (Transform child in objectsParent)
        {
            var vfx = child.GetComponentInChildren<VisualEffect>();
            if (vfx != null) vfx.Stop();
        }

        Debug.Log("[Stage3InputHandler] 所有 VFX 已初始化為停止狀態。");
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) || Input.GetKeyDown(KeyCode.Space))
            TriggerNearestVFX(OVRInput.Controller.LTouch);

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.Space))
            TriggerNearestVFX(OVRInput.Controller.RTouch);
    }

    private void TriggerNearestVFX(OVRInput.Controller controller)
    {
        if (objectsParent == null)
        {
            Debug.LogWarning("[Stage3InputHandler] objectsParent 未指定！");
            return;
        }

        Ray ray = GetControllerRay(controller);

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (Transform child in objectsParent)
        {
            // 計算物件位置到射線的垂直距離
            Vector3 toChild = child.position - ray.origin;
            float alongRay = Vector3.Dot(toChild, ray.direction);
            Vector3 closestPoint = ray.origin + ray.direction * Mathf.Max(0f, alongRay);
            float dist = Vector3.Distance(child.position, closestPoint);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = child;
            }
        }

        if (nearest == null) return;

        VisualEffect vfx = nearest.GetComponentInChildren<VisualEffect>();
        if (vfx == null)
        {
            Debug.LogWarning($"[Stage3InputHandler] {nearest.name} 底下找不到 VisualEffect！");
            return;
        }

        // 若同一個 VFX 已在播放，重設計時器
        if (_activeCoroutines.TryGetValue(vfx, out Coroutine existing) && existing != null)
            StopCoroutine(existing);

        _activeCoroutines[vfx] = StartCoroutine(PlayVFXForDuration(vfx));
        Debug.Log($"[Stage3InputHandler] 觸發 {vfx.name}（{nearest.name}），持續 {vfxDuration} 秒");
    }

    private Ray GetControllerRay(OVRInput.Controller controller)
    {
        Vector3 localPos = OVRInput.GetLocalControllerPosition(controller);
        Quaternion localRot = OVRInput.GetLocalControllerRotation(controller);

        Vector3 worldPos;
        Vector3 worldForward;

        if (trackingSpace != null)
        {
            worldPos = trackingSpace.TransformPoint(localPos);
            worldForward = trackingSpace.TransformDirection(localRot * Vector3.forward);
        }
        else
        {
            worldPos = localPos;
            worldForward = localRot * Vector3.forward;
        }

        return new Ray(worldPos, worldForward);
    }

    private IEnumerator PlayVFXForDuration(VisualEffect vfx)
    {
        vfx.Play();
        yield return new WaitForSeconds(vfxDuration);
        vfx.Stop();
    }
}
