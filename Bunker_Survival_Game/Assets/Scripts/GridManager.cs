using UnityEngine;

public class GridManager : MonoBehaviour
{ // <--- 클래스 시작

    public int mapWidth = 100;
    public int mapHeight = 100;
    public float gridSize = 1f;

    // (이전 단계에서 추가했던 원점 - 일단은 0,0,0으로 둡니다)
    public Vector3 gridOrigin = Vector3.zero;

    private bool[,] occupancyGrid;

    void Awake()
    {
        occupancyGrid = new bool[mapWidth, mapHeight];
    }

    // --- 좌표 변환 함수들 ---
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(relativePos.x / gridSize);
        int z = Mathf.FloorToInt(relativePos.z / gridSize);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float x = (gridPosition.x * gridSize) + (gridSize / 2f) + gridOrigin.x;
        float z = (gridPosition.y * gridSize) + (gridSize / 2f) + gridOrigin.z;

        // --- [여기 수정!] ---
        // 'gridSize / 2f' (절반) 대신 'gridSize' (전체)를 더합니다.
        // 이렇게 하면 높이 2짜리 건물이 바닥(y=0)에 설 때 
        // 피봇 위치(y=1)가 정확히 계산됩니다.
        float y = gridOrigin.y + gridSize;
        // ------------------

        return new Vector3(x, y, z);
    }

    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < mapWidth &&
               gridPos.y >= 0 && gridPos.y < mapHeight;
    }

    // --- [여기!] 에러가 발생한 함수들 ---
    // 이 함수들이 클래스 { } 안에 있어야 합니다.

    public bool IsPlacementValid(Vector2Int gridPos)
    {
        // 1. 맵 범위 안인지?
        if (!IsValidGridPosition(gridPos))
        {
            return false;
        }

        // 2. 이미 차있는지? (false가 '비어있음')
        if (occupancyGrid[gridPos.x, gridPos.y])
        {
            return false;
        }

        // 모든 검사 통과
        return true;
    }

    public void SetGridOccupied(Vector2Int gridPos, bool isOccupied)
    {
        if (IsValidGridPosition(gridPos))
        {
            occupancyGrid[gridPos.x, gridPos.y] = isOccupied;
        }
    }

} // <--- 클래스 끝. 이 중괄호 바깥에 함수가 있으면 안 됩니다.