using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Clicked_NumberOfNPC : MonoBehaviour
{
    public int numberOfNPC;
    
    public void OnButtonClick()
    {
        PlayerPrefs.SetInt("NumberOfNPC", numberOfNPC);
        PlayerPrefs.Save();
        Debug.Log("Number of NPC selected: " + numberOfNPC);

        SceneManager.LoadScene("PlayScene");
    }
}
