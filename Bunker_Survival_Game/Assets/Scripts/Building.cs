using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List 검색(Find)을 위해 추가
using System; // [Serializable]을 위해 추가

/// <summary>
/// 벙커 내 모든 상호작용 가능한 건물(침대, 정수기, 샤워실 등)에
/// 부착되는 핵심 스크립트입니다.
/// [수정됨] NPC의 '고유 ID'를 기반으로, *이 건물*의 개별 한도를 적용합니다.
/// </summary>
public class Building : MonoBehaviour
{
    [Header("1. 기본 정보 (Grid & Placement)")]
    [Tooltip("건물의 그리드 크기 (예: 2x2)")]
    public Vector2Int size = new Vector2Int(1, 1);
    [Tooltip("건물의 실제 높이 (Y좌표 계산용)")]
    public float height = 1.0f;

    // ----------------------------------------------------------------

    [Header("2. 기능 (Needs & Resources)")]
    [Tooltip("이 건물이 충족시키는 욕구 (유니티 Tag로 자동 감지됨)")]
    [ReadOnly] public string providesNeedTag;

    [Tooltip("작동을 위해 필요한 자원 (GameDefinitions.NeedType 상수 사용, 예: 'Power')")]
    public string requiredResourceTag = GameDefinitions.NeedType.Power;
    [Tooltip("자원이 없으면 작동을 멈추는지 여부")]
    public bool requiresResourceToFunction = true;

    // ----------------------------------------------------------------

    [Header("3. 재고 (Storage) - (정수기, 자판기 등)")]
    [Tooltip("이 건물이 재고를 가진 '저장고' 타입인지 (예: 정수기, 비상식량 상자)")]
    public bool isStorageBuilding = false;

    [Tooltip("최대 재고량 (예: 200.0)")]
    public float maxStock = 100.0f;
    [Tooltip("현재 재고량 (정수기에 남은 물의 총 양)")]
    public float currentStock = 100.0f;

    // ----------------------------------------------------------------

    [Header("4. NPC별 차등 배급 (Local Rules)")]
    [Tooltip("이 *특정 건물*이 각 NPC에게 제공할 1회 최대 한도(Cap) 목록입니다.")]
    public List<NpcRationRule> npcRationRules;
    [Tooltip("위 목록에 *없는* NPC에게 적용할 기본 한도")]
    public float defaultCap = 10.0f; // (예: 10만 가져감)

    // [System.Serializable]을 붙여야 인스펙터에 노출됨
    [System.Serializable]
    public class NpcRationRule
    {
        [Tooltip("NpcAI.cs에 정의된 NPC의 고유 ID (예: Human_A, Human_B)")]
        public string npcId; // "인간 A", "인간 B"
        [Tooltip("이 *건물*이 이 *NPC*에게만 허용하는 1회 최대 한도 (예: 30, 40)")]
        public float amountCap; // (예: 30)
    }
    // --- [여기까지 수정] ---

    // ----------------------------------------------------------------

    [Header("5. 현재 상태 (Runtime)")]
    [Tooltip("현재 이 건물이 이벤트 등으로 인해 작동 중지되었는지 여부")]
    public bool isFunctioning = true;


    // 스크립트가 시작될 때, 태그를 읽어서 자신의 타입을 설정
    void Start()
    {
        DetectNeedFromTag();
    }

