using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f;
    public int playerOwner = 1;         // Which player fired this bullet

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Bullets always travel straight upward in world space
        transform.position += Vector3.up * speed * Time.deltaTime;

        // Destroy when off the top of the screen
        float topEdge = mainCamera.transform.position.y
                      + mainCamera.orthographicSize + 3f;
        if (transform.position.y > topEdge)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // --- Hit an asteroid ---
        if (other.CompareTag("Asteroid"))
        {
            DestroyAsteroidCluster(other);
            Destroy(gameObject);
            return;
        }

        // --- Hit the other player ---
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null && health.playerNumber != playerOwner)
        {
            health.TakeHit();
            Destroy(gameObject);
            return;
        }
    }

    void DestroyAsteroidCluster(Collider2D hitAsteroid)
    {
        // Destroy the hit asteroid plus all asteroids directly touching it
        // Use OverlapCircle to find all colliders in contact radius
        float radius = hitAsteroid.bounds.extents.x * 2.5f;
        Collider2D[] neighbours = Physics2D.OverlapCircleAll(
            hitAsteroid.transform.position, radius
        );

        foreach (Collider2D col in neighbours)
        {
            if (col.CompareTag("Asteroid"))
                Destroy(col.gameObject);
        }
    }
}