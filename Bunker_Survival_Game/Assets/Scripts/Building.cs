using UnityEngine;

/// <summary>
/// 건물의 모든 속성을 정의하는 스크립트입니다.
/// (BuildingPlacer가 사용하는 '크기/높이' 정보와
///  NPC가 사용하는 '욕구/이벤트' 정보를 모두 포함합니다)
/// </summary>
public class Building : MonoBehaviour
{
    [Header("1. 배치 시스템 정보 (Placement)")]
    [Tooltip("그리드 시스템이 사용할 건물의 크기입니다.")]
    public Vector2Int size = new Vector2Int(1, 1); // 건물 크기

    [Tooltip("그리드 시스템이 사용할 건물의 실제 높이입니다.")]
    public float height = 1.0f; // 건물 높이

    [Header("2. NPC 욕구 제공 정보 (Needs)")]
    [Tooltip("이 건물이 제공하는 '욕구'의 타입 (GameObject의 Tag로 자동 감지됨)")]
    [SerializeField] // 인스펙터에서는 보이지만 다른 스크립트는 접근 불가
    private string providesNeedTag; // 태그에 따라서 타입이 결정

    [Tooltip("이 건물이 기본적으로 제공하는 욕구 충족량입니다.")]
    public float baseSatisfaction = 20.0f; // 기본 수치 (얼마만큼 제공하는지)

    [Tooltip("성격에 따른 배율// 일단 추가했습니다.")]
    public float qualityMultiplier = 1.0f; // NPC 성격 가중치

    [Header("3. 이벤트 상호작용 (Events)")]
    [Tooltip("전력 고장 같은 이벤트에 영향을 받는지 여부입니다.")]
    public bool isAffectedByEvent = true; 

    [Tooltip("이벤트가 활성화되었을 때의 욕구 충족량입니다. (예: 전력 나간 냉장고 = 0)")]
    public float satisfactionDuringEvent = 0.0f; // 이벤트 상황일 때 수치

    // 이벤트 매니저에 의해 제어되는 현재 이벤트 상태
    private bool isEventActive = false;

    /// <summary>
    /// 스크립트가 시작될 때, 이 GameObject의 Tag를 읽어서
    /// 'providesNeedTag' 변수를 자동으로 설정합니다.
    /// </summary>
    void Start()
    {
        DetectNeedFromTag();
    }

    /// <summary>
    /// 2. 태그에 따라서 타입이 결정되게 해줘
    /// -> 이 GameObject의 Tag를 'GameDefinitions.cs'와 비교하여 타입을 결정합니다.
    /// </summary>
    private void DetectNeedFromTag()
    {
        if (this.CompareTag(NeedType.Hunger))
            providesNeedTag = NeedType.Hunger;
        else if (this.CompareTag(NeedType.Energy))
            providesNeedTag = NeedType.Energy;
        else if (this.CompareTag(NeedType.Hygiene))
            providesNeedTag = NeedType.Hygiene;
        else if (this.CompareTag(NeedType.Toilet))
            providesNeedTag = NeedType.Toilet;
        else if (this.CompareTag(NeedType.Social))
            providesNeedTag = NeedType.Social;
        else if (this.CompareTag(NeedType.Fun))
            providesNeedTag = NeedType.Fun;
        else
            providesNeedTag = "None"; // 아무 욕구도 제공하지 않음
    }

    // --- Public Functions (다른 스크립트가 호출) ---

    /// <summary>
    /// EventManager가 이 함수를 호출하여 이 건물의 이벤트 상태를 변경합니다.
    /// </summary>
    public void SetEventStatus(bool isActive)
    {
        isEventActive = isActive;
    }

    /// <summary>
    /// NPC가 이 함수를 호출하여 이 건물에서 얻을 수 있는 최종 만족도를 계산합니다.
    /// (NPC 성격 가중치를 'weight'로 받아옵니다)
    /// </summary>
    public float GetSatisfactionAmount(float npcPersonalityWeight = 1.0f)
    {
        float currentSatisfaction;

        // 1. 이벤트 상황인지 확인
        if (isAffectedByEvent && isEventActive)
        {
            currentSatisfaction = satisfactionDuringEvent; // 이벤트 상황일 때 수치
        }
        else
        {
            currentSatisfaction = baseSatisfaction; // 기본 수치
        }

        // 2. 건물 품질과 NPC 성격 가중치 적용
        return currentSatisfaction  * npcPersonalityWeight;
    }

    /// <summary>
    /// NPC가 이 건물이 어떤 욕구를 채워주는지 물어볼 때 사용합니다.
    /// </summary>
    public string GetProvidedNeedTag()
    {
        return providesNeedTag;
    }
}
