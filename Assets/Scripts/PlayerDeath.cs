using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("Player Setup")]
    public int playerNumber = 1;

    private bool isDead = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isDead) return;
        if (GameManager.Instance.IsGameOver) return;

        float bottomEdge = mainCamera.transform.position.y
                         - mainCamera.orthographicSize - 1f;
        if (transform.position.y < bottomEdge)
            Die();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Asteroid")) return;

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeHit();
        else
            Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        gameObject.SetActive(false);
        GameManager.Instance.TriggerPlayerDeath(playerNumber);
    }
}