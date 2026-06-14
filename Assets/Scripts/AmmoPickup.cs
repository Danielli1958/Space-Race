using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Settings")]
    public int ammoAmount = 2;
    public float bobSpeed = 2.5f;
    public float bobHeight = 0.15f;
    public float rotateSpeed = -60f;    // Negative = counter-clockwise spin

    private float startY;
    private Camera mainCamera;

    void Start()
    {
        startY = transform.position.y;
        mainCamera = Camera.main;
    }

    void Update()
    {
        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, 0f);
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        float bottomEdge = mainCamera.transform.position.y
                         - mainCamera.orthographicSize - 3f;
        if (transform.position.y < bottomEdge)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerShooter shooter = other.GetComponent<PlayerShooter>();
        if (shooter == null) return;

        shooter.AddAmmo(ammoAmount);
        Destroy(gameObject);
    }
}