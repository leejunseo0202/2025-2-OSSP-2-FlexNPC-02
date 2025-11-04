using UnityEngine;

public class UI_OpenAndCloseStatus : MonoBehaviour
{
    public Animator animator;

    public void PlayOpen()
    {
        animator.SetTrigger("OpenTrigger");
    }
    public void PlayClose()
    {
        animator.SetTrigger("CloseTrigger");
    }
}
