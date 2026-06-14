using UnityEngine;

public class RocketController : MonoBehaviour
{
    [Header("Player Setup")]
    public int playerNumber = 1;

    [Header("Movement")]
    public float strafeSpeed = 5f;

    [Header("Boost")]
    public float boostExtraSpeed = 4f;
    public float boostDuration = 0.3f;
    public float boostCooldown = 0.5f;

    [Header("Tilt")]
    public float maxTiltAngle = 25f;
    public float tiltSpeed = 8f;

    [Header("Screen Bounds")]
    public float horizontalPadding = 0.2f;
    public float playerHalfWidth = 0.3f;

    private Camera mainCamera;
    private Rigidbody2D rb;
    private float boostTimer = 0f;
    private float cooldownTimer = 0f;
    private float currentTilt = 0f;
    private float horizontalInput = 0f;

    public bool IsBoosting => boostTimer > 0f;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        transform.SetParent(null);
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        ReadInput();
        HandleBoost();
        HandleTilt();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        HandleBoostTimers();
        ApplyMovement();
        EnforceHorizontalBounds();
    }

    void ReadInput()
    {
        if (playerNumber == 1)
        {
            if (Input.GetKey(KeyCode.A))           horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.D))      horizontalInput =  1f;
            else                                   horizontalInput =  0f;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow))        horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.RightArrow))  horizontalInput =  1f;
            else                                        horizontalInput =  0f;
        }
    }

    void HandleBoostTimers()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.fixedDeltaTime;
        if (boostTimer    > 0f) boostTimer    -= Time.fixedDeltaTime;
    }

    void HandleBoost()
    {
        bool boostPressed = playerNumber == 1
            ? Input.GetKeyDown(KeyCode.W)
            : Input.GetKeyDown(KeyCode.UpArrow);

        if (boostPressed && cooldownTimer <= 0f)
        {
            boostTimer    = boostDuration;
            cooldownTimer = boostCooldown;
        }
    }

    void ApplyMovement()
    {
        float scrollSpeed = ScrollManager.Instance != null
            ? ScrollManager.Instance.CurrentSpeed : 4f;

        float verticalSpeed = IsBoosting
            ? scrollSpeed + boostExtraSpeed
            : scrollSpeed;

        rb.MovePosition(rb.position + new Vector2(
            horizontalInput * strafeSpeed * Time.fixedDeltaTime,
            verticalSpeed * Time.fixedDeltaTime
        ));
    }

    void EnforceHorizontalBounds()
    {
        float halfWidth  = mainCamera.orthographicSize * mainCamera.aspect;
        float camX       = mainCamera.transform.position.x;
        float leftBound  = camX - halfWidth + playerHalfWidth + horizontalPadding;
        float rightBound = camX + halfWidth - playerHalfWidth - horizontalPadding;

        Vector2 pos = rb.position;
        if (pos.x < leftBound || pos.x > rightBound)
            rb.MovePosition(new Vector2(Mathf.Clamp(pos.x, leftBound, rightBound), pos.y));
    }

    void HandleTilt()
    {
        float targetTilt = -horizontalInput * maxTiltAngle;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentTilt);
    }
}