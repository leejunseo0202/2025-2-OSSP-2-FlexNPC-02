using UnityEngine;

public class GridManager : MonoBehaviour
{ // <--- Ŭ���� ����

    public int mapWidth = 100;
    public int mapHeight = 100;
    public float gridSize = 1f;

    // (���� �ܰ迡�� �߰��ߴ� ���� - �ϴ��� 0,0,0���� �Ӵϴ�)
    public Vector3 gridOrigin = Vector3.zero;

    private bool[,] occupancyGrid;

    void Awake()
    {
        occupancyGrid = new bool[mapWidth, mapHeight];
    }

    // --- ��ǥ ��ȯ �Լ��� ---
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

        // --- [���� ����!] ---
        // 'gridSize / 2f' (����) ��� 'gridSize' (��ü)�� ���մϴ�.
        // �̷��� �ϸ� ���� 2¥�� �ǹ��� �ٴ�(y=0)�� �� �� 
        // �Ǻ� ��ġ(y=1)�� ��Ȯ�� ���˴ϴ�.
        float y = gridOrigin.y + gridSize;
        // ------------------

        return new Vector3(x, y, z);
    }

    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < mapWidth &&
               gridPos.y >= 0 && gridPos.y < mapHeight;
    }

    // --- [����!] ������ �߻��� �Լ��� ---
    // �� �Լ����� Ŭ���� { } �ȿ� �־�� �մϴ�.

    public bool IsPlacementValid(Vector2Int gridPos)
    {
        // 1. �� ���� ������?
        if (!IsValidGridPosition(gridPos))
        {
            return false;
        }

        // 2. �̹� ���ִ���? (false�� '�������')
        if (occupancyGrid[gridPos.x, gridPos.y])
        {
            return false;
        }

        // ��� �˻� ���
        return true;
    }

    public void SetGridOccupied(Vector2Int gridPos, bool isOccupied)
    {
        if (IsValidGridPosition(gridPos))
        {
            occupancyGrid[gridPos.x, gridPos.y] = isOccupied;
        }
    }

} // <--- Ŭ���� ��. �� �߰�ȣ �ٱ��� �Լ��� ������ �� �˴ϴ�.