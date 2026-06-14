using UnityEngine;

public class ScrollManager : MonoBehaviour
{
    public static ScrollManager Instance { get; private set; }

    [Header("Speed Settings")]
    public float startSpeed = 4f;
    public float acceleration = 0.05f;
    public float maxSpeed = 15f;

    public float CurrentSpeed { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        CurrentSpeed = startSpeed;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        CurrentSpeed = Mathf.Min(CurrentSpeed + acceleration * Time.deltaTime, maxSpeed);
    }
}