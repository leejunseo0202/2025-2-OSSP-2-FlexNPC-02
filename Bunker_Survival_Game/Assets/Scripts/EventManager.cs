using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// "단전", "단수", "방화벽" 등 벙커의 위기 이벤트를 관리하고 발동시킵니다.
/// [수정됨] "ID"를 기반으로 개별 방화벽을 제어합니다.
/// </summary>
public class EventManager : MonoBehaviour
{
    // --- [핵심] ---
    // 맵에 있는 모든 방화벽을 'ID'로 찾아 빠르게 접근하기 위한 '전화번호부' (Dictionary)
    // 이 스크립트가 작동하려면, 각 방화벽 오브젝트에 'Firewall.cs' 스크립트가 붙어있어야 합니다.
    private Dictionary<string, Firewall> firewallRegistry = new Dictionary<string, Firewall>();

    // 각 방화벽의 현재 닫힘/열림 상태를 기억
    private Dictionary<string, bool> firewallStates = new Dictionary<string, bool>();
    // --- [여기까지] ---


    // --- 이벤트 현재 상태 ---
    private bool isPowerOut = false;    // 현재 정전 상태인가?
    private bool isWaterOut = false;    // 현재 단수 상태인가?

    void Start()
    {
        // [수정됨] 시작 시 맵의 모든 'Firewall'을 찾아 '전화번호부'에 등록
        RegisterAllFirewalls();
    }

    /// <summary>
    /// 맵에 있는 모든 'Firewall' 스크립트를 찾아 '전화번호부'(Registry)에 등록합니다.
    /// </summary>
    private void RegisterAllFirewalls()
    {
        // 1. 맵의 모든 Firewall 컴포넌트를 찾습니다.
        // (CS0618 경고는 님의 환경에 맞추기 위해 의도된 것입니다)
        Firewall[] allFirewalls = FindObjectsOfType<Firewall>();

        foreach (Firewall fw in allFirewalls)
        {
            if (string.IsNullOrEmpty(fw.firewallId))
            {
                UnityEngine.Debug.LogError($"'{fw.gameObject.name}'에 firewallId가 없어 등록에 실패했습니다.", fw.gameObject);
                continue;
            }

            if (firewallRegistry.ContainsKey(fw.firewallId))
            {
                UnityEngine.Debug.LogError($"'{fw.firewallId}' ID가 중복되었습니다! 맵에 동일한 ID의 방화벽이 2개 이상 있습니다.", fw.gameObject);
            }
            else
            {
                // 2. 전화번호부와 상태 사전에 등록
                firewallRegistry.Add(fw.firewallId, fw);
                firewallStates.Add(fw.firewallId, false); // 'false' = 열림(Open) 상태로 시작
                fw.Open(); // 시작 시 항상 열어둠
            }
        }
    }


    void Update()
    {
        // --- 테스트용 키보드 입력 감지 ---
        if (Keyboard.current == null) return;

        // F7키: 정전 (Toggle)
        if (Keyboard.current[Key.F7].wasPressedThisFrame)
        {
            isPowerOut = !isPowerOut; // 상태 뒤집기
            TriggerEvent(GameDefinitions.EventType.PowerOutage, !isPowerOut);
            UnityEngine.Debug.Log(isPowerOut ? "--- [이벤트] 정전 발생! ---" : "--- [이벤트] 전력 복구됨 ---");
        }

        // F8키: 단수 (Toggle)
        if (Keyboard.current[Key.F8].wasPressedThisFrame)
        {
            isWaterOut = !isWaterOut; // 상태 뒤집기
            TriggerEvent(GameDefinitions.EventType.WaterOutage, !isWaterOut);
            UnityEngine.Debug.Log(isWaterOut ? "--- [이벤트] 단수 발생! ---" : "--- [이벤트] 급수 복구됨 ---");
        }

        // --- [수정됨] F9키: "Firewall_A" 방화벽만 개별 제어 (Toggle) ---
        if (Keyboard.current[Key.F9].wasPressedThisFrame)
        {
            // ID "Firewall_A"를 가진 방화벽만 토글
            ToggleSpecificShutter("Firewall_A");
        }

        // --- [추가됨] F10키: "Firewall_B" 방화벽만 개별 제어 (Toggle) ---
        if (Keyboard.current[Key.F10].wasPressedThisFrame)
        {
            // ID "Firewall_B"를 가진 방화벽만 토글
            ToggleSpecificShutter("Firewall_B");
        }
    }

    /// <summary>
    /// 1. 정전 / 2. 단수 이벤트를 발동시킵니다.
    /// (Building 스크립트들에게 방송)
    /// </summary>
    public void TriggerEvent(string eventType, bool isActive)
    {
        // (CS0618 경고는 님의 환경에 맞추기 위해 의도된 것입니다)
        Building[] allBuildings = FindObjectsOfType<Building>();
        foreach (Building building in allBuildings)
        {
            building.HandleEvent(eventType, isActive);
        }
    }

    /// <summary>
    /// [수정됨] 3. '특정 ID'의 방화벽(셔터)을 토글(Toggle)합니다.
    /// (개별 Firewall 스크립트에게 명령)
    /// </summary>
    /// <param name="firewallId">제어할 방화벽의 고유 ID</param>
    public void ToggleSpecificShutter(string firewallId)
    {
        // 1. '전화번호부'에 이 ID가 등록되어 있는지 확인
        if (!firewallRegistry.ContainsKey(firewallId))
        {
            UnityEngine.Debug.LogWarning($"'{firewallId}' ID를 가진 방화벽을 찾을 수 없습니다!");
            return;
        }

        // 2. 현재 상태를 가져오고, 뒤집습니다.
        bool currentStateIsClosed = firewallStates[firewallId];
        bool newStateIsClosed = !currentStateIsClosed;

        // 3. '전화번호부'에서 실제 방화벽 오브젝트를 찾습니다.
        Firewall firewallToToggle = firewallRegistry[firewallId];

        // 4. 새 상태에 따라 '명령'을 내립니다.
        if (newStateIsClosed)
        {
            firewallToToggle.Close();
        }
        else
        {
            firewallToToggle.Open();
        }

        // 5. 새 상태를 저장합니다.
        firewallStates[firewallId] = newStateIsClosed;
        UnityEngine.Debug.Log($"--- [이벤트] 방화벽 '{firewallId}' 작동! (새 상태: {newStateIsClosed}) ---");
    }
}