using JetBrains.Annotations;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class UI_StatusNPC : MonoBehaviour
{
    public RectTransform[] guage = new RectTransform[6];
    public Image[] guageImage = new Image[6];
    public TMPro.TMP_Text npcNameText;

    public Test selectedNPC = null;
    public Button[] npcButton;
    public TMPro.TMP_Text[] npcText;
    public Test[] npclist;

    //UI 활성화 관련 변수
    public Animator animator;
    public Button openButton;
    public Button closeButton;
    public GameObject ui_SelectNPCGroup;
    public GameObject ui_ShowGuageGroup;
    public bool isSelectNPC = false;
    private bool ui_Open = false;

    private float maxWidth = 400f;
    private int numberOfNPC = 0;
    private bool shift;

    // 설정 버튼
    public Button settingButton;

    void Start()
    {
        numberOfNPC = PlayerPrefs.GetInt("NumberOfNPC");
        npclist = new Test[numberOfNPC + 1];

        // NPC 수에 따라 NPC 생성
        for (int i=1; i <= numberOfNPC; i++)
        {
            Vector3 spawnPoint = new Vector3(0 + i, (float)0.5, 0);

            GameObject npcObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            npcObj.name = "NPC" + i;
            npcObj.transform.position = spawnPoint;

            // Test 스크립트 컴포넌트 추가
            Test newNpc = npcObj.AddComponent<Test>();

            npclist[i] = newNpc;

            //버튼 활성화
            if (npcButton != null && npcText != null)
            {
                npcButton[i].gameObject.SetActive(true);
                npcText[i].gameObject.SetActive(true);
                npcText[i].text = "NPC" + i;
            }
        }

        for (int i= 0; i < guage.Length; i++)
            if (guageImage[i] == null && guage[i] != null)
                guageImage[i] = guage[i].GetComponent<Image>();    
    }

    void Update()
    {
        shift = Keyboard.current.shiftKey.isPressed;
        if (Keyboard.current.digit1Key.wasPressedThisFrame && numberOfNPC >= 1 && !shift) SelectNPC(1);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && numberOfNPC >= 2 && !shift) SelectNPC(2);
        if (Keyboard.current.digit3Key.wasPressedThisFrame && numberOfNPC >= 3 && !shift) SelectNPC(3);
        if (Keyboard.current.digit4Key.wasPressedThisFrame && numberOfNPC >= 4 && !shift) SelectNPC(4);

        // back키
        if (Keyboard.current.bKey.wasPressedThisFrame && ui_ShowGuageGroup.activeSelf)
        {
            if (ui_SelectNPCGroup != null)
                ui_SelectNPCGroup.SetActive(true);

            if (ui_ShowGuageGroup != null)
                ui_ShowGuageGroup.SetActive(false);
        }

        // left 키를 눌러 NPC Status 열기/닫기 토글
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            if (ui_Open)
            {
                closeButton.onClick.Invoke();
                ui_Open = false;
            }
            else
            {
                openButton.onClick.Invoke();
                ui_Open = true;
            }
        }

        // 설정버튼
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            settingButton.onClick.Invoke();

        }

        if (selectedNPC != null)
            UpdateGuage(selectedNPC);
    }

    private void SelectNPC(int index)
    {
        selectedNPC = npclist[index];

        // 제목 텍스트 업데이트
        npcNameText.text = selectedNPC.name;

        // 애니메이션 재생
        if (animator != null && !ui_Open)
        {
            openButton.onClick.Invoke();
            ui_Open = true;
        }
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

    private void UpdateGuage(Test npcStatus)
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
