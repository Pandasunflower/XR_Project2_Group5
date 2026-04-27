using UnityEngine;
// 必須引入這個命名空間才能使用新版輸入系統
using UnityEngine.InputSystem; 

public class CameraControl : MonoBehaviour
{
    [Header("視角靈敏度")]
    // 新版 Input System 讀取到的數值通常比較大，所以靈敏度預設調小一點
    public float mouseSensitivity = 10f; 

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 確保滑鼠存在且有連線
        if (Mouse.current != null)
        {
            // 使用新版 Input System 讀取滑鼠的移動量 (Delta)
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
            float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            yRotation += mouseX;

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}