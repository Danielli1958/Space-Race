using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;           // Shows winner or "game over"

    [Header("Players")]
    public GameObject player1Object;
    public GameObject player2Object;        // Leave empty for single-player

    public bool IsGameOver { get; private set; } = false;

    private bool player1Dead = false;
    private bool player2Dead = false;
    private bool singlePlayer => player2Object == null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Called by each PlayerDeath script independently
    public void TriggerPlayerDeath(int playerNumber)
    {
        if (playerNumber == 1) player1Dead = true;
        if (playerNumber == 2) player2Dead = true;

        // Disable the dead player's movement and visuals
        GameObject deadPlayer = playerNumber == 1 ? player1Object : player2Object;
        if (deadPlayer != null)
            deadPlayer.SetActive(false);

        // Single player: any death ends the game
        if (singlePlayer)
        {
            EndGame("Game Over");
            return;
        }

        // 2-player: only end when both are dead, or declare winner when one dies
        if (player1Dead && player2Dead)
        {
            EndGame("Draw!");
        }
        else if (player1Dead)
        {
            EndGame("Player 2 Wins!");
        }
        else if (player2Dead)
        {
            EndGame("Player 1 Wins!");
        }
    }

    // Legacy support — called if anything still uses TriggerGameOver()
    public void TriggerGameOver()
    {
        EndGame("Game Over");
    }

    void EndGame(string message)
    {
        if (IsGameOver) return;
        IsGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.text = message;
            }
        }
    }

    public void RestartGame()
    {
        IsGameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    public void ReturnToMainMenu()
    {
        IsGameOver = false;
        SceneManager.LoadScene("MenuScene");
    }
}