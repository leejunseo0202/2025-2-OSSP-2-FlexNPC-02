using UnityEngine;
using System.Collections.Generic; // List<>를 사용하기 위해 추가

public class GridManager : MonoBehaviour
{
    public int mapWidth = 100;
    public int mapHeight = 100;
    public float gridSize = 1f;
    public Vector3 gridOrigin = Vector3.zero;

    // 1차 필터: 이 레이어에 속한 오브젝트만 검사 대상으로 합니다.
    public LayerMask obstacleLayerMask;

    // 2차 필터: 1차 필터를 통과한 오브젝트 중, 이 리스트에 있는 태그를 가진 것만 '장애물'로 인식합니다.
    public List<string> obstacleTags = new List<string>();

    private bool[,] occupancyGrid;

    void Awake()
    {
        occupancyGrid = new bool[mapWidth, mapHeight];

        // Awake에서 gridOrigin.y를 오브젝트의 실제 Y위치로 자동 설정
        gridOrigin.y = this.transform.position.y;
    }

    // [필수 함수 1] 월드 좌표 -> 그리드 좌표 (마우스 위치 변환용)
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(relativePos.x / gridSize);
        int z = Mathf.FloorToInt(relativePos.z / gridSize);
        return new Vector2Int(x, z);
    }

    // [필수 함수 2] 그리드 좌표 -> 월드 좌표 (건물 배치용 '모서리')
    public Vector3 GridToWorldPosition_BottomLeft(Vector2Int gridPosition)
    {
        float x = (gridPosition.x * gridSize) + gridOrigin.x;
        float z = (gridPosition.y * gridSize) + gridOrigin.z;
        return new Vector3(x, gridOrigin.y, z);
    }

    // [필수 함수 3] 그리드 좌표가 맵 범위 안인지 확인 (헬퍼 함수)
    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < mapWidth &&
               gridPos.y >= 0 && gridPos.y < mapHeight;
    }

    // [필수 함수 4] '문제가 있는 칸' 목록 반환 (유효성 검사용)
    // [수정됨] 건물의 'height'를 매개변수로 받습니다.
    public List<Vector2Int> GetInvalidCells(Vector2Int gridPos, Vector2Int size, float height)
    {
        List<Vector2Int> invalidCells = new List<Vector2Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cellToTest = new Vector2Int(gridPos.x + x, gridPos.y + y);

                if (!IsValidGridPosition(cellToTest))
                {
                    invalidCells.Add(cellToTest); // 1. 맵 벗어난 칸
                }
                else if (occupancyGrid[cellToTest.x, cellToTest.y])
                {
                    invalidCells.Add(cellToTest); // 2. '장부'(Grid)에 이미 찬 칸
                }
                else
                {
                    // --- [이 블록 전체를 수정하세요] ---
                    // 3. '물리적' 장애물 검사 (Tag 기반)

                    // 이 셀의 월드 좌표 중심 계산
                    Vector3 cellCenter = GridToWorldPosition_BottomLeft(cellToTest);
                    cellCenter.x += gridSize / 2.0f;
                    cellCenter.z += gridSize / 2.0f;
                    // 바닥(transform.position.y) + 건물 높이의 절반
                    cellCenter.y = transform.position.y + (height / 2.0f);

                    // 셀 크기(0.9 곱해서 여유 둠)의 박스로 'obstacleLayerMask'에 있는 모든 콜라이더를 가져옴
                    Vector3 halfExtents = new Vector3(gridSize * 0.45f, height * 0.45f, gridSize * 0.45f);
                    Collider[] hits = Physics.OverlapBox(cellCenter,
                                                         halfExtents,
                                                         Quaternion.identity,
                                                         obstacleLayerMask, // 1차 필터: 이 레이어들만 검사
                                                         QueryTriggerInteraction.Ignore);

                    // 태그 리스트가 비어있지 않고, 충돌한 오브젝트가 있을 때만 2차 검사
                    if (hits.Length > 0 && obstacleTags.Count > 0)
                    {
                        // 2차 필터: 가져온 콜라이더의 태그가 'obstacleTags' 리스트에 있는지 검사
                        bool tagFound = false;
                        foreach (Collider hit in hits)
                        {
                            foreach (string tag in obstacleTags)
                            {
                                // 태그가 null이거나 비어있지 않은지 확인
                                if (!string.IsNullOrEmpty(tag) && hit.CompareTag(tag))
                                {
                                    invalidCells.Add(cellToTest); // 3. 물리적 장애물(태그 일치) 발견
                                    tagFound = true;
                                    break; // 안쪽 루프 탈출
                                }
                            }
                            if (tagFound) break; // 바깥쪽 루프 탈출
                        }
                    }
                    // ---------------------------------
                }
            }
        }
        return invalidCells;
    }

    // [필수 함수 5] 그리드에 '차있음' 표시 (배치 확정용)
    public void SetGridOccupied(Vector2Int gridPos, Vector2Int size, bool isOccupied)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cellToUpdate = new Vector2Int(gridPos.x + x, gridPos.y + y);
                if (IsValidGridPosition(cellToUpdate))
                {
                    occupancyGrid[cellToUpdate.x, cellToUpdate.y] = isOccupied;
                }
            }
        }
    }
}

