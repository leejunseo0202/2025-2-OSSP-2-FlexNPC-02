using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// "단전", "단수", "방화벽" 등 벙커의 위기 이벤트를 관리하고 발동시킵니다.
/// (테스트용 키보드 입력을 포함합니다)
/// </summary>
public class EventManager : MonoBehaviour
{
    [Header("3번 기능: 방화벽/셔터")]
    [Tooltip("방화벽(셔터)의 Animator 컴포넌트를 연결하세요.")]
    public Animator shutterAnimator;

    [Tooltip("방화벽이 닫혔을 때 NPC의 길을 막을 '물리적 장애물' 오브젝트 (예: Box Collider)")]
    public GameObject shutterBlocker;

    // --- 이벤트 현재 상태 ---
    private bool isPowerOut = false;    // 현재 정전 상태인가?
    private bool isWaterOut = false;    // 현재 단수 상태인가?
    private bool isShutterClosed = false; // 현재 셔터가 닫혔나?

    void Start()
    {
        // 시작 시: 모든 것이 정상 상태
        if (shutterBlocker != null)
        {
            shutterBlocker.SetActive(false); // 길막기 비활성화
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

            // [수정됨: CS0104 에러 해결]
            UnityEngine.Debug.Log(isPowerOut ? "--- [이벤트] 정전 발생! ---" : "--- [이벤트] 전력 복구됨 ---");
        }

        // F8키: 단수 (Toggle)
        if (Keyboard.current[Key.F8].wasPressedThisFrame)
        {
            isWaterOut = !isWaterOut; // 상태 뒤집기
            TriggerEvent(GameDefinitions.EventType.WaterOutage, !isWaterOut);

            // [수정됨: CS0104 에러 해결]
            UnityEngine.Debug.Log(isWaterOut ? "--- [이벤트] 단수 발생! ---" : "--- [이벤트] 급수 복구됨 ---");
        }

        // F9키: 방화벽 (Toggle)
        if (Keyboard.current[Key.F9].wasPressedThisFrame)
        {
            isShutterClosed = !isShutterClosed; // 상태 뒤집기
            TriggerShutter(isShutterClosed);

            // [수정됨: CS0104 에러 해결]
            UnityEngine.Debug.Log(isShutterClosed ? "--- [이벤트] 방화벽 작동! ---" : "--- [이벤트] 방화벽 개방됨 ---");
        }
    }

    /// <summary>
    /// 1. 정전 / 2. 단수 이벤트를 발동시킵니다.
    /// 맵의 모든 'Building' 자식 스크립트를 찾아 상태를 변경합니다.
    /// </summary>
    /// <param name="eventType">GameDefinitions에 정의된 이벤트 타입</param>
    /// <param name="isActive">이벤트가 끝났는지(true), 시작됐는지(false)</param>
    public void TriggerEvent(string eventType, bool isActive)
    {
        // 1. 맵에 있는 모든 'Building' (부모) 컴포넌트를 찾습니다.

        // --- [수정됨: CS0103 에러를 해결하기 위해 구버전 함수로 복귀] ---
        // 'FindObjectsByType' (신버전)이 님의 환경에서 CS0103 에러를 유발하므로,
        // (CS0618 'obsolete' 경고가 뜨더라도) 이전 버전인 'FindObjectsOfType'을 사용합니다.
        Building[] allBuildings = FindObjectsOfType<Building>();
        // --- [여기까지] ---

        // 2. 모든 건물에게 이벤트를 '방송(Broadcast)'합니다.
        foreach (Building building in allBuildings)
        {
            building.HandleEvent(eventType, isActive);
        }
    }

    /// <summary>
    /// 3. 방화벽(셔터) 이벤트를 발동시킵니다.
    /// </summary>
    /// <param name="isClosed">셔터를 닫을지(true) 열지(false)</param>
    public void TriggerShutter(bool isClosed)
    {
        // 1. (연출) Animator의 "isClosed" 파라미터를 변경하여 애니메이션 재생
        if (shutterAnimator != null)
        {
            shutterAnimator.SetBool("isClosed", isClosed);
        }

        // 2. (기능) NPC의 길을 막는 물리적 장애물(콜라이더)을 활성화/비활성화
        if (shutterBlocker != null)
        {
            shutterBlocker.SetActive(isClosed);

            // (팁: 이 'shutterBlocker'는 NPC의 NavMesh(길찾기)에도
            // '장애물'로 인식되도록 설정해야 합니다.)
        }
    }
}