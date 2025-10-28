using UnityEngine;
using UnityEngine.InputSystem;
public class BuildingPlacer : MonoBehaviour
{
    // 1. GridManager를 연결할 변수
    private GridManager gridManager;

    // 2. [임시] 테스트용으로 지을 건물 프리팹
    public GameObject buildingPrefab_Test;

    // 3. 현재 배치 중인 '고스트' 오브젝트
    private GameObject ghostBuilding;

    // 4. 마우스가 닿는 바닥 레이어
    public LayerMask groundLayerMask;

    void Start()
    {
        // GridManager 컴포넌트를 자동으로 찾아와서 연결합니다.
        gridManager = GetComponent<GridManager>();
    }

    // [중요] 이 함수는 나중에 UI 버튼이 호출할 것입니다.
    // (지금은 테스트를 위해 키보드 '1'키로 호출해봅시다)
    public void StartPlacingBuilding(GameObject prefabToPlace)
    {
        // 이미 배치 중인 고스트가 있다면 삭제
        if (ghostBuilding != null)
        {
            Destroy(ghostBuilding);
        }

        // 프리팹으로 '고스트'를 생성합니다.
        ghostBuilding = Instantiate(prefabToPlace);

        // (팁: 고스트가 물리법칙이나 다른 것에 충돌하지 않도록)
        Collider ghostCollider = ghostBuilding.GetComponent<Collider>();
        if (ghostCollider != null)
        {
            ghostCollider.enabled = false;
        }
    }

    void Update()
    {
        // --- 테스트용 키보드 입력 (Input System 버전으로 변경) ---
        // 'Alpha1' -> 'Digit1'로 수정
        if (Keyboard.current != null && Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            StartPlacingBuilding(buildingPrefab_Test);
        }
        // ----------------------------------------------------


        // '고스트'가 손에 들려있을 때 (배치 모드일 때)만 실행
        if (ghostBuilding == null)
        {
            return;
        }

        // 1. 마우스 위치로 레이캐스팅
        // (Input System의 마우스 위치를 사용하는 것이 더 정확합니다)
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayerMask))
        {
            // 2. 마우스가 닿은 월드 좌표를 그리드 좌표로 변환 (GridManager 함수 사용)
            Vector2Int gridPos = gridManager.WorldToGridPosition(hit.point);

            // 3. 그리드 좌표를 다시 월드 좌표로 변환 (칸의 중앙, 스냅)
            Vector3 snappedWorldPos = gridManager.GridToWorldPosition(gridPos);

            // 4. '고스트'를 스냅된 위치로 이동
            ghostBuilding.transform.position = snappedWorldPos;
        }

        // (3단계: 유효성 검사 및 색상 변경 로직이 여기에 추가될 예정)

        // (4단계: 배치 확정 로직이 여기에 추가될 예정)
    }
}