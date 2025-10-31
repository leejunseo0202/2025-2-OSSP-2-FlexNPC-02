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

        // --- [여기!] CS0104 에러 수정 ---
        // 'Debug.LogError' -> 'UnityEngine.Debug.LogError'로 명확하게 지정
        if (ghostRenderer == null)
        {
            UnityEngine.Debug.LogError("빌딩 프리팹에 Renderer 컴포넌트가 없습니다!");
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
        // --- 테스트용 '1'키 입력 ---
        if (Keyboard.current != null && Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            StartPlacingBuilding(buildingPrefab_Test);
        }

        // --- 배치 취소 (마우스 우클릭) ---
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && ghostBuilding != null)
        {
            Destroy(ghostBuilding);
            ghostBuilding = null;
            ghostRenderer = null;
            return; // 배치 모드 종료
        }

        if (ghostBuilding == null || Mouse.current == null)
        {
            return;
        }

        // --- 1. 레이캐스팅 및 스냅 ---
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayerMask))
        {
            currentGridPos = gridManager.WorldToGridPosition(hit.point);
            Vector3 snappedWorldPos = gridManager.GridToWorldPosition(currentGridPos);
            ghostBuilding.transform.position = snappedWorldPos;

            // --- 2. 유효성 검사 및 '그린/레드' 피드백 ---
            isCurrentPosValid = gridManager.IsPlacementValid(currentGridPos);

            if (ghostRenderer != null)
            {
                ghostRenderer.material = isCurrentPosValid ? validMaterial : invalidMaterial;
            }

            // --- 3. 배치 확정 (마우스 좌클릭) ---
            if (isCurrentPosValid && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlaceBuilding();
            }
        }
    }

    void PlaceBuilding()
    {
        // 1. 실제 건물 생성
        Vector3 worldPos = gridManager.GridToWorldPosition(currentGridPos);
        Instantiate(buildingPrefab_Test, worldPos, Quaternion.identity);

        // 2. [중요] 그리드 데이터를 '차있음'으로 업데이트
        gridManager.SetGridOccupied(currentGridPos, true);

        // 3. 고스트 파괴 및 배치 모드 종료
        Destroy(ghostBuilding);
        ghostBuilding = null;
        ghostRenderer = null;
    }
}