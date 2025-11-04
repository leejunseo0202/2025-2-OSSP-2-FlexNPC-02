using UnityEngine;
using System; // [Serializable]을 위해 추가

/// <summary>
/// 게임 내 모든 공용 태그와 타입을 정의하는 '공용 사전'입니다.
/// [수정됨] ResourceType이 NeedType으로 통합되었습니다.
/// [수정됨] PersonalityType enum이 삭제되었습니다.
/// </summary>
public static class GameDefinitions
{
    /// <summary>
    /// 건물/NPC가 참조하는 모든 태그 이름 정의 (욕구 + 자원)
    /// </summary>
    public static class NeedType
    {
        // --- NPC 6대 욕구 태그 ---
        public const string Hunger = "Hunger";
        public const string Energy = "Energy";
        public const string Hygiene = "Hygiene";
        public const string Toilet = "Toilet";
        public const string Social = "Social";
        public const string Fun = "Fun";

        // --- 자원 타입 태그 ---
        [Tooltip("건물이 작동하기 위해 필요한 자원 태그 (예: 전기)")]
        public const string Power = "Power";
        [Tooltip("건물이 작동하기 위해 필요한 자원 태그 (예: 물)")]
        public const string Water = "Water";
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

