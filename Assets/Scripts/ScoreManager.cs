using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text scoreText;

    public float Score { get; private set; } = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        float speedMultiplier = ScrollManager.Instance != null ? ScrollManager.Instance.CurrentSpeed : 1f;
        Score += Time.deltaTime * speedMultiplier;

        if (scoreText != null)
            scoreText.text = "Score: " + Score.ToString("0");
    }
}