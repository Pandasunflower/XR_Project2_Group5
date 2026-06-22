using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
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

    [Header("Sound Settings")]
    public AK.Wwise.Event fireworkSound;
    public float soundDelay = 0.2f;

    [Header("Charge & Launch All Settings")]
    [Tooltip("集氣圓圈的 Image（Image Type = Filled / Radial 360），會控制顯示/隱藏與 fillAmount")]
    public Image chargeCircleImage;
    [Tooltip("雙手板機需要持續按住超過這個秒數才會開始集氣")]
    public float chargeHoldThreshold = 0.2f;
    [Tooltip("集氣需要的秒數，集滿後會發射所有煙火")]
    public float chargeDuration = 0.5f;
    [Tooltip("全部發射後的冷卻時間（秒），冷卻中無法再次集氣")]
    public float allFireCooldown = 5f;

    private float _bothHeldTimer = 0f;
    private float _chargeTimer = 0f;
    private bool _isCharging = false;
    private float _cooldownRemaining = 0f;

    // 集氣完成（全部煙火發射）時設為 true，放開對應手的板機時消費掉，跳過該手的一般煙火發射
    private bool _suppressLeftRelease = false;
    private bool _suppressRightRelease = false;

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
        HideChargeCircle();
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
        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) || Input.GetKeyUp(KeyCode.Space))
        {
            if (_suppressLeftRelease)
                _suppressLeftRelease = false; // 這次放開是因為集氣發射完成，跳過一般煙火
            else
                TriggerNearestVFX(OVRInput.Controller.LTouch);
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || Input.GetKeyUp(KeyCode.Space))
        {
            if (_suppressRightRelease)
                _suppressRightRelease = false;
            else
                TriggerNearestVFX(OVRInput.Controller.RTouch);
        }

        UpdateChargeAndLaunchAll();
    }

    // ──────────────────────────────────────────────────────────
    //  雙手板機集氣 → 集滿後全部煙火一起發射 → 冷卻
    // ──────────────────────────────────────────────────────────
    private void UpdateChargeAndLaunchAll()
    {
        // 冷卻中：不接受集氣輸入
        if (_cooldownRemaining > 0f)
        {
            _cooldownRemaining -= Time.deltaTime;
            return;
        }

        bool leftHeld  = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        bool rightHeld = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        bool bothHeld  = leftHeld && rightHeld;

        if (bothHeld)
        {
            _bothHeldTimer += Time.deltaTime;

            if (!_isCharging && _bothHeldTimer >= chargeHoldThreshold)
                StartCharging();

            if (_isCharging)
            {
                _chargeTimer += Time.deltaTime;

                if (chargeCircleImage != null)
                    chargeCircleImage.fillAmount = Mathf.Clamp01(_chargeTimer / chargeDuration);

                if (_chargeTimer >= chargeDuration)
                    LaunchAllFireworks();
            }
        }
        else
        {
            // 鬆手就取消集氣
            if (_isCharging)
                CancelCharging();

            _bothHeldTimer = 0f;
        }
    }

    private void StartCharging()
    {
        _isCharging = true;
        _chargeTimer = 0f;
        ShowChargeCircle();

        Debug.Log("[FireWorkController] 開始集氣");
    }

    private void CancelCharging()
    {
        _isCharging = false;
        _chargeTimer = 0f;
        _bothHeldTimer = 0f;
        HideChargeCircle();

        Debug.Log("[FireWorkController] 集氣取消");
    }

    private void LaunchAllFireworks()
    {
        _isCharging = false;
        _chargeTimer = 0f;
        _bothHeldTimer = 0f;
        _cooldownRemaining = allFireCooldown;
        HideChargeCircle();

        // 兩手目前都還按著（集氣的前提就是雙手按住），放開時跳過一般煙火發射
        _suppressLeftRelease = true;
        _suppressRightRelease = true;

        if (objectsParent == null)
        {
            Debug.LogWarning("[FireWorkController] objectsParent 未指定，無法發射全部煙火！");
            return;
        }

        foreach (Transform child in objectsParent)
        {
            VisualEffect vfx = child.GetComponentInChildren<VisualEffect>();
            if (vfx == null) continue;

            if (_activeCoroutines.TryGetValue(vfx, out Coroutine existing) && existing != null)
                StopCoroutine(existing);

            _activeCoroutines[vfx] = StartCoroutine(PlayVFXForDuration(vfx));
        }

        Debug.Log($"[FireWorkController] 集氣完成！全部煙火發射，{allFireCooldown} 秒冷卻開始。");
    }

    private void ShowChargeCircle()
    {
        if (chargeCircleImage == null) return;
        chargeCircleImage.gameObject.SetActive(true);
        chargeCircleImage.fillAmount = 0f;
    }

    private void HideChargeCircle()
    {
        if (chargeCircleImage == null) return;
        chargeCircleImage.gameObject.SetActive(false);
        chargeCircleImage.fillAmount = 0f;
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
        yield return new WaitForSeconds(soundDelay);
        fireworkSound.Post(vfx.gameObject);
        yield return new WaitForSeconds(vfxDuration);
        vfx.Stop();
        AkUnitySoundEngine.StopAll(vfx.gameObject);
    }
}
