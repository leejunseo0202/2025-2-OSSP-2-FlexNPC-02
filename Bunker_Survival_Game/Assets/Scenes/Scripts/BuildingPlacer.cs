using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildingPlacer : MonoBehaviour
{
    // --- [누락되었던 변수 정의] ---
    private GridManager gridManager;
    public LayerMask groundLayerMask;

    [Header("프리팹")]
    public List<GameObject> buildingPrefabs;
    public GameObject gridMarkerPrefab; // 'GridMarker' 프리팹

    [Header("피드백 재질")]
    public Material validMaterial;
    // 'invalidMaterial'은 'GridMarker' 프리팹이 사용합니다.

    private GameObject ghostBuilding;
    private Renderer ghostRenderer;
    private Vector2Int currentGridPos;
    private Vector2Int currentBuildingSize;
    private GameObject currentPrefabToBuild;
    private float currentBuildingHeight; // 건물의 실제 높이

    // 마커 및 상태 변수
    private List<GameObject> gridMarkers = new List<GameObject>();
    private bool isCurrentPlacementValid = false;
    private Vector3 currentSnappedWorldPos; // 현재 스냅된 위치
    // --- [여기까지] ---

    void Start()
    {
        gridManager = GetComponent<GridManager>();
        if (gridManager == null)
        {
            UnityEngine.Debug.LogError("GridManager 컴포넌트를 찾을 수 없습니다!");
        }
    }

    // [Refactored] 1. 입력 처리
    void Update()
    {
        // 1-1. 건물 선택 입력
        HandleBuildingSelectionInput();

        // 1-2. 배치 취소 입력
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && ghostBuilding != null)
        {
            StopPlacing();
            return;
        }

        // '고스트'가 없으면(배치 모드가 아니면) Update 종료
        if (ghostBuilding == null) return;

        // 1-3. '고스트' 위치 갱신 및 피드백
        UpdateGhostPositionAndFeedback();

        // 1-4. 배치 확정 입력
        if (isCurrentPlacementValid && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceBuilding();
        }
    }

    public void StartPlacingBuilding(GameObject prefabToPlace)
    {
        if (prefabToPlace == null) return;
        currentPrefabToBuild = prefabToPlace;

        if (ghostBuilding != null) Destroy(ghostBuilding);
        ghostBuilding = Instantiate(prefabToPlace);

        // '고스트'가 물리 검사에 감지되지 않도록 레이어 변경
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        ghostBuilding.layer = ignoreRaycastLayer;
        foreach (Transform child in ghostBuilding.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = ignoreRaycastLayer;
        }

        ghostRenderer = ghostBuilding.GetComponentInChildren<Renderer>();

        // 프리팹에서 'size'와 'height'를 모두 가져옴
        Building buildingInfo = ghostBuilding.GetComponent<Building>();
        if (buildingInfo != null)
        {
            currentBuildingSize = buildingInfo.size;
            currentBuildingHeight = buildingInfo.height; // 높이 값 저장
        }
        else
        {
            currentBuildingSize = new Vector2Int(1, 1);
            currentBuildingHeight = 1.0f; // 기본 높이
            UnityEngine.Debug.LogError("'" + prefabToPlace.name + "' 프리팹에 'Building.cs' 컴포넌트가 없습니다!");
        }

        Collider ghostCollider = ghostBuilding.GetComponent<Collider>();
        if (ghostCollider != null) ghostCollider.enabled = false;
    }

    // [Refactored] 2. '고스트' 위치 갱신 및 피드백 처리
    void UpdateGhostPositionAndFeedback()
    {
        if (Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayerMask))
        {
            currentGridPos = gridManager.WorldToGridPosition(hit.point);

            // 피봇 계산
            Vector3 cornerPos = gridManager.GridToWorldPosition_BottomLeft(currentGridPos);
            Vector3 pivotOffset = new Vector3(
                (currentBuildingSize.x * gridManager.gridSize) / 2.0f,
                currentBuildingHeight / 2.0f, // 건물 실제 높이의 절반 사용
                (currentBuildingSize.y * gridManager.gridSize) / 2.0f
            );
            currentSnappedWorldPos = cornerPos + pivotOffset;
            ghostBuilding.transform.position = currentSnappedWorldPos;


            // 'GetInvalidCells' 호출 시 'currentBuildingHeight' 매개변수 추가
            List<Vector2Int> invalidCells = gridManager.GetInvalidCells(currentGridPos, currentBuildingSize, currentBuildingHeight);

            isCurrentPlacementValid = (invalidCells.Count == 0);

            // --- [핵심 수정!] ---

            // 1. '고스트'는 항상 켜져 있고, 항상 초록색(Valid) 재질을 사용합니다.
            // (if/else 밖으로 이동하여 '원래 색'으로 돌아오는 버그 수정)
            if (ghostRenderer != null)
            {
                ghostRenderer.enabled = true;
                ghostRenderer.material = validMaterial; // 항상 초록색
            }

            if (isCurrentPlacementValid)
            {
                // 2. 배치 가능: '마커'를 숨깁니다.
                ShowGridMarkers(new List<Vector2Int>()); // 모든 마커 숨김
            }
            else
            {
                // 3. 배치 불가능: '마커'를 보여줍니다. (고스트는 이미 켜져 있음)
                ShowGridMarkers(invalidCells); // 문제가 되는 칸에 마커 표시
            }
        }
    }

    // [Refactored] 3. 마커 시각화
    void ShowGridMarkers(List<Vector2Int> invalidCells)
    {
        foreach (GameObject marker in gridMarkers)
        {
            marker.SetActive(false);
        }

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

            // 마커의 Y위치를 (바닥 높이 + *건물 높이* + 0.01f)로 변경합니다.
            float groundY = gridManager.transform.position.y;
            markerPos.y = groundY + currentBuildingHeight + 0.01f;

            marker.transform.position = markerPos;
            marker.SetActive(true);
        }
    }

    // [Refactored] 4. 배치 확정
    void PlaceBuilding()
    {
        Instantiate(currentPrefabToBuild, currentSnappedWorldPos, Quaternion.identity);
        gridManager.SetGridOccupied(currentGridPos, currentBuildingSize, true);
        StopPlacing();
    }

    // [Refactored] 5. 입력 처리 헬퍼
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

    // [Refactored] 6. 배치 모드 종료
    void StopPlacing()
    {
        if (ghostBuilding != null) Destroy(ghostBuilding);
        ghostBuilding = null;
        currentPrefabToBuild = null;

        foreach (GameObject marker in gridMarkers)
        {
            marker.SetActive(false);
        }
    }
}

