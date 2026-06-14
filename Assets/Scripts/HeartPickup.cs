using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [Header("Settings")]
    public int healAmount = 1;
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;
    public float rotateSpeed = 90f;

    private float startY;
    private Camera mainCamera;

    void Start()
    {
        startY = transform.position.y;
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Bob up and down
        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, 0f);

        // Spin
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // Destroy when off the bottom of the screen
        float bottomEdge = mainCamera.transform.position.y
                         - mainCamera.orthographicSize - 3f;
        if (transform.position.y < bottomEdge)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        health.Heal(healAmount);
        Destroy(gameObject);
    }
}