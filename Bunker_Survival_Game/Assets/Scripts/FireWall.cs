using UnityEngine;

/// <summary>
/// 개별 방화벽(셔터) 오브젝트에 부착됩니다.
/// 자신의 Animator와 Blocker를 스스로 제어합니다.
/// </summary>
public class Firewall : MonoBehaviour
{
    [Header("1. 고유 ID")]
    [Tooltip("EventManager가 이 방화벽을 찾는 데 사용할 고유 ID (예: 'Firewall_A', 'Sector_B_Gate')")]
    public string firewallId;

    [Header("2. 컴포넌트")]
    [Tooltip("이 방화벽의 Animator")]
    [SerializeField] private Animator myAnimator;
    [Tooltip("이 방화벽의 길막용 콜라이더")]
    [SerializeField] private GameObject myBlocker;

    // 스크립트가 시작될 때, 혹시 컴포넌트 연결이 안 됐으면 스스로 찾음
    void Awake()
    {
        if (myAnimator == null)
        {
            myAnimator = GetComponentInChildren<Animator>();
        }
        if (myBlocker == null)
        {
            // 자식 오브젝트 중에 "Blocker"라는 이름의 오브젝트를 찾아봄
            Transform blockerT = transform.Find("Blocker");
            if (blockerT != null)
            {
                myBlocker = blockerT.gameObject;
            }
        }

        // 유효성 검사
        if (string.IsNullOrEmpty(firewallId))
        {
            UnityEngine.Debug.LogError($"'{this.gameObject.name}'의 firewallId가 비어있습니다!", this.gameObject);
        }
    }

    /// <summary>
    /// 이 방화벽을 '닫습니다'.
    /// </summary>
    public void Close()
    {
        if (myAnimator != null) myAnimator.SetBool("isClosed", true);
        if (myBlocker != null) myBlocker.SetActive(true);
    }

    /// <summary>
    /// 이 방화벽을 '엽니다'.
    /// </summary>
    public void Open()
    {
        if (myAnimator != null) myAnimator.SetBool("isClosed", false);
        if (myBlocker != null) myBlocker.SetActive(false);
    }
}