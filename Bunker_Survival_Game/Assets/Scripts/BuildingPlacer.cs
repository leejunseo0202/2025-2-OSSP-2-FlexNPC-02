using UnityEngine;
using UnityEngine.InputSystem;
public class BuildingPlacer : MonoBehaviour
{
    // 1. GridManager�� ������ ����
    private GridManager gridManager;

    // 2. [�ӽ�] �׽�Ʈ������ ���� �ǹ� ������
    public GameObject buildingPrefab_Test;

    // 3. ���� ��ġ ���� '��Ʈ' ������Ʈ
    private GameObject ghostBuilding;

    // 4. ���콺�� ��� �ٴ� ���̾�
    public LayerMask groundLayerMask;

    void Start()
    {
        // GridManager ������Ʈ�� �ڵ����� ã�ƿͼ� �����մϴ�.
        gridManager = GetComponent<GridManager>();
    }

    // [�߿�] �� �Լ��� ���߿� UI ��ư�� ȣ���� ���Դϴ�.
    // (������ �׽�Ʈ�� ���� Ű���� '1'Ű�� ȣ���غ��ô�)
    public void StartPlacingBuilding(GameObject prefabToPlace)
    {
        // �̹� ��ġ ���� ��Ʈ�� �ִٸ� ����
        if (ghostBuilding != null)
        {
            Destroy(ghostBuilding);
        }

        // ���������� '��Ʈ'�� �����մϴ�.
        ghostBuilding = Instantiate(prefabToPlace);

        // (��: ��Ʈ�� ������Ģ�̳� �ٸ� �Ϳ� �浹���� �ʵ���)
        Collider ghostCollider = ghostBuilding.GetComponent<Collider>();
        if (ghostCollider != null)
        {
            ghostCollider.enabled = false;
        }
    }

    void Update()
    {
        // --- �׽�Ʈ�� Ű���� �Է� (Input System �������� ����) ---
        // 'Alpha1' -> 'Digit1'�� ����
        if (Keyboard.current != null && Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            StartPlacingBuilding(buildingPrefab_Test);
        }
        // ----------------------------------------------------


        // '��Ʈ'�� �տ� ������� �� (��ġ ����� ��)�� ����
        if (ghostBuilding == null)
        {
            return;
        }

        // 1. ���콺 ��ġ�� ����ĳ����
        // (Input System�� ���콺 ��ġ�� ����ϴ� ���� �� ��Ȯ�մϴ�)
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayerMask))
        {
            // 2. ���콺�� ���� ���� ��ǥ�� �׸��� ��ǥ�� ��ȯ (GridManager �Լ� ���)
            Vector2Int gridPos = gridManager.WorldToGridPosition(hit.point);

            // 3. �׸��� ��ǥ�� �ٽ� ���� ��ǥ�� ��ȯ (ĭ�� �߾�, ����)
            Vector3 snappedWorldPos = gridManager.GridToWorldPosition(gridPos);

            // 4. '��Ʈ'�� ������ ��ġ�� �̵�
            ghostBuilding.transform.position = snappedWorldPos;
        }

        // (3�ܰ�: ��ȿ�� �˻� �� ���� ���� ������ ���⿡ �߰��� ����)

        // (4�ܰ�: ��ġ Ȯ�� ������ ���⿡ �߰��� ����)
    }
}