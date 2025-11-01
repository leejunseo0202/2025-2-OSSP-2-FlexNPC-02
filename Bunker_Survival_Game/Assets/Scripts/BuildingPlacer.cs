using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildingPlacer : MonoBehaviour
{
    private GridManager gridManager;
    public LayerMask groundLayerMask;

    [Header("프리팹")]
    public List<GameObject> buildingPrefabs;
    public GameObject gridMarkerPrefab;

    [Header("피드백 재질")]
    public Material validMaterial;

    // --- Private 상태 변수 ---
    private GameObject ghostBuilding;
    private Renderer ghostRenderer;
    private GameObject currentPrefabToBuild;
    private float currentBuildingHeight;
    private Vector2Int currentGridPos;
    private Vector2Int currentBuildingSize;
    private Vector3 currentSnappedWorldPos; // [새 변수] 스냅된 최종 위치 저장
    private bool isCurrentPlacementValid = false; // [새 변수] 현재 배치 가능 여부 저장

    private List<GameObject> gridMarkers = new List<GameObject>();

    void Start()
    {
        gridManager = GetComponent<GridManager>();
    }

    /// <summary>
    /// 매 프레임 호출되며, 입력과 고스트 위치를 관리합니다.
    /// </summary>
    void Update()
    {
        HandleBuildingSelectionInput(); // F1-F6 키 입력 처리

        if (ghostBuilding == null) // 배치 모드가 아니면 중단
        {
            return;
        }

        HandlePlacementCancellation(); // 배치 취소 (우클릭) 처리
        UpdateGhostPositionAndFeedback(); // 고스트 위치 계산 및 시각적 피드백
        HandlePlacementConfirmation(); // 배치 확정 (좌클릭) 처리
    }

    #region 입력 처리 (Input Handling)

    /// <summary>
    /// F1-F6 키를 감지하여 건물 배치 모드를 시작합니다.
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
    /// 마우스 우클릭으로 배치를 취소합니다.
    /// </summary>
    void HandlePlacementCancellation()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            StopPlacing();
        }
    }

    /// <summary>
    /// 마우스 좌클릭으로 배치를 확정합니다. (배치 가능할 때만)
    /// </summary>
    void HandlePlacementConfirmation()
    {
        if (isCurrentPlacementValid && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceBuilding();
        }
    }

    #endregion

    #region 배치 로직 (Placement Logic)

    /// <summary>
    /// 배치 모드를 시작하고 '고스트'를 생성합니다.
    /// </summary>
    public void StartPlacingBuilding(GameObject prefabToPlace)
    {
        if (prefabToPlace == null) return;

        StopPlacing(); // 혹시 이미 들고 있던 게 있다면 정리

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
            UnityEngine.Debug.LogError("프리팹에 'Building.cs' 컴포넌트가 없습니다!");
        }

        Collider ghostCollider = ghostBuilding.GetComponent<Collider>();
        if (ghostCollider != null) ghostCollider.enabled = false;

        // 고스트는 항상 초록색(Valid) 재질을 사용 (Opaque 권장)
        if (ghostRenderer != null) ghostRenderer.material = validMaterial;
    }

    /// <summary>
    /// 고스트 위치를 갱신하고, 피드백(마커/고스트 숨김)을 처리합니다.
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

            // 피봇 Y좌표 계산
            Vector3 cornerPos = gridManager.GridToWorldPosition_BottomLeft(currentGridPos);
            Vector3 pivotOffset = new Vector3(
                (currentBuildingSize.x * gridManager.gridSize) / 2.0f,
                currentBuildingHeight / 2.0f, // 건물 실제 높이의 절반
                (currentBuildingSize.y * gridManager.gridSize) / 2.0f
            );

            // 계산된 최종 위치를 저장
            currentSnappedWorldPos = cornerPos + pivotOffset;
            ghostBuilding.transform.position = currentSnappedWorldPos;

            // 유효성 검사
            List<Vector2Int> invalidCells = gridManager.GetInvalidCells(currentGridPos, currentBuildingSize);
            isCurrentPlacementValid = (invalidCells.Count == 0);

            // --- [핵심 수정 1] ---
            // '고스트'를 숨기는 로직을 제거하고, 항상 보이도록 합니다.
            if (ghostRenderer != null)
            {
                ghostRenderer.enabled = true;
            }
            // '부분적 레드 마커'를 항상 갱신합니다.
            ShowGridMarkers(invalidCells);
            // --------------------
        }
    }

    /// <summary>
    /// '부분적 레드 마커'를 표시합니다.
    /// </summary>
    void ShowGridMarkers(List<Vector2Int> invalidCells)
    {
        // 1. 모든 마커 숨김
        foreach (GameObject marker in gridMarkers)
        {
            marker.SetActive(false);
        }

        // 2. 필요한 만큼 마커 재활용/생성 해서 배치
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

            // --- [핵심 수정!] ---
            // 쿼드(마커)의 높이를 (바닥 높이 + 0.01f) + (현재 건물 높이)로 설정합니다.
            // 예: 0.51f + 1.0f = 1.51f
            markerPos.y = (gridManager.transform.position.y + 0.01f) + currentBuildingHeight;
            // --------------------

            marker.transform.position = markerPos;
            marker.SetActive(true);
        }
    }

    /// <summary>
    /// 실제 건물을 배치합니다.
    /// </summary>
    void PlaceBuilding()
    {
        Instantiate(currentPrefabToBuild, currentSnappedWorldPos, Quaternion.identity);
        gridManager.SetGridOccupied(currentGridPos, currentBuildingSize, true);
        StopPlacing();
    }

    /// <summary>
    /// 배치 모드를 종료하고 모든 임시 오브젝트를 정리합니다.
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


