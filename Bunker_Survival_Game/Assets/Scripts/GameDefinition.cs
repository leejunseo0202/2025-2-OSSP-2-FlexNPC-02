using UnityEngine;

/// <summary>
/// NPC의 욕구 타입을 Unity 'Tag' 이름과 일치시키기 위한 공용 정의 클래스입니다.
/// (이 파일은 NPC 스크립트와 Building 스크립트가 모두 참조합니다)
/// </summary>
public static class NeedType
{
    // 님이 요청하신 대로, Unity Tag 이름과 변수명을 동일하게 맞춥니다.
    public const string Hunger = "Hunger";
    public const string Energy = "Energy";
    public const string Hygiene = "Hygiene";
    public const string Toilet = "Toilet";
    public const string Social = "Social";
    public const string Fun = "Fun";
}

// --- [이 아래 부분이 추가되었습니다] ---

/// <summary>
/// 벙커의 공용 자원(Resource) 타입을 정의합니다.
/// (예: Building.cs가 어떤 자원을 필요로 하는지 명시할 때 사용)
/// </summary>
public static class ResourceType
{
    public const string Power = "Power";
    public const string Water = "Water";
    public const string Food = "Food";
    // (필요시 '산소(Oxygen)' 등 추가)
}

/// <summary>
/// 벙커 전체에 영향을 미치는 이벤트(Event) 타입을 정의합니다.
/// (예: EventManager.cs가 이벤트를 발동시킬 때 사용)
/// </summary>
public static class EventType
{
    public const string PowerOutage = "PowerOutage"; // 단전
    public const string WaterOutage = "WaterOutage"; // 단수
    public const string DiseaseOutbreak = "DiseaseOutbreak"; // 질병 발생
    // (필요시 'NPC 갈등(Conflict)' 등 추가)
}

