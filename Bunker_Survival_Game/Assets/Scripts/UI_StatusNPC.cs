using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class UI_StatusNPC : MonoBehaviour
{
    public const int HUNGER  = 0;
    public const int TOILET  = 1;
    public const int SOCIAL  = 2;
    public const int HYGIENE = 3;
    public const int FUN     = 4;
    public const int ENERGY  = 5;

    public RectTransform[] guage = new RectTransform[6];
    public Image[] guageImage = new Image[6];
    public TMPro.TMP_Text npcNameText;

    public Test selectedNPC = null;
    public Test[] npclist = new Test[4];

    public float maxWidth = 400f;

    //UI 활성화 관련 변수
    public Animator animator;
    public Button openButton;
    public Button closeButton;
    public GameObject ui_SelectNPCGroup;
    public GameObject ui_ShowGuageGroup;

    void Start()
    {
        for(int i= 0; i < guage.Length; i++)
            if (guageImage[i] == null && guage[i] != null)
                guageImage[i] = guage[i].GetComponent<Image>();    
    }

    void Update()
    {
        //클릭한 NPC 선택
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 제목 텍스트 업데이트
                Test npcStatus = hit.collider.GetComponent<Test>();
                if (npcStatus != null) {
                    selectedNPC = npcStatus;
                    npcNameText.text = selectedNPC.name;
                }
                // 애니메이션 재생
                if (animator != null)
                    animator.Play("OpenStatusNPC");
                //버튼 활성화 설정
                if (openButton != null && closeButton != null)
                {
                    openButton.gameObject.SetActive(false);
                    closeButton.gameObject.SetActive(true);
                }
                //UI 그룹 활성화 설정
                if (ui_SelectNPCGroup != null && ui_ShowGuageGroup != null)
                {
                    ui_SelectNPCGroup.SetActive(false);
                    ui_ShowGuageGroup.SetActive(true);
                }
            }
        }

        if (selectedNPC != null)
            UpdateGuage(selectedNPC);
    }

    public void UpdateGuage(Test npcStatus)
    {
        float[] statusValue = new float[6];
        for (int i=0; i < guage.Length; i++)
        {
            statusValue[i] = npcStatus.NPCstatus[i];

            float clamped = Mathf.Clamp(statusValue[i], 0, 100);
            float newWidth = (clamped / 100f) * maxWidth;

            Vector2 size = guage[i].sizeDelta;
            size.x = Mathf.Lerp(size.x, newWidth, Time.deltaTime * 5f);
            guage[i].sizeDelta = size;

            if (guageImage[i] != null)
                guageImage[i].color = Color.Lerp(Color.red, Color.green, clamped / 100f);
        }
    }
}
