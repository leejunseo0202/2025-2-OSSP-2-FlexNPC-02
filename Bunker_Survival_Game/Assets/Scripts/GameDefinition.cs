using UnityEngine;
using System; // [Serializable]을 위해 추가

/// <summary>
/// 게임 내 모든 공용 태그와 타입을 정의하는 '공용 사전'입니다.
/// [수정됨] NeedType(욕구)과 ResourceType(자원)을 명확히 분리합니다.
/// </summary>
public static class GameDefinitions
{
    /// <summary>
    /// 건물/NPC가 참조하는 "NPC 6대 욕구" 태그 이름
    /// </summary>
    public static class NeedType
    {
        public const string Hunger = "Hunger";
        public const string Energy = "Energy";
        public const string Hygiene = "Hygiene";
        public const string Toilet = "Toilet";
        public const string Social = "Social";
        public const string Fun = "Fun";
    }

    /// <summary>
    /// 건물이 작동하기 위해 필요한 "자원" 태그 이름
    /// </summary>
    public static class ResourceType
    {
        [Tooltip("건물이 작동하기 위해 필요한 자원 태그 (예: 전기)")]
        public const string Power = "Power";
        [Tooltip("건물이 작동하기 위해 필요한 자원 태그 (예: 물)")]
        public const string Water = "Water";
        [Tooltip("정수기/자판기 등이 저장하는 자원 태그 (예: 식량)")]
        public const string Food = "Food"; // "Hunger" 욕구와 맵핑됨
    }

    /// <summary>
    /// 벙커 이벤트 타입
    /// </summary>
    public static class EventType
    {
        public const string PowerOutage = "PowerOutage"; // 단전
        public const string WaterOutage = "WaterOutage"; // 단수
        public const string DiseaseOutbreak = "DiseaseOutbreak"; // 질병 발생
    }
}