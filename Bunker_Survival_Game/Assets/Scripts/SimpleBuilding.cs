using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List 검색(Find)을 위해 추가
using System; // [Serializable]을 위해 추가

/// <summary>
/// "부모" Building을 상속받는 '단순 시설' 타입 건물입니다. (예: 침대, 샤워실)
/// [수정됨] NPC에게 '다중 수치 조정' 목록을 반환합니다.
/// </summary>
public class SimpleBuilding : Building // Building을 상속!
{
    // --- '단순 시설' 건물만 가지는 고유 변수 ---

    [Header("3. 기능 (Effects)")]
    [Tooltip("이 건물을 사용했을 때 NPC에게 적용될 '수치 조정' 목록 (예: Energy+10)")]
    public List<NeedModification> needEffects;

    [Header("4. NPC별 차등 만족도 (Local Rules)")]
    [Tooltip("이 *특정 건물*이 각 NPC에게 제공할 1회 만족도(Cap) 목록입니다.")]
    public List<NpcSatisfactionRule> npcSatisfactionRules;
    [Tooltip("위 목록에 *없는* NPC에게 적용할 기본 만족도")]
    public float defaultSatisfaction = 10.0f;

    [System.Serializable]
    public class NpcSatisfactionRule
    {
        [Tooltip("NpcAI.cs에 정의된 NPC의 고유 ID (예: Human_A, Human_B)")]
        public string npcId;
        [Tooltip("이 *건물*이 이 *NPC*에게만 허용하는 1회 만족도 (예: 30, 40)")]
        public float amountCap;
    }
    // ------------------------------------


    /// <summary>
    /// [핵심] 부모(Building)의 'UseBuilding' 함수를 '단순 시설' 방식대로 구현(override)합니다.
    /// </summary>
    public override List<NeedModification> UseBuilding(string npcId, float amountRequested)
    {
        // 1. 건물이 작동 중이 아니면 (실패)
        if (!isFunctioning)
        {
            return new List<NeedModification>(); // 빈 리스트 반환
        }

        // 2. '단순 시설'은 재고(Stock)가 없습니다.
        // NPC ID를 기반으로 정해진 '만족도' 값을 찾아 *효과 목록에 적용*합니다.

        float satisfactionAmount = GetCapForNpc(npcId);

        // 3. 반환할 효과 목록을 '복사' (원본을 수정하지 않기 위해)
        List<NeedModification> finalEffects = new List<NeedModification>();
        foreach (var effect in needEffects)
        {
            // (예: Energy +10 -> Energy +satisfactionAmount)
            // (만약 amount가 -20 (Energy-20)이면, 그 값은 그대로 둠)
            float finalAmount = (effect.amount > 0) ? satisfactionAmount : effect.amount;

            finalEffects.Add(new NeedModification { needTag = effect.needTag, amount = finalAmount });
        }

        return finalEffects; // "성공했으니 [Energy +30] 효과를 받아라"
    }

    /// <summary>
    /// [헬퍼 함수] NPC 'ID'에 맞는 '최대 만족도(Cap)'를 목록에서 찾아 반환
    /// </summary>
    private float GetCapForNpc(string npcId)
    {
        var rule = npcSatisfactionRules.Find(r => r.npcId == npcId);
        if (rule != null)
        {
            return rule.amountCap; // 찾았으면 해당 값 반환 (예: 30.0)
        }
        else
        {
            return defaultSatisfaction; // 못 찾았으면 기본값 반환 (예: 10.0)
        }
    }
}