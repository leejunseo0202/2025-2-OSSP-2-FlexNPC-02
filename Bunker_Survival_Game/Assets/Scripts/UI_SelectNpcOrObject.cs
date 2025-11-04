using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SelectObject : MonoBehaviour, IPointerDownHandler
{
    private bool isClick = false;
    
    public UI_StatusObject obj = null;
    public UI_StatusNPC npc = null;
    public int ObjectNumber; // 1 : REFRIGERATOR, 2 : WATERTANK, 
    public int NpcNumber; 

    // 버튼 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        isClick = true;

        //Object 버튼이 눌렸을 때
        if (obj != null)
        {
            switch (ObjectNumber)
            {
                case TestObject.REFRIGERATOR:
                    obj.selectedObject = obj.objectlist[1];
                    obj.objectNameText.text = "Refrigerator";
                    break;
                case TestObject.WATERTANK:
                    obj.selectedObject = obj.objectlist[2];
                    obj.objectNameText.text = "WaterTank";
                    break;
                default:
                    break;
            }
        }
        //NPC 버튼이 눌렸을 때
        else if (npc != null)
        {
            switch (NpcNumber)
            {
                case Test.NPC1:
                    npc.selectedNPC = npc.npclist[1];
                    npc.npcNameText.text = "NPC1";
                    break;
                case Test.NPC2:
                    npc.selectedNPC = npc.npclist[2];
                    npc.npcNameText.text = "NPC2";
                    break;
                default:
                    break;
            }
        }


    }

    // 버튼 떼었을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        isClick = false;
    }
}
