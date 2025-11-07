using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List 검색(Find)을 위해 추가
using System; // [Serializable]을 위해 추가

// --- [핵심 추가!] ---
/// <summary>
/// NPC의 어떤 욕구(Need)를 얼마만큼(Amount) 변경할지 정의하는 클래스
/// (예: Fun +20, Energy -20)
/// </summary>
[System.Serializable]
public class NeedModification
{
    [Tooltip("변경할 욕구의 태그 (GameDefinitions.NeedType 사용, 예: 'Fun', 'Energy')")]
    public string needTag;
    [Tooltip("변경할 수치 (예: +20, -20)")]
    public float amount;
}
// --- [여기까지] ---


/// <summary>
/// [리팩토링됨] 모든 건물의 "부모" 역할을 하는 추상 클래스입니다.
/// [수정됨] "태그 감지" 기능이 삭제되고, "다중 수치 조정" 기능으로 대체됩니다.
/// </summary>
public abstract class Building : MonoBehaviour
{
    [Header("1. 기본 정보 (Grid & Placement)")]
    // --- [핵심 수정!] ---
    [Tooltip("게임 내 UI에 표시될 건물의 이름 (예: '정수기 A', '침대')")]
    public string buildingName = "New Building";
    // --- [여기까지] ---
    [Tooltip("건물의 그리드 크기 (예: 2x2)")]
    public Vector2Int size = new Vector2Int(1, 1);
    [Tooltip("건물의 실제 높이 (Y좌표 계산용)")]
    public float height = 1.0f;

    // ----------------------------------------------------------------

    [Header("2. 기능 (Needs & Resources)")]
    // [삭제됨] providesNeedTag (아래 'needEffects' 리스트로 대체됨)

    [Tooltip("작동을 위해 필요한 자원 (GameDefinitions.ResourceType 상수 사용, 예: 'Power')")]
    public string requiredResourceTag; //GameDefinitions.ResourceType.Power;
    [Tooltip("자원이 없으면 작동을 멈추는지 여부")]
    public bool requiresResourceToFunction = false;

    // ----------------------------------------------------------------

    [Header("5. 현재 상태 (Runtime)")]
    [Tooltip("현재 이 건물이 이벤트 등으로 인해 작동 중지되었는지 여부")]
    public bool isFunctioning = true;


    // [삭제됨] Start()와 DetectNeedFromTag() (더 이상 태그 감지 안 함)


    /// <summary>
    /// [핵심 함수 - 변경됨]
    /// NPC가 이 건물을 사용하려고 시도합니다.
    /// 성공 시: NPC에게 적용될 '수치 조정 목록' (Fun+20, Energy-20)을 반환합니다.
    /// 실패 시: 빈(Empty) 리스트를 반환합니다.
    /// </summary>
    public abstract List<NeedModification> UseBuilding(string npcId, float amountRequested);


    /// <summary>
    /// [이벤트 연동 함수] EventManager가 이 함수를 호출하여 건물의 상태를 변경합니다.
    /// </summary>
    public void HandleEvent(string eventType, bool isActive)
    {
        // 예: "단전" 이벤트가 발생했고, 이 건물이 "전기"를 필요로 한다면
        if (eventType == GameDefinitions.EventType.PowerOutage && requiredResourceTag == GameDefinitions.ResourceType.Power)
        {
            this.isFunctioning = isActive; // 작동 상태 변경 (false)
        }

        // 예: "단수" 이벤트
        if (eventType == GameDefinitions.EventType.WaterOutage && requiredResourceTag == GameDefinitions.ResourceType.Water)
        {
            this.isFunctioning = isActive;
        }
    }

    // --- [ReadOnly] 어트리뷰트 관련 헬퍼 (공통) ---
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(position, property, label);
            GUI.enabled = wasEnabled;
        }
    }
#endif
    public class ReadOnlyAttribute : PropertyAttribute { }
}