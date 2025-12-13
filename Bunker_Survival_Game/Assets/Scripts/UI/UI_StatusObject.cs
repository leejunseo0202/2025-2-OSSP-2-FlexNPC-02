using Mono.Cecil;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_StatusObject : MonoBehaviour
{
    private const int FOOD = 0;
    private const int WATER = 1;
    private const int ENERGY = 2;

    private string[] resourcelist = new string[4];
    public int selectedResource;

    public ResourceManager resource;

    //UI 활성화 관련 변수
    public Animator animator;
    public Button openButton;
    public Button closeButton;
    public GameObject ui_SelectItemGroup;
    public GameObject ui_SharedItemGroup;
    public TMPro.TMP_Text resouceText;
    public TMPro.TMP_Text[] countText;
    public TMPro.TMP_Text totalText;
    public TMPro.TMP_Text npcName;

    public GameObject[] ui_NPCGroup;
    public GameObject ui_Shared;
    public Button[] upButton = new Button[5];
    public Button[] downButton = new Button[5];
    private bool ui_Open = false;
    private bool shift;
    private int numberOfNPC;
    private int selectedNPC = 1;

    void Start()
    {
        numberOfNPC = PlayerPrefs.GetInt("NumberOfNPC");

        //버튼 활성화
        for(int i = 1; i<= numberOfNPC; i++)
            ui_NPCGroup[i].gameObject.SetActive(true);

        resourcelist[0] = "Food";
        resourcelist[1] = "Water";
        resourcelist[2] = "Energy";
    }

    void Update()
    {
        // 자원 선택
        shift = Keyboard.current.shiftKey.isPressed;
        if (!ui_Shared.activeSelf)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame && shift) SelectResource("Food");
            if (Keyboard.current.digit2Key.wasPressedThisFrame && shift) SelectResource("Water");
            if (Keyboard.current.digit3Key.wasPressedThisFrame && shift) SelectResource("Energy");
        }

        // shared resource 변경
        if (ui_Shared.activeSelf)
        {
            // npc 선택
            if (Keyboard.current.digit1Key.wasPressedThisFrame && shift)    ChangeSharedResource(1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame && shift && numberOfNPC >= 2)    ChangeSharedResource(2);
            if (Keyboard.current.digit3Key.wasPressedThisFrame && shift && numberOfNPC >= 3)    ChangeSharedResource(3);
            if (Keyboard.current.digit4Key.wasPressedThisFrame && shift && numberOfNPC >= 4)    ChangeSharedResource(4);

            // 선택된 npc의 자원 변경
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                UI_ChangeItemValue btnScript = upButton[selectedNPC].GetComponent<UI_ChangeItemValue>();
                if (btnScript != null)
                {
                    btnScript.OnPointerDown(eventData);
                }
            }
            if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                UI_ChangeItemValue btnScript = upButton[selectedNPC].GetComponent<UI_ChangeItemValue>();
                if (btnScript != null)
                {
                    btnScript.OnPointerUp(eventData);
                }
            }

            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                UI_ChangeItemValue btnScript = downButton[selectedNPC].GetComponent<UI_ChangeItemValue>();
                if (btnScript != null)
                {
                    btnScript.OnPointerDown(eventData);
                }
            }
            if (Keyboard.current.downArrowKey.wasReleasedThisFrame)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                UI_ChangeItemValue btnScript = downButton[selectedNPC].GetComponent<UI_ChangeItemValue>();
                if (btnScript != null)
                {
                    btnScript.OnPointerUp(eventData);
                }
            }
        }

        // shift back키
        if (Keyboard.current.bKey.wasPressedThisFrame && ui_SharedItemGroup.activeSelf && shift)
        {
            if (ui_SelectItemGroup != null)
                ui_SelectItemGroup.SetActive(true);

            if (ui_SharedItemGroup != null)
                ui_SharedItemGroup.SetActive(false);
        }

        // right 키를 눌러 NPC Status 열기/닫기 토글
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
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

        UpdateObjectValue(selectedResource);
    }

    private void SelectResource(string resource)
    {
        selectedResource = System.Array.IndexOf(resourcelist, resource);
        // 제목 텍스트 업데이트
        resouceText.text = resource;

        // 애니메이션 재생
        if (animator != null)
            animator.Play("OpenStatusObject");
        //버튼 활성화 설정
        if (openButton != null && closeButton != null)
        {
            openButton.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(true);
        }
        //UI 그룹 활성화 설정
        if (ui_SharedItemGroup != null && ui_SelectItemGroup != null)
        {
            ui_SelectItemGroup.SetActive(false);
            ui_SharedItemGroup.SetActive(true);
        }
    }

    //shared resource 변경
    public void ChangeSharedResource(int npcNum)
    {
        npcName.text = "NPC " + npcNum;
        selectedNPC = npcNum;

    }

    // Resource Value 업데이트
    public void UpdateObjectValue(int resourceNum)
    {
        Debug.Log("UpdateObjectValue called for resourceNum: " + resourceNum);
        for (int i = 1; i <= numberOfNPC; i++)
            countText[i].text = resource.sharedStock[resourceNum+1 + (i-1) * 3].ToString();
        totalText.text = resource.currentStock[resourceNum].ToString();
    }

    public void SetResourceName(string rName)
    {
        resouceText.text = rName;
    }
}
