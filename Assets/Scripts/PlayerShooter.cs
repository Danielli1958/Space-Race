using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Player Setup")]
    public int playerNumber = 1;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public int   startingAmmo  = 5;
    public float fireCooldown  = 0.4f;
    public Vector2 bulletSpawnOffset = new Vector2(0f, 0.6f);

    [Header("UI")]
    public StatusBar ammoBar;       // Assign the smaller StatusBar in Inspector

    public int CurrentAmmo { get; private set; }

    private float cooldownTimer = 0f;

    void Start()
    {
        CurrentAmmo = startingAmmo;
        if (ammoBar != null)
            ammoBar.Init(startingAmmo, CurrentAmmo);
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        bool firePressed = playerNumber == 1
            ? Input.GetKeyDown(KeyCode.S)
            : Input.GetKeyDown(KeyCode.DownArrow);

        if (firePressed) TryFire();
    }

    void TryFire()
    {
        if (CurrentAmmo <= 0)   return;
        if (cooldownTimer > 0f) return;

        Vector3 spawnPos = transform.position
                         + transform.up    * bulletSpawnOffset.y
                         + transform.right * bulletSpawnOffset.x;

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null) bullet.playerOwner = playerNumber;

        CurrentAmmo--;
        cooldownTimer = fireCooldown;
        ammoBar?.SetValue(CurrentAmmo);
    }

    public void AddAmmo(int amount)
    {
        int prev = CurrentAmmo;
        CurrentAmmo += amount;
        ammoBar?.SetValue(CurrentAmmo);
    }
}