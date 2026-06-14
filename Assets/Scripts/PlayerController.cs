using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Vertical Recovery")]
    public float recoverySpeed = 1f;    // How fast the player creeps back up
    public float homeY = -4f;           // Y position the player recovers toward

    [Header("Screen Bounds")]
    public float horizontalPadding = 0.1f;

    private Camera mainCamera;
    private float screenLeftBound;
    private float screenRightBound;
    private Rigidbody2D rb;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        UpdateScreenBounds();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        HandleHorizontalMovement();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver) return;
        HandleVerticalRecovery();
    }

    void HandleHorizontalMovement()
    {
        float input = Input.GetAxis("Horizontal");

        Vector3 newPosition = transform.position;
        newPosition.x += input * moveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, screenLeftBound, screenRightBound);

        transform.position = newPosition;
    }

    void HandleVerticalRecovery()
    {
        // Only recover upward — never fight against a block pushing the player down
        if (transform.position.y < homeY)
        {
            // Nudge the rigidbody upward, but cap it so it doesn't overshoot homeY
            float newY = Mathf.MoveTowards(transform.position.y, homeY, recoverySpeed * Time.fixedDeltaTime);
            rb.MovePosition(new Vector2(transform.position.x, newY));
        }
    }

    void UpdateScreenBounds()
    {
        float leftEdge  = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        float rightEdge = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
        float playerRadius = GetComponent<CircleCollider2D>().radius * transform.localScale.x;

        screenLeftBound  = leftEdge  + playerRadius + horizontalPadding;
        screenRightBound = rightEdge - playerRadius - horizontalPadding;
    }

    void OnDrawGizmosSelected()
    {
        if (mainCamera == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(screenLeftBound, -10, 0), new Vector3(screenLeftBound,  10, 0));
        Gizmos.DrawLine(new Vector3(screenRightBound, -10, 0), new Vector3(screenRightBound, 10, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-10, homeY, 0), new Vector3(10, homeY, 0));
    }
}