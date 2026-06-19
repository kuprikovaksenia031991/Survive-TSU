using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string firstSceneName = "IntroScene";  

    public void StartGame()
    {
        Debug.Log("Запуск игры!");
        SceneManager.LoadScene(firstSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}