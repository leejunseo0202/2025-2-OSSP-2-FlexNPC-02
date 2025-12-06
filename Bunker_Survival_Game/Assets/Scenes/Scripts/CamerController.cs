using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // [새 변수] 카메라 모드를 구분하기 위한 Enum(열거형)
    public enum CameraMode
    {
        FPS,        // 1인칭
        QuarterView, // 쿼터뷰
        TopDown     // 탑뷰
    }

    [Header("공통 설정")]
    public float moveSpeed = 5.0f;
    private CameraMode currentMode = CameraMode.FPS; // 현재 카메라 모드

    [Header("1인칭(FPS) 모드 설정")]
    public float lookSensitivity = 0.1f;
    private float rotationX = 0f;

    [Header("탑뷰/쿼터뷰 고정 값")]
    public Vector3 quarterViewRotation = new Vector3(45, 45, 0); // 쿼터뷰 각도
    public Vector3 topDownRotation = new Vector3(90, 0, 0);   // 탑뷰 각도

    void Start()
    {
        // 7번 키를 눌러 1인칭 모드로 시작
        SetMode_FPS();
    }

    void Update()
    {
        // 키보드나 마우스가 연결되어 있는지 확인
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        // 1. 모드 전환 입력 감지
        HandleModeSwitchInput();

        // 2. 현재 모드에 맞는 이동/회전 처리
        HandleCameraMovement();

        // 3. (선택) Esc 키로 마우스 잠금 해제
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // [새 함수] 7, 8, 9 키 입력을 감지해 모드를 변경
    void HandleModeSwitchInput()
    {
        if (Keyboard.current[Key.Digit7].wasPressedThisFrame)
        {
            SetMode_FPS();
        }
        else if (Keyboard.current[Key.Digit8].wasPressedThisFrame)
        {
            SetMode_QuarterView();
        }
        else if (Keyboard.current[Key.Digit9].wasPressedThisFrame)
        {
            SetMode_TopDown();
        }
    }

    // [새 함수] 현재 모드에 따라 다른 이동/회전 로직을 실행
    void HandleCameraMovement()
    {
        switch (currentMode)
        {
            case CameraMode.FPS:
                HandleMovement_FPS();
                break;
            case CameraMode.QuarterView:
            case CameraMode.TopDown:
                HandleMovement_Pan(); // 쿼터뷰와 탑뷰는 동일한 '패닝' 이동 사용
                break;
        }
    }

    // --- 1인칭(FPS) 모드 설정 ---
    void SetMode_FPS()
    {
        currentMode = CameraMode.FPS;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // 1인칭 모드에서는 카메라 회전값을 현재 값으로 유지
        rotationX = transform.localEulerAngles.x;
    }

    // [기존 Update 로직] 1인칭(FPS) 모드일 때의 이동/회전
    void HandleMovement_FPS()
    {
        // 마우스로 시점 회전
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * lookSensitivity;
        transform.Rotate(Vector3.up * mouseDelta.x);
        rotationX -= mouseDelta.y;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y, 0);

        // WASD로 카메라 기준 이동
        Vector3 moveDirection = Vector3.zero;
        if (Keyboard.current[Key.W].isPressed) moveDirection += transform.forward;
        if (Keyboard.current[Key.S].isPressed) moveDirection -= transform.forward;
        if (Keyboard.current[Key.A].isPressed) moveDirection -= transform.right;
        if (Keyboard.current[Key.D].isPressed) moveDirection += transform.right;
        if (Keyboard.current[Key.E].isPressed) moveDirection += Vector3.up;
        if (Keyboard.current[Key.Q].isPressed) moveDirection -= Vector3.up;

        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
    }


    // --- 쿼터뷰/탑뷰 모드 설정 ---
    void SetMode_QuarterView()
    {
        currentMode = CameraMode.QuarterView;
        Cursor.lockState = CursorLockMode.None; // 마우스 커서 보이기
        Cursor.visible = true;
        transform.rotation = Quaternion.Euler(quarterViewRotation);
    }

    void SetMode_TopDown()
    {
        currentMode = CameraMode.TopDown;
        Cursor.lockState = CursorLockMode.None; // 마우스 커서 보이기
        Cursor.visible = true;
        transform.rotation = Quaternion.Euler(topDownRotation);
    }

    // [새 함수] 쿼터뷰/탑뷰일 때의 '패닝(Panning)' 이동
    void HandleMovement_Pan()
    {
        Vector3 moveDirection = Vector3.zero;

        // WASD 입력을 월드 X, Z축 기준으로 변환 (수평 이동)
        if (Keyboard.current[Key.W].isPressed) moveDirection += Vector3.forward; // Z+
        if (Keyboard.current[Key.S].isPressed) moveDirection -= Vector3.forward; // Z-
        if (Keyboard.current[Key.A].isPressed) moveDirection -= Vector3.right;   // X-
        if (Keyboard.current[Key.D].isPressed) moveDirection += Vector3.right;   // X+

        // Space.World 기준으로 이동
        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
    }
}