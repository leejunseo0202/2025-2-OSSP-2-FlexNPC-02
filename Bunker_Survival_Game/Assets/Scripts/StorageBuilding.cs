using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List 검색(Find)을 위해 추가
using System; // [Serializable]을 위해 추가

/// <summary>
/// "부모" Building을 상속받는 '저장고' 타입 건물입니다. (예: 정수기, 자판기)
/// [수정됨] NPC에게 '다중 수치 조정' 목록을 반환합니다.
/// </summary>
public class StorageBuilding : Building // Building을 상속!
{
    // --- '저장고' 건물만 가지는 고유 변수 ---
    [Header("3. 기능 (Effects)")]
    [Tooltip("이 건물을 사용했을 때 NPC에게 적용될 '수치 조정' 목록 (예: Fun+20, Energy-20)")]
    public List<NeedModification> needEffects;

    [Header("4. 재고 (Storage)")]
    [Tooltip("최대 재고량 (예: 200.0)")]
    public float maxStock = 100.0f;
    [Tooltip("현재 재고량 (정수기에 남은 물의 총 양)")]
    public float currentStock = 100.0f;
    [Tooltip("1회 사용 시 '재고'에서 차감할 양 (이 값은 NPC 요청량과 무관)")]
    public float stockConsumedPerUse = 1.0f; // (예: 자판기 1회 사용 시 재고 1 차감)

    [Header("5. NPC별 누적 총량 캡 (Local Rules)")]
    [Tooltip("이 *특정 건물*이 각 NPC에게 허용하는 *누적 총량 캡* 목록입니다.")]
    public List<NpcRationRule> npcRationRules;
    [Tooltip("위 목록에 *없는* NPC에게 적용할 기본 *누적 총량 캡*")]
    public float defaultCap = 50.0f;

    [System.Serializable]
    public class NpcRationRule
    {
        [Tooltip("NpcAI.cs에 정의된 NPC의 고유 ID (예: Human_A, Human_B)")]
        public string npcId;
        [Tooltip("이 *건물*이 이 *NPC*에게만 허용하는 *누적 총량 캡* (예: 40)")]
        public float amountCap; // (예: 40)
    }
    // ------------------------------------

    // 이 건물이 NPC별로 "지금까지 총 얼마를 줬는지" 기억하는 장부
    private Dictionary<string, float> npcUsageTracker = new Dictionary<string, float>();
    // ------------------------------------


    /// <summary>
    /// [핵심] 부모(Building)의 'UseBuilding' 함수를 '저장고' 방식대로 구현(override)합니다.
    /// </summary>
    public override List<NeedModification> UseBuilding(string npcId, float amountRequested)
    {
        // 1. 건물이 작동 중이 아니거나, 재고가 없으면 (실패)
        if (!isFunctioning || currentStock <= 0)
        {
            return new List<NeedModification>(); // 빈 리스트 반환
        }

        // 2. 이 NPC의 '누적 총량 캡'을 찾음 (예: 40)
        float totalCap = GetCapForNpc(npcId);

        // 3. 이 NPC가 '지금까지 가져간 총량'을 장부(Tracker)에서 찾음
        float amountAlreadyTaken = 0f;
        if (npcUsageTracker.ContainsKey(npcId))
        {
            amountAlreadyTaken = npcUsageTracker[npcId];
        }

        // 4. 이 NPC에게 '남아있는 캡'을 계산
        float remainingCap = totalCap - amountAlreadyTaken;

        // 4a. 이미 캡을 다 채웠으면 (실패)
        if (remainingCap <= 0)
        {
            return new List<NeedModification>(); // 빈 리스트 반환
        }

        // 5. NPC의 '요청량'(예: 10)과 '남은 캡'(예: 40) 중 더 작은 값을 선택
        // (이 값은 이제 '재고'가 아닌 '누적 캡' 계산에만 사용됩니다)
        float capConsumed = Mathf.Min(amountRequested, remainingCap);

        // 6. 실제 재고(Stock)가 1회 사용량보다 적은지 확인
        if (currentStock < stockConsumedPerUse)
        {
            return new List<NeedModification>(); // 재고 부족 (실패)
        }

        // 7. [성공!] 모든 검사 통과
        currentStock -= stockConsumedPerUse; // 건물 재고 차감
        npcUsageTracker[npcId] = amountAlreadyTaken + capConsumed; // NPC 장부 갱신

        return needEffects; // "성공했으니 [Fun+20, Energy-20] 효과를 받아라"
    }

    /// <summary>
    /// [헬퍼 함수] NPC 'ID'에 맞는 '*누적 총량 캡*'을 목록에서 찾아 반환
    /// </summary>
    private float GetCapForNpc(string npcId)
    {
        var rule = npcRationRules.Find(r => r.npcId == npcId);
        if (rule != null)
        {
            return rule.amountCap; // 찾았으면 해당 값 반환 (예: 40.0)
        }
        else
        {
            return defaultCap; // 못 찾았으면 기본값 반환 (예: 50.0)
        }
    }
}