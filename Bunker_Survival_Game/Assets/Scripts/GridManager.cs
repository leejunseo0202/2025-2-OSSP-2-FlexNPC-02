using UnityEngine;

public class GridManager : MonoBehaviour
{
    // ���� ũ�� (��: 100x100)
    public int mapWidth = 100;
    public int mapHeight = 100;

    // �׸��� �� ĭ�� ũ�� (1�̸� 1m x 1m)
    public float gridSize = 1f;

    // [�߿�] �׸��� ���� ���¸� ������ 2D �迭
    // (int�� �Ἥ 0=�����, 1=�ǹ�, 2=�� ������ Ȯ���� ���� �ֽ��ϴ�)
    private bool[,] occupancyGrid;

    void Awake()
    {
        // �� ũ�⸸ŭ 2D �迭�� �ʱ�ȭ�մϴ�.
        occupancyGrid = new bool[mapWidth, mapHeight];

        // (���߿��� �ʿ� �̹� ��ġ�� ���̳� ��ֹ�����
        //  �̸� 'true'�� �����ϴ� ������ �ʿ��մϴ�)
    }

    // (���⿡ ������ �Լ����� �߰��� ���Դϴ�)

    // ���� ��ǥ(Vector3)�� �׸��� ��ǥ(Vector2Int)�� ��ȯ
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        // ���� (0,0) ������ �������� ����մϴ�. (�� ������ ���� �޶��� �� ����)
        int x = Mathf.FloorToInt(worldPosition.x / gridSize);
        int z = Mathf.FloorToInt(worldPosition.z / gridSize);

        return new Vector2Int(x, z);
    }

    // �׸��� ��ǥ(Vector2Int)�� ���� ��ǥ(Vector3)�� ��ȯ (ĭ�� �߾�)
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float x = gridPosition.x * gridSize + (gridSize / 2f);
        float z = gridPosition.y * gridSize + (gridSize / 2f); // y�� �ƴ� z�Դϴ�.

        return new Vector3(x, 0, z); // ���̴� 0���� ����
    }

    // [�߿�] Ư�� �׸��� ��ǥ�� ��ȿ����(�� ���� ������) Ȯ��
    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < mapWidth &&
               gridPos.y >= 0 && gridPos.y < mapHeight;
    }
}

