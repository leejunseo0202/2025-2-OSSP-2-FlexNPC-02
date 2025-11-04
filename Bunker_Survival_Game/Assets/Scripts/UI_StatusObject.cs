using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_StatusObject : MonoBehaviour
{
    public TMPro.TMP_Text objectNameText;
    public TMPro.TMP_Text[] objectText = new TMPro.TMP_Text[5];

    public TestObject selectedObject = null;
    public TestObject[] objectlist = new TestObject[6];

    //UI 활성화 관련 변수
    public Animator animator;
    public Button openButton;
    public Button closeButton;
    public GameObject ui_SelectItemGroup;
    public GameObject ui_SharedItemGroup;


    void Update()
    {
        // 클릭한 Object 선택
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TestObject npcStatus = hit.collider.GetComponent<TestObject>();
                if (npcStatus != null)
                {
                    // 제목 텍스트 업데이트
                    selectedObject = npcStatus;
                    objectNameText.text = selectedObject.name;

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
            }
        }

        if (selectedObject != null)
            UpdateObjectValue(selectedObject);
    }

    // Object Value 업데이트
    public void UpdateObjectValue(TestObject obj)
    {
        objectText[0].text = obj.sharedObject[TestObject.TOTAL].ToString();
        for (int i = 1; i < objectText.Length; i++)
            objectText[i].text = obj.sharedObject[i].ToString();
    }
}
