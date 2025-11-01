using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildingPlacer : MonoBehaviour
{
    private GridManager gridManager;
    public LayerMask groundLayerMask;

    [Header("������")]
    public List<GameObject> buildingPrefabs;
    public GameObject gridMarkerPrefab;

    [Header("�ǵ�� ����")]
    public Material validMaterial;

    // --- Private ���� ���� ---
    private GameObject ghostBuilding;
    private Renderer ghostRenderer;
    private GameObject currentPrefabToBuild;
    private float currentBuildingHeight;
    private Vector2Int currentGridPos;
    private Vector2Int currentBuildingSize;
    private Vector3 currentSnappedWorldPos; // [�� ����] ������ ���� ��ġ ����
    private bool isCurrentPlacementValid = false; // [�� ����] ���� ��ġ ���� ���� ����

    private List<GameObject> gridMarkers = new List<GameObject>();

    void Start()
    {
        gridManager = GetComponent<GridManager>();
    }

    /// <summary>
    /// �� ������ ȣ��Ǹ�, �Է°� ��Ʈ ��ġ�� �����մϴ�.
    /// </summary>
    void Update()
    {
        HandleBuildingSelectionInput(); // F1-F6 Ű �Է� ó��

        if (ghostBuilding == null) // ��ġ ��尡 �ƴϸ� �ߴ�
        {
            return;
        }

        HandlePlacementCancellation(); // ��ġ ��� (��Ŭ��) ó��
        UpdateGhostPositionAndFeedback(); // ��Ʈ ��ġ ��� �� �ð��� �ǵ��
        HandlePlacementConfirmation(); // ��ġ Ȯ�� (��Ŭ��) ó��
    }

    #region �Է� ó�� (Input Handling)

    /// <summary>
    /// F1-F6 Ű�� �����Ͽ� �ǹ� ��ġ ��带 �����մϴ�.
    /// </summary>
    void HandleBuildingSelectionInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[Key.F1].wasPressedThisFrame && buildingPrefabs.Count > 0)
            StartPlacingBuilding(buildingPrefabs[0]);
        if (Keyboard.current[Key.F2].wasPressedThisFrame && buildingPrefabs.Count > 1)
            StartPlacingBuilding(buildingPrefabs[1]);
        if (Keyboard.current[Key.F3].wasPressedThisFrame && buildingPrefabs.Count > 2)
            StartPlacingBuilding(buildingPrefabs[2]);
        // (F4 ~ F6...)
    }

    /// <summary>
    /// ���콺 ��Ŭ������ ��ġ�� ����մϴ�.
    /// </summary>
    void HandlePlacementCancellation()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            StopPlacing();
        }
    }

    /// <summary>
    /// ���콺 ��Ŭ������ ��ġ�� Ȯ���մϴ�. (��ġ ������ ����)
    /// </summary>
    void HandlePlacementConfirmation()
    {
        if (isCurrentPlacementValid && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceBuilding();
        }
    }

    #endregion

    #region ��ġ ���� (Placement Logic)

    /// <summary>
    /// ��ġ ��带 �����ϰ� '��Ʈ'�� �����մϴ�.
    /// </summary>
    public void StartPlacingBuilding(GameObject prefabToPlace)
    {
        if (prefabToPlace == null) return;

        StopPlacing(); // Ȥ�� �̹� ��� �ִ� �� �ִٸ� ����

        currentPrefabToBuild = prefabToPlace;
        ghostBuilding = Instantiate(prefabToPlace);
        ghostRenderer = ghostBuilding.GetComponentInChildren<Renderer>();

        Building buildingInfo = ghostBuilding.GetComponent<Building>();
        if (buildingInfo != null)
        {
            currentBuildingSize = buildingInfo.size;
            currentBuildingHeight = buildingInfo.height;
        }
        else
        {
            currentBuildingSize = new Vector2Int(1, 1);
            currentBuildingHeight = 1.0f;
            UnityEngine.Debug.LogError("�����տ� 'Building.cs' ������Ʈ�� �����ϴ�!");
        }

        Collider ghostCollider = ghostBuilding.GetComponent<Collider>();
        if (ghostCollider != null) ghostCollider.enabled = false;

        // ��Ʈ�� �׻� �ʷϻ�(Valid) ������ ��� (Opaque ����)
        if (ghostRenderer != null) ghostRenderer.material = validMaterial;
    }

    /// <summary>
    /// ��Ʈ ��ġ�� �����ϰ�, �ǵ��(��Ŀ/��Ʈ ����)�� ó���մϴ�.
    /// </summary>
    void UpdateGhostPositionAndFeedback()
    {
        if (Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayerMask))
        {
            currentGridPos = gridManager.WorldToGridPosition(hit.point);

            // �Ǻ� Y��ǥ ���
            Vector3 cornerPos = gridManager.GridToWorldPosition_BottomLeft(currentGridPos);
            Vector3 pivotOffset = new Vector3(
                (currentBuildingSize.x * gridManager.gridSize) / 2.0f,
                currentBuildingHeight / 2.0f, // �ǹ� ���� ������ ����
                (currentBuildingSize.y * gridManager.gridSize) / 2.0f
            );

            // ���� ���� ��ġ�� ����
            currentSnappedWorldPos = cornerPos + pivotOffset;
            ghostBuilding.transform.position = currentSnappedWorldPos;

            // ��ȿ�� �˻�
            List<Vector2Int> invalidCells = gridManager.GetInvalidCells(currentGridPos, currentBuildingSize);
            isCurrentPlacementValid = (invalidCells.Count == 0);

            // --- [�ٽ� ���� 1] ---
            // '��Ʈ'�� ����� ������ �����ϰ�, �׻� ���̵��� �մϴ�.
            if (ghostRenderer != null)
            {
                ghostRenderer.enabled = true;
            }
            // '�κ��� ���� ��Ŀ'�� �׻� �����մϴ�.
            ShowGridMarkers(invalidCells);
            // --------------------
        }
    }

    /// <summary>
    /// '�κ��� ���� ��Ŀ'�� ǥ���մϴ�.
    /// </summary>
    void ShowGridMarkers(List<Vector2Int> invalidCells)
    {
        // 1. ��� ��Ŀ ����
        foreach (GameObject marker in gridMarkers)
        {
            marker.SetActive(false);
        }

        // 2. �ʿ��� ��ŭ ��Ŀ ��Ȱ��/���� �ؼ� ��ġ
        for (int i = 0; i < invalidCells.Count; i++)
        {
            GameObject marker;
            if (i < gridMarkers.Count)
            {
                marker = gridMarkers[i];
            }
            else
            {
                marker = Instantiate(gridMarkerPrefab);
                gridMarkers.Add(marker);
            }

            Vector3 markerPos = gridManager.GridToWorldPosition_BottomLeft(invalidCells[i]);
            markerPos.x += gridManager.gridSize / 2.0f;
            markerPos.z += gridManager.gridSize / 2.0f;

            // --- [�ٽ� ����!] ---
            // ����(��Ŀ)�� ���̸� (�ٴ� ���� + 0.01f) + (���� �ǹ� ����)�� �����մϴ�.
            // ��: 0.51f + 1.0f = 1.51f
            markerPos.y = (gridManager.transform.position.y + 0.01f) + currentBuildingHeight;
            // --------------------

            marker.transform.position = markerPos;
            marker.SetActive(true);
        }
    }

    /// <summary>
    /// ���� �ǹ��� ��ġ�մϴ�.
    /// </summary>
    void PlaceBuilding()
    {
        Instantiate(currentPrefabToBuild, currentSnappedWorldPos, Quaternion.identity);
        gridManager.SetGridOccupied(currentGridPos, currentBuildingSize, true);
        StopPlacing();
    }

    /// <summary>
    /// ��ġ ��带 �����ϰ� ��� �ӽ� ������Ʈ�� �����մϴ�.
    /// </summary>
    void StopPlacing()
    {
        if (ghostBuilding != null) Destroy(ghostBuilding);
        ghostBuilding = null;
        currentPrefabToBuild = null;
        isCurrentPlacementValid = false;

        foreach (GameObject marker in gridMarkers)
        {
            marker.SetActive(false);
        }
    }

    #endregion
}


