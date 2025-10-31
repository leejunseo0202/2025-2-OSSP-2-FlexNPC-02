using UnityEngine;
using UnityEngine.InputSystem; // 1. Input System 사용을 위해 추가

public class CameraController : MonoBehaviour
{
    [Header("카메라 설정")]
    public float moveSpeed = 5.0f; // 카메라 이동 속도
    public float lookSensitivity = 0.1f; // 마우스 민감도

    private float rotationX = 0f; // 카메라의 상하 회전값

    void Start()
    {
        // 2. 게임이 시작되면 마우스 커서를 잠그고 숨깁니다.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 키보드나 마우스가 연결되어 있는지 확인
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        // --- 3. 마우스로 시점 회전 ---
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * lookSensitivity;

        // 좌우 회전 (Y축 기준 회전)
        transform.Rotate(Vector3.up * mouseDelta.x);

        // 상하 회전 (X축 기준 회전)
        rotationX -= mouseDelta.y;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // 상하 시야각을 -90~+90도로 제한

        // 카메라의 localRotation을 적용 (Y축 회전은 위에서 했으므로 유지)
        transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y, 0);


        // --- 4. WASD로 카메라 이동 ---
        Vector3 moveDirection = Vector3.zero;

        if (Keyboard.current[Key.W].isPressed)
        {
            moveDirection += transform.forward; // 앞
        }
        if (Keyboard.current[Key.S].isPressed)
        {
            moveDirection -= transform.forward; // 뒤
        }
        if (Keyboard.current[Key.A].isPressed)
        {
            moveDirection -= transform.right; // 왼쪽
        }
        if (Keyboard.current[Key.D].isPressed)
        {
            moveDirection += transform.right; // 오른쪽
        }

        // (선택) Q, E로 수직 이동
        if (Keyboard.current[Key.E].isPressed)
        {
            moveDirection += Vector3.up; // 위
        }
        if (Keyboard.current[Key.Q].isPressed)
        {
            moveDirection -= Vector3.up; // 아래
        }

        // 이동 벡터를 정규화하고(대각선 이동이 더 빠르지 않게), 속도와 시간을 곱해 적용
        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);


        // --- 5. (선택) Esc 키로 마우스 잠금 해제 ---
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}