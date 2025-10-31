using UnityEngine;
using UnityEngine.InputSystem; // 1. Input System ����� ���� �߰�

public class CameraController : MonoBehaviour
{
    [Header("ī�޶� ����")]
    public float moveSpeed = 5.0f; // ī�޶� �̵� �ӵ�
    public float lookSensitivity = 0.1f; // ���콺 �ΰ���

    private float rotationX = 0f; // ī�޶��� ���� ȸ����

    void Start()
    {
        // 2. ������ ���۵Ǹ� ���콺 Ŀ���� ��װ� ����ϴ�.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Ű���峪 ���콺�� ����Ǿ� �ִ��� Ȯ��
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        // --- 3. ���콺�� ���� ȸ�� ---
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * lookSensitivity;

        // �¿� ȸ�� (Y�� ���� ȸ��)
        transform.Rotate(Vector3.up * mouseDelta.x);

        // ���� ȸ�� (X�� ���� ȸ��)
        rotationX -= mouseDelta.y;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // ���� �þ߰��� -90~+90���� ����

        // ī�޶��� localRotation�� ���� (Y�� ȸ���� ������ �����Ƿ� ����)
        transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y, 0);


        // --- 4. WASD�� ī�޶� �̵� ---
        Vector3 moveDirection = Vector3.zero;

        if (Keyboard.current[Key.W].isPressed)
        {
            moveDirection += transform.forward; // ��
        }
        if (Keyboard.current[Key.S].isPressed)
        {
            moveDirection -= transform.forward; // ��
        }
        if (Keyboard.current[Key.A].isPressed)
        {
            moveDirection -= transform.right; // ����
        }
        if (Keyboard.current[Key.D].isPressed)
        {
            moveDirection += transform.right; // ������
        }

        // (����) Q, E�� ���� �̵�
        if (Keyboard.current[Key.E].isPressed)
        {
            moveDirection += Vector3.up; // ��
        }
        if (Keyboard.current[Key.Q].isPressed)
        {
            moveDirection -= Vector3.up; // �Ʒ�
        }

        // �̵� ���͸� ����ȭ�ϰ�(�밢�� �̵��� �� ������ �ʰ�), �ӵ��� �ð��� ���� ����
        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);


        // --- 5. (����) Esc Ű�� ���콺 ��� ���� ---
        if (Keyboard.current[Key.Escape].wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}