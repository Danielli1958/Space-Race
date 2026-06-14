using UnityEngine;
using UnityEngine.SceneManagement; // Required for switching scenes

public class MainMenu : MonoBehaviour
{
    // Call this to load the first level of your game
    public void PlayGame()
    {
        // "GameScene" must match your gameplay scene name exactly in Build Settings
        SceneManager.LoadScene("GameScene");
    }
    public void HowToPlay()
    {
        SceneManager.LoadScene("InfoScene");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

}
