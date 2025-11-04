using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ChangeItemValue : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public TMP_Text targetText;
    public TMP_Text totalValue;
    public bool isIncreaseButton = true;
    public int npcNumber = 0; // 1~4
    public UI_StatusObject objValue;

    public float repeatRate = 0.1f;     // 숫자 증가 간격
    private bool isClick = false;

    public int[] sharedObject = new int[5];
    // 버튼 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        isClick = true;
        int direction = isIncreaseButton ? 1 : -1;
        StartCoroutine(HoldIncrease(direction));
    }

    // 버튼 떼었을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        isClick = false;
    }

    //+버튼, -버튼 길게 눌렀을 때 숫자 증가/감소
    private System.Collections.IEnumerator HoldIncrease(int direction)
    {
        // 선택된 오브젝트 변수 가져오기
        TestObject selectedObject = objValue.selectedObject;
        if (selectedObject == null)
            yield break;
        int[] sharedObject = selectedObject.sharedObject;

        while (isClick)
        {
            if (direction == 1 && sharedObject[0] > 0)
            {
                sharedObject[npcNumber]++;
                sharedObject[0]--;
            }
            else if (direction == -1 && sharedObject[npcNumber] > 0)
            {
                sharedObject[npcNumber]--;
                sharedObject[0]++;
            }

            yield return new WaitForSeconds(repeatRate);
        }
    }

}
