using System.Resources;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ChangeItemValue : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isIncreaseButton = true;
    public int npcNumber = 0; // 1~4
    public UI_StatusObject statusObject;
    
    public ResourceManager resource;
    private int[,] sharedStock = new int[3, 5];


    public float repeatRate = 0.1f;     // 숫자 증가 간격
    private bool isClick = false;

    void Start()
    {
        sharedStock = resource.sharedStock;
    }

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
        int resourceNum = statusObject.selectedResource;

        while (isClick)
        {
            if (direction == 1 && resource.maxStock[resourceNum] > 0 && resource.currentStock[resourceNum] > 0)
            {
                sharedStock[resourceNum, npcNumber]++;
                resource.currentStock[resourceNum]--;
            }
            else if (direction == -1 && sharedStock[resourceNum, npcNumber] > 0 && resource.currentStock[resourceNum] > 0)
            {
                sharedStock[resourceNum, npcNumber]--;
                resource.currentStock[resourceNum]++;
            }

            yield return new WaitForSeconds(repeatRate);
        }
    }

}
