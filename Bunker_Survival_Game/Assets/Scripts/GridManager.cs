using UnityEngine;
using System.Collections.Generic; // List<>�� ����ϱ� ���� �߰�

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

    // [�ʼ� �Լ� 1] ���� ��ǥ -> �׸��� ��ǥ (���콺 ��ġ ��ȯ��)
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(relativePos.x / gridSize);
        int z = Mathf.FloorToInt(relativePos.z / gridSize);
        return new Vector2Int(x, z);
    }

    // [�ʼ� �Լ� 2] �׸��� ��ǥ -> ���� ��ǥ (�ǹ� ��ġ�� '�𼭸�')
    public Vector3 GridToWorldPosition_BottomLeft(Vector2Int gridPosition)
    {
        float x = (gridPosition.x * gridSize) + gridOrigin.x;
        float z = (gridPosition.y * gridSize) + gridOrigin.z;
        return new Vector3(x, gridOrigin.y, z);
    }

    // [�ʼ� �Լ� 3] �׸��� ��ǥ�� �� ���� ������ Ȯ�� (���� �Լ�)
    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < mapWidth &&
               gridPos.y >= 0 && gridPos.y < mapHeight;
    }

    // [�ʼ� �Լ� 4] '������ �ִ� ĭ' ��� ��ȯ (��ȿ�� �˻��)
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
                    invalidCells.Add(cellToTest); // �� ��� ĭ
                }
                else if (occupancyGrid[cellToTest.x, cellToTest.y])
                {
                    invalidCells.Add(cellToTest); // �̹� �� ĭ
                }
            }
        }
        return invalidCells;
    }

    // [�ʼ� �Լ� 5] �׸��忡 '������' ǥ�� (��ġ Ȯ����)
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