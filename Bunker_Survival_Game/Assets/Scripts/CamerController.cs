using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // [�� ����] ī�޶� ��带 �����ϱ� ���� Enum(������)
    public enum CameraMode
    {
        FPS,        // 1��Ī
        QuarterView, // ���ͺ�
        TopDown     // ž��
    }

    [Header("���� ����")]
    public float moveSpeed = 5.0f;
    private CameraMode currentMode = CameraMode.FPS; // ���� ī�޶� ���

    [Header("1��Ī(FPS) ��� ����")]
    public float lookSensitivity = 0.1f;
    private float rotationX = 0f;

    [Header("ž��/���ͺ� ���� ��")]
    public Vector3 quarterViewRotation = new Vector3(45, 45, 0); // ���ͺ� ����
    public Vector3 topDownRotation = new Vector3(90, 0, 0);   // ž�� ����

    void Start()
    {
        // 7�� Ű�� ���� 1��Ī ���� ����
        SetMode_FPS();
    }

    void Update()
    {
        // Ű���峪 ���콺�� ����Ǿ� �ִ��� Ȯ��
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        // 1. ��� ��ȯ �Է� ����
        HandleModeSwitchInput();

        // 2. ���� ��忡 �´� �̵�/ȸ�� ó��
        HandleCameraMovement();

        // 3. (����) Esc Ű�� ���콺 ��� ����
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // [�� �Լ�] 7, 8, 9 Ű �Է��� ������ ��带 ����
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

    // [�� �Լ�] ���� ��忡 ���� �ٸ� �̵�/ȸ�� ������ ����
    void HandleCameraMovement()
    {
        switch (currentMode)
        {
            case CameraMode.FPS:
                HandleMovement_FPS();
                break;
            case CameraMode.QuarterView:
            case CameraMode.TopDown:
                HandleMovement_Pan(); // ���ͺ�� ž��� ������ '�д�' �̵� ���
                break;
        }
    }

    // --- 1��Ī(FPS) ��� ���� ---
    void SetMode_FPS()
    {
        currentMode = CameraMode.FPS;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // 1��Ī ��忡���� ī�޶� ȸ������ ���� ������ ����
        rotationX = transform.localEulerAngles.x;
    }

    // [���� Update ����] 1��Ī(FPS) ����� ���� �̵�/ȸ��
    void HandleMovement_FPS()
    {
        // ���콺�� ���� ȸ��
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * lookSensitivity;
        transform.Rotate(Vector3.up * mouseDelta.x);
        rotationX -= mouseDelta.y;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y, 0);

        // WASD�� ī�޶� ���� �̵�
        Vector3 moveDirection = Vector3.zero;
        if (Keyboard.current[Key.W].isPressed) moveDirection += transform.forward;
        if (Keyboard.current[Key.S].isPressed) moveDirection -= transform.forward;
        if (Keyboard.current[Key.A].isPressed) moveDirection -= transform.right;
        if (Keyboard.current[Key.D].isPressed) moveDirection += transform.right;
        if (Keyboard.current[Key.E].isPressed) moveDirection += Vector3.up;
        if (Keyboard.current[Key.Q].isPressed) moveDirection -= Vector3.up;

        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
    }


    // --- ���ͺ�/ž�� ��� ���� ---
    void SetMode_QuarterView()
    {
        currentMode = CameraMode.QuarterView;
        Cursor.lockState = CursorLockMode.None; // ���콺 Ŀ�� ���̱�
        Cursor.visible = true;
        transform.rotation = Quaternion.Euler(quarterViewRotation);
    }

    void SetMode_TopDown()
    {
        currentMode = CameraMode.TopDown;
        Cursor.lockState = CursorLockMode.None; // ���콺 Ŀ�� ���̱�
        Cursor.visible = true;
        transform.rotation = Quaternion.Euler(topDownRotation);
    }

    // [�� �Լ�] ���ͺ�/ž���� ���� '�д�(Panning)' �̵�
    void HandleMovement_Pan()
    {
        Vector3 moveDirection = Vector3.zero;

        // WASD �Է��� ���� X, Z�� �������� ��ȯ (���� �̵�)
        if (Keyboard.current[Key.W].isPressed) moveDirection += Vector3.forward; // Z+
        if (Keyboard.current[Key.S].isPressed) moveDirection -= Vector3.forward; // Z-
        if (Keyboard.current[Key.A].isPressed) moveDirection -= Vector3.right;   // X-
        if (Keyboard.current[Key.D].isPressed) moveDirection += Vector3.right;   // X+

        // Space.World �������� �̵�
        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
    }
}