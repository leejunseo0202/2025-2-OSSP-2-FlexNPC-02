using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPlacer : MonoBehaviour
{
    private GridManager gridManager;
    public GameObject buildingPrefab_Test;
    public LayerMask groundLayerMask;

    public Material validMaterial;
    public Material invalidMaterial;

    private GameObject ghostBuilding;
    private Renderer ghostRenderer;
    private Vector2Int currentGridPos;
    private bool isCurrentPosValid = false;

    void Start()
    {
        gridManager = GetComponent<GridManager>();
    }

    public void StartPlacingBuilding(GameObject prefabToPlace)
    {
        if (ghostBuilding != null)
        {
            Destroy(ghostBuilding);
        }

        ghostBuilding = Instantiate(prefabToPlace);

        ghostRenderer = ghostBuilding.GetComponentInChildren<Renderer>();

        // --- [����!] CS0104 ���� ���� ---
        // 'Debug.LogError' -> 'UnityEngine.Debug.LogError'�� ��Ȯ�ϰ� ����
        if (ghostRenderer == null)
        {
            UnityEngine.Debug.LogError("���� �����տ� Renderer ������Ʈ�� �����ϴ�!");
        }
        // --------------------------

        Collider ghostCollider = ghostBuilding.GetComponent<Collider>();
        if (ghostCollider != null)
        {
            ghostCollider.enabled = false;
        }
    }

    void Update()
    {
        // --- �׽�Ʈ�� '1'Ű �Է� ---
        if (Keyboard.current != null && Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            StartPlacingBuilding(buildingPrefab_Test);
        }

        // --- ��ġ ��� (���콺 ��Ŭ��) ---
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && ghostBuilding != null)
        {
            Destroy(ghostBuilding);
            ghostBuilding = null;
            ghostRenderer = null;
            return; // ��ġ ��� ����
        }

        if (ghostBuilding == null || Mouse.current == null)
        {
            return;
        }

        // --- 1. ����ĳ���� �� ���� ---
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayerMask))
        {
            currentGridPos = gridManager.WorldToGridPosition(hit.point);
            Vector3 snappedWorldPos = gridManager.GridToWorldPosition(currentGridPos);
            ghostBuilding.transform.position = snappedWorldPos;

            // --- 2. ��ȿ�� �˻� �� '�׸�/����' �ǵ�� ---
            isCurrentPosValid = gridManager.IsPlacementValid(currentGridPos);

            if (ghostRenderer != null)
            {
                ghostRenderer.material = isCurrentPosValid ? validMaterial : invalidMaterial;
            }

            // --- 3. ��ġ Ȯ�� (���콺 ��Ŭ��) ---
            if (isCurrentPosValid && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlaceBuilding();
            }
        }
    }

    void PlaceBuilding()
    {
        // 1. ���� �ǹ� ����
        Vector3 worldPos = gridManager.GridToWorldPosition(currentGridPos);
        Instantiate(buildingPrefab_Test, worldPos, Quaternion.identity);

        // 2. [�߿�] �׸��� �����͸� '������'���� ������Ʈ
        gridManager.SetGridOccupied(currentGridPos, true);

        // 3. ��Ʈ �ı� �� ��ġ ��� ����
        Destroy(ghostBuilding);
        ghostBuilding = null;
        ghostRenderer = null;
    }
}