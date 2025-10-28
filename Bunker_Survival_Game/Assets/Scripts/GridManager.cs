using UnityEngine;

public class GridManager : MonoBehaviour
{
    // 맵의 크기 (예: 100x100)
    public int mapWidth = 100;
    public int mapHeight = 100;

    // 그리드 한 칸의 크기 (1이면 1m x 1m)
    public float gridSize = 1f;

    // [중요] 그리드 점유 상태를 저장할 2D 배열
    // (int를 써서 0=비었음, 1=건물, 2=벽 등으로 확장할 수도 있습니다)
    private bool[,] occupancyGrid;

    void Awake()
    {
        // 맵 크기만큼 2D 배열을 초기화합니다.
        occupancyGrid = new bool[mapWidth, mapHeight];

        // (나중에는 맵에 이미 배치된 벽이나 장애물들을
        //  미리 'true'로 설정하는 로직이 필요합니다)
    }

    // (여기에 앞으로 함수들을 추가할 것입니다)

    // 월드 좌표(Vector3)를 그리드 좌표(Vector2Int)로 변환
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        // 맵의 (0,0) 원점을 기준으로 계산합니다. (맵 원점에 따라 달라질 수 있음)
        int x = Mathf.FloorToInt(worldPosition.x / gridSize);
        int z = Mathf.FloorToInt(worldPosition.z / gridSize);

        return new Vector2Int(x, z);
    }

    // 그리드 좌표(Vector2Int)를 월드 좌표(Vector3)로 변환 (칸의 중앙)
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float x = gridPosition.x * gridSize + (gridSize / 2f);
        float z = gridPosition.y * gridSize + (gridSize / 2f); // y가 아닌 z입니다.

        return new Vector3(x, 0, z); // 높이는 0으로 가정
    }

    // [중요] 특정 그리드 좌표가 유효한지(맵 범위 내인지) 확인
    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < mapWidth &&
               gridPos.y >= 0 && gridPos.y < mapHeight;
    }
}

