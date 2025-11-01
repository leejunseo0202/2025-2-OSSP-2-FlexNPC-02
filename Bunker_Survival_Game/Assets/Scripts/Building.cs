using UnityEngine;

// 이 스크립트는 '건물' 프리팹에 붙어서
// 자신의 그리드 크기를 알려주는 역할을 합니다.
public class Building : MonoBehaviour
{
    public Vector2Int size = new Vector2Int(1, 1);

    public float height = 1.0f;
}