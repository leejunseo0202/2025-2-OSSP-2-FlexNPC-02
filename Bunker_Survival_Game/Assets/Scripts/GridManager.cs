using UnityEngine;
using System.Collections.Generic; // List<>를 사용하기 위해 추가

public class GridManager : MonoBehaviour
{
    public int mapWidth = 100;
    public int mapHeight = 100;
    public float gridSize = 1f;
    public Vector3 gridOrigin = Vector3.zero;

    private bool[,] occupancyGrid;

    void Awake()
    {
        occupancyGrid = new bool[mapWidth, mapHeight];
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
    public List<Vector2Int> GetInvalidCells(Vector2Int gridPos, Vector2Int size)
    {
        List<Vector2Int> invalidCells = new List<Vector2Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cellToTest = new Vector2Int(gridPos.x + x, gridPos.y + y);

                if (!IsValidGridPosition(cellToTest))
                {
                    invalidCells.Add(cellToTest); // 맵 벗어난 칸
                }
                else if (occupancyGrid[cellToTest.x, cellToTest.y])
                {
                    invalidCells.Add(cellToTest); // 이미 찬 칸
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