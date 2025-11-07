using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List 검색(Find)을 위해 추가
using System; // [Serializable]을 위해 추가

/// <summary>
/// "부모" Building을 상속받는 '저장고' 타입 건물입니다. (예: 정수기, 자판기)
/// [수정됨] NPC별로 "1회당 소모량"이 다릅니다. (누적 캡 아님)
/// </summary>
public class StorageBuilding : Building // Building을 상속!
{
    // --- '저장고' 건물만 가지는 고유 변수 ---
    [Header("3. 기능 (Effects)")]
    [Tooltip("이 건물을 사용했을 때 NPC에게 적용될 '수치 조정' 목록 (예: Fun+20, Energy-20)")]
    public List<NeedModification> needEffects;

    [Header("4. 재고 (Storage)")]
    [Tooltip("최대 재고량 (예: 100.0)")]
    public float maxStock = 100.0f;
    [Tooltip("현재 재고량")]
    public float currentStock = 100.0f;

    // [삭제됨] stockConsumedPerUse (아래 목록으로 대체됨)

    // --- [핵심 수정!] ---
    [Header("5. NPC별 1회당 소모량 (Local Rules)")]
    [Tooltip("이 *특정 건물*이 각 NPC에게서 *1회 사용 시 차감*할 재고량입니다.")]
    public List<NpcConsumptionRule> npcConsumptionRules;
    [Tooltip("위 목록에 *없는* NPC에게서 차감할 기본 재고량")]
    public float defaultConsumption = 1.0f;

    [System.Serializable]
    public class NpcConsumptionRule
    {
        [Tooltip("NpcAI.cs에 정의된 NPC의 고유 ID (예: Human_A, Human_B)")]
        public string npcId;
        [Tooltip("이 NPC가 1회 사용 시 '재고(Stock)'에서 소모시키는 양 (예: 20)")]
        public float amountConsumedPerUse; // (예: 20)
    }
    // ------------------------------------

    // [삭제됨] npcUsageTracker (누적 캡 장부)
    // ------------------------------------


    /// <summary>
    /// [핵심] 부모(Building)의 'UseBuilding' 함수를 "1회당 차등 소모" 방식대로 구현(override)합니다.
    /// </summary>
    public override List<NeedModification> UseBuilding(string npcId, float amountRequested)
    {
        // 1. 건물이 작동 중이 아니면 (실패)
        if (!isFunctioning)
        {
            return new List<NeedModification>(); // 빈 리스트 반환
        }

        // 2. 이 NPC("Human_A")의 "1회당 소모량"을 찾음 (예: 20)
        float amountToConsume = GetConsumptionForNpc(npcId);

        // 3. 건물의 '현재 재고'가 '1회당 소모량'보다 적은지 확인
        if (currentStock < amountToConsume)
        {
            return new List<NeedModification>(); // 재고 부족 (실패)
        }

        // 4. [성공!] 모든 검사 통과
        currentStock -= amountToConsume; // 건물 재고에서 "1회당 소모량" 차감

        // NPC가 요청한 'amountRequested' 값은 이 로직에서 무시됩니다.
        // 건물은 단지 NPC에게 "효과 목록"만 반환합니다.
        return needEffects; // "성공했으니 [Fun+20, Energy-20] 효과를 받아라"
    }

    /// <summary>
    /// [헬퍼 함수 - 수정됨] NPC 'ID'에 맞는 "1회당 소모량"을 목록에서 찾아 반환
    /// </summary>
    private float GetConsumptionForNpc(string npcId)
    {
        var rule = npcConsumptionRules.Find(r => r.npcId == npcId);
        if (rule != null)
        {
            return rule.amountConsumedPerUse; // 찾았으면 해당 값 반환 (예: 20.0)
        }
        else
        {
            return defaultConsumption; // 못 찾았으면 기본값 반환 (예: 1.0)
        }
    }
}