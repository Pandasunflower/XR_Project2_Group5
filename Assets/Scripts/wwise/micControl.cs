using UnityEngine;
using UnityEngine.InputSystem; // 使用新版 Input System

public class MicTestController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;

    void Update()
    {
        // --- 1. 平移控制 (WASD + QE) ---
        Vector3 moveDir = Vector3.zero;

        // 讀取鍵盤輸入
        if (Keyboard.current.wKey.isPressed) moveDir += transform.forward;
        if (Keyboard.current.sKey.isPressed) moveDir -= transform.forward;
        if (Keyboard.current.aKey.isPressed) moveDir -= transform.right;
        if (Keyboard.current.dKey.isPressed) moveDir += transform.right;
        if (Keyboard.current.qKey.isPressed) moveDir += Vector3.up;
        if (Keyboard.current.eKey.isPressed) moveDir -= Vector3.up;

        // 執行移動
        transform.position += moveDir * moveSpeed * Time.deltaTime;


        // --- 2. 旋轉控制 (方向鍵) ---
        // 用來測試麥克風「頭部」指向的角度變化
        Vector3 rotateDir = Vector3.zero;

        if (Keyboard.current.upArrowKey.isPressed) rotateDir.x = -1f;
        if (Keyboard.current.downArrowKey.isPressed) rotateDir.x = 1f;
        if (Keyboard.current.leftArrowKey.isPressed) rotateDir.y = -1f;
        if (Keyboard.current.rightArrowKey.isPressed) rotateDir.y = 1f;

        // 執行旋轉
        transform.Rotate(rotateDir * rotateSpeed * Time.deltaTime, Space.Self);
    }
}