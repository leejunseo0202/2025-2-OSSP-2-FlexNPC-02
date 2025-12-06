using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonMover : MonoBehaviour
{
    public Animator animator1;
    public Animator animator2;
    public Button button;
    public TMPro.TMP_Text text_Start;
    private bool isMoved = true;

    void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        if(isMoved == false)
        {
            //text 활성화 설정
            if (text_Start != null)
            {
                text_Start.text = "START";
                text_Start.fontSize = 60;
            }

        }
        //Number Of NPC 활성화
        else
        {
            //text 활성화 설정
            if (text_Start != null)
            {
                text_Start.text = "Number Of\nNPC";
                text_Start.fontSize = 50;
            }
        }

        isMoved = !isMoved;
        animator1.SetBool("isMoved", isMoved);
        animator2.SetBool("isMoved", isMoved);
    }
}