    /// <summary>
    /// [핵심 함수 - 수정됨] NPC가 '자신의 ID'와 '요청량'을 건물에 전달합니다.
    /// </summary>
    /// <param name="npcId">자원을 요청하는 NPC의 고유 ID (예: "Human_A")</param>
    /// <param name="amountRequested">NPC가 (성격/욕구에 따라) 실제로 요청하는 양 (예: 50 "갈망")</param>
    /// <returns>건물이 실제로 제공한 자원의 양</returns>
    public float TryTakeResource(string npcId, float amountRequested)
    {
        // 1. 건물이 작동 중이 아니면 0 반환
        if (!isFunctioning)
        {
            return 0f;
        }

        // 2. 이 건물이 '저장고'가 아니면 (예: 침대)
        if (!isStorageBuilding)
        {
            // 침대는 NPC ID를 기반으로 '휴식 한도'(기본값)를 반환
            return GetCapForNpc(npcId);
        }

        // 3. '저장고'일 경우 (예: 정수기)

        // 3a. 이 NPC("Human_A")의 '최대 한도(Cap)'를 이 건물 목록에서 찾음
        float npcCap = GetCapForNpc(npcId); // (예: 정수기A=30, 정수기B=40)

        // 3b. NPC의 '요청량'(50)과 이 NPC의 '최대 한도'(30) 중 *더 작은 값*을 선택
        // (아무리 갈망(50)해도 30 넘게 못 줌)
        float amountToDispense = Mathf.Min(amountRequested, npcCap);

        // 3c. 재고 확인
        if (currentStock >= amountToDispense)
        {
            // 재고가 충분하면 계산된 양(30)만큼 차감
            currentStock -= amountToDispense;
            return amountToDispense;
        }
        else if (currentStock > 0)
        {
            // 재고가 부족하면 (예: 5 남음), 남은 재고만 줌
            float remainingStock = currentStock;
            currentStock = 0;
            return remainingStock;
        }
        else
        {
            // 재고 0
            return 0f;
        }
    }

    /// <summary>
    /// [헬퍼 함수 - 추가됨] NPC 'ID'에 맞는 '최대 한도(Cap)'를 목록에서 찾아 반환
    /// </summary>
    private float GetCapForNpc(string npcId)
    {
        // 'npcRationRules' 리스트에서 일치하는 ID 찾기
        var rule = npcRationRules.Find(r => r.npcId == npcId);

        if (rule != null)
        {
            return rule.amountCap; // 찾았으면 해당 값 반환 (예: 30.0)
        }
        else
        {
            return defaultCap; // 못 찾았으면 기본값 반환 (예: 10.0)
        }
    }


    /// <summary>
    /// [헬퍼 함수] 이 오브젝트의 유니티 'Tag'를 읽어 'providesNeedTag' 변수를 설정합니다.
    /// </summary>
    private void DetectNeedFromTag()
    {
        // GameDefinitions.cs의 'NeedType' 상수들과 비교
        if (this.CompareTag(GameDefinitions.NeedType.Hunger)) providesNeedTag = GameDefinitions.NeedType.Hunger; // "Hunger"
        else if (this.CompareTag(GameDefinitions.NeedType.Energy)) providesNeedTag = GameDefinitions.NeedType.Energy; // "Energy"
        else if (this.CompareTag(GameDefinitions.NeedType.Hygiene)) providesNeedTag = GameDefinitions.NeedType.Hygiene; // "Hygiene"
        else if (this.CompareTag(GameDefinitions.NeedType.Toilet)) providesNeedTag = GameDefinitions.NeedType.Toilet; // "Toilet"
        else if (this.CompareTag(GameDefinitions.NeedType.Social)) providesNeedTag = GameDefinitions.NeedType.Social; // "Social"
        else if (this.CompareTag(GameDefinitions.NeedType.Fun)) providesNeedTag = GameDefinitions.NeedType.Fun; // "Fun"
        else providesNeedTag = "None"; // 6대 욕구 태그가 아니면 'None'
    }

    /// <summary>
    /// [이벤트 연동 함수] EventManager가 이 함수를 호출하여 건물의 상태를 변경합니다.
    /// </summary>
    public void HandleEvent(string eventType, bool isActive)
    {
        // 예: "단전" 이벤트가 발생했고, 이 건물이 "전기"를 필요로 한다면
        if (eventType == GameDefinitions.EventType.PowerOutage && requiredResourceTag == GameDefinitions.NeedType.Power)
        {
            this.isFunctioning = isActive; // 작동 상태 변경 (false)
        }

        // 예: "단수" 이벤트
        if (eventType == GameDefinitions.EventType.WaterOutage && requiredResourceTag == GameDefinitions.NeedType.Water)
        {
            this.isFunctioning = isActive;
        }
    }

    /// <summary>
    /// 인스펙터에서 providesNeedTag를 읽기 전용으로 보여주기 위한 속성
    /// (이 코드가 없으면 [ReadOnly] 어트리뷰트가 작동하지 않음)
    /// </summary>
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

    /// <summary>
    /// [ReadOnly] 어트리뷰트 정의
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }
}

