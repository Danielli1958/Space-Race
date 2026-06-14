using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Setup")]
    public int playerNumber  = 1;

    [Header("HP Settings")]
    public int maxHP         = 5;
    public int absoluteMaxHP = 10;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;

    [Header("UI")]
    public StatusBar healthBar;

    public int  CurrentHP    { get; private set; }
    public bool IsInvincible { get; private set; } = false;

    private float          invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        CurrentHP      = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (healthBar != null)
            healthBar.Init(maxHP, CurrentHP);
    }

    void Update()
    {
        if (!IsInvincible) return;

        invincibilityTimer -= Time.deltaTime;
        float flash = Mathf.Sin(Time.time * 20f);
        spriteRenderer.color = flash > 0 ? Color.white : new Color(1f, 1f, 1f, 0.2f);

        if (invincibilityTimer <= 0f)
        {
            IsInvincible         = false;
            spriteRenderer.color = Color.white;
        }
    }

    public void TakeHit()
    {
        if (IsInvincible) return;
        if (GameManager.Instance.IsGameOver) return;

        CurrentHP--;

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            // Never change baseMax on damage — pass -1 to leave it unchanged
            healthBar?.SetValue(CurrentHP);
            GetComponent<PlayerDeath>()?.Die();
        }
        else
        {
            // Never change baseMax on damage — pass -1 to leave it unchanged
            healthBar?.SetValue(CurrentHP);
            IsInvincible       = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    public void Heal(int amount)
    {
        if (GameManager.Instance.IsGameOver) return;

        CurrentHP = Mathf.Min(CurrentHP + amount, absoluteMaxHP);

        // Never pass newBaseMax — baseMax is always fixed at maxHP (5)
        // StatusBar.VisibleSegCount handles overheal segments automatically
        healthBar?.SetValue(CurrentHP);
    }
}