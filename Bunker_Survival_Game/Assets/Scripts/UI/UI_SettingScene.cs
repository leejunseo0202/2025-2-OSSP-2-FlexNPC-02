using UnityEngine;
using UnityEngine.UI;

public class UI_GamePause : MonoBehaviour
{
    public Button pauseButton;     // 설정(일시정지) 버튼
    public Button resumeButton;    // 재개 버튼
    public Button mainMenuButton;  // 메인메뉴로 이동 버튼 (선택사항)

    // 게임 정지
    public void PauseGame()
    {
        Time.timeScale = 0f;        
    }

    // 게임 재개
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    // 메인메뉴로 이동
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }

    // 게임 종료
    public void QuitGame()
    {
        Application.Quit();
    }
}
