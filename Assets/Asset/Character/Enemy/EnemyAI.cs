using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyLevel
    {
        Level1_NinjaFrog,   // Level 1 (Hijau): Melee patroli/tabrak (HP kecil)
        Level2_PinkMan,     // Level 2 (Putih): Tembak pistol jarak menengah (HP sedang)
        Level3_VirtualGuy   // Level 3 (Biru): Tembak burst/cepat & agresif kejar player (HP tebal)
    }

    [Header("Enemy Level Configuration")]
    [SerializeField] private EnemyLevel level = EnemyLevel.Level1_NinjaFrog;
    [SerializeField] private int maxHealth = 3;

    [Header("Movement & Patrol")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallCheckDistance = 0.4f;
    [SerializeField] private float ledgeCheckDistance = 0.8f;
    [SerializeField] private float ledgeForwardOffset = 0.35f;

    [Header("Combat & Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float stopShootingDistance = 4f; // Jarak berhenti agar tidak menabrak player saat menembak
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float contactDamageCooldown = 1f;

    [Header("Ranged Attack (Level 2 & 3)")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private float fireRate = 1.8f;
    [SerializeField] private int burstCount = 3;       // Jumlah peluru per burst (Level 3)
    [SerializeField] private float burstInterval = 0.15f; // Jeda antar peluru dalam 1 burst

    [Header("Visual & Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("New Sprite Animator (NewEnemy)")]
    [Tooltip("Isi jika pakai sprite NewEnemy. Kosongkan jika pakai sprite lama.")]
    [SerializeField] private NewEnemyAnimController newAnimController;

    [Header("Pistol Object (Kosongkan untuk NewEnemy)")]
    [Tooltip("GameObject senjata pistol yang mengorbit. Set null/unassign untuk NewEnemy.")]
    [SerializeField] private GameObject pistolObject;

    private Rigidbody2D rb;
    private EnemyHealth healthComponent;
    private Transform playerTransform;

    private bool movingRight = true;
    private float nextFireTime;
    private float lastContactDamageTime = -100f;
    private bool isBurstShooting = false;
    private bool isPlayerDetected = false;

    public EnemyLevel Level => level;
    public bool IsPlayerEngaged => isPlayerDetected;
    public bool IsFacingRight => movingRight;

    public void Configure(EnemyLevel lvl, int hp, float pSpeed, float cSpeed, float detectRange, float stopDist, int cDamage)
    {
        level = lvl;
        maxHealth = hp;
        patrolSpeed = pSpeed;
        chaseSpeed = cSpeed;
        detectionRange = detectRange;
        stopShootingDistance = stopDist;
        contactDamage = cDamage;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthComponent = GetComponent<EnemyHealth>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Auto-detect animator bridge — cari di GameObject ini dan semua children
        if (newAnimController == null)
            newAnimController = GetComponentInChildren<NewEnemyAnimController>();

        // Default groundLayer jika belum diset di inspector (Layer 6: Ground, mask = 64)
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
            if (groundLayer.value == 0) groundLayer = 1 << 6;
        }

        // Auto-find child "EnemyWeapon" jika pistolObject belum diassign manual
        if (pistolObject == null)
        {
            Transform found = transform.Find("EnemyWeapon");
            if (found != null) pistolObject = found.gameObject;
        }

        // Sembunyikan VISUAL pistol saja (SpriteRenderer) — EnemyPistol tetap aktif untuk tembak
        if (pistolObject != null)
        {
            // Matikan semua SpriteRenderer di dalam EnemyWeapon (termasuk children)
            foreach (SpriteRenderer sr in pistolObject.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.enabled = false;
            }
        }

        // Terapkan default stats berdasarkan level
        ApplyLevelDefaults();
    }

    private void Start()
    {
        // Cari player di scene
        FindPlayer();

        // Terapkan max health ke EnemyHealth jika ada
        if (healthComponent != null)
        {
            healthComponent.SetMaxHealth(maxHealth);
        }
    }

    private void ApplyLevelDefaults()
    {
        switch (level)
        {
            case EnemyLevel.Level1_NinjaFrog:
                maxHealth = 3;
                patrolSpeed = 2.2f;
                contactDamage = 1;
                detectionRange = 0f; // Tidak menembak, fokus patroli tabrak
                break;

            case EnemyLevel.Level2_PinkMan:
                maxHealth = 5;
                patrolSpeed = 1.6f;
                contactDamage = 1;
                detectionRange = 8f;
                fireRate = 1.8f;
                stopShootingDistance = 5f;
                break;

            case EnemyLevel.Level3_VirtualGuy:
                maxHealth = 8;
                patrolSpeed = 2.2f;
                chaseSpeed = 4f;
                contactDamage = 1;
                detectionRange = 10f;
                fireRate = 1.4f;
                burstCount = 3;
                burstInterval = 0.14f;
                stopShootingDistance = 4f;
                break;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (playerTransform == null)
        {
            FindPlayer();
        }

        switch (level)
        {
            case EnemyLevel.Level1_NinjaFrog:
                UpdateLevel1();
                break;
            case EnemyLevel.Level2_PinkMan:
                UpdateLevel2();
                break;
            case EnemyLevel.Level3_VirtualGuy:
                UpdateLevel3();
                break;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    // ==========================================
    // LEVEL 1: NINJA FROG (Melee Patroli Tabrak)
    // ==========================================
    private void UpdateLevel1()
    {
        isPlayerDetected = false;
        Patrol(patrolSpeed);
    }

    // ==========================================
    // LEVEL 2: PINK MAN (Penembak Jarak Menengah)
    // ==========================================
    private void UpdateLevel2()
    {
        if (playerTransform == null)
        {
            isPlayerDetected = false;
            Patrol(patrolSpeed);
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distToPlayer <= detectionRange)
        {
            isPlayerDetected = true;
            // Player terdeteksi: berhenti dan hadap player
            StopMovement();
            FaceTarget(playerTransform.position.x);

            // Tembak pistol jika cooldown selesai (HANYA jika tidak ada EnemyPistol terpisah agar tidak menembak ganda)
            EnemyPistol pistol = GetComponentInChildren<EnemyPistol>();
            if (pistol == null && Time.time >= nextFireTime)
            {
                ShootSingle();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            isPlayerDetected = false;
            // Tidak ada player: lanjut patroli
            Patrol(patrolSpeed);
        }
    }

    // ==========================================
    // LEVEL 3: VIRTUAL GUY (Burst Shooter & Chaser)
    // ==========================================
    private void UpdateLevel3()
    {
        if (playerTransform == null)
        {
            isPlayerDetected = false;
            Patrol(patrolSpeed);
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distToPlayer <= detectionRange)
        {
            isPlayerDetected = true;
            FaceTarget(playerTransform.position.x);

            // Jika masih jauh dari stopShootingDistance, kejar player
            if (distToPlayer > stopShootingDistance)
            {
                ChasePlayer();
            }
            else
            {
                StopMovement();
            }

            // Tembak mode Burst (HANYA jika tidak ada EnemyPistol terpisah agar tidak menembak ganda)
            EnemyPistol pistol = GetComponentInChildren<EnemyPistol>();
            if (pistol == null && Time.time >= nextFireTime && !isBurstShooting)
            {
                StartCoroutine(ShootBurstRoutine());
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            isPlayerDetected = false;
            Patrol(patrolSpeed);
        }
    }

    private Collider2D bodyCollider;
    private float lastTurnTime = -100f;
    private float turnCooldown = 0.35f; // Mencegah bolak-balik stuck setiap frame
    private float lastXPosition = -9999f;
    private float stuckTimer = 0f;

    // ==========================================
    // PATROL & MOVEMENT LOGIC
    // ==========================================
    private void Patrol(float speed)
    {
        CheckObstaclesAndLedge();
        CheckAntiStuckWatchdog();

        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        // Atur arah hadap sprite
        UpdateSpriteFacing(dir > 0);
    }

    private void ChasePlayer()
    {
        float dirX = playerTransform.position.x - transform.position.x;
        bool wantRight = dirX > 0f;

        // Cek tebing di depan saat mengejar agar tidak jatuh
        if (IsLedgeAhead(wantRight) || IsWallAhead(wantRight))
        {
            // Berhenti di tepi tebing agar tidak jatuh
            StopMovement();
            return;
        }

        float dir = wantRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        UpdateSpriteFacing(wantRight);
    }

    private void StopMovement()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public bool IsGrounded()
    {
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        Bounds b = bodyCollider != null ? bodyCollider.bounds : new Bounds(transform.position, Vector3.one * 0.8f);

        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.05f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.2f, groundLayer);

        return hit.collider != null && !hit.collider.isTrigger;
    }

    private void CheckObstaclesAndLedge()
    {
        // Beri jeda minimal sebelum bisa balik arah lagi (anti-stuck loop)
        if (Time.time < lastTurnTime + turnCooldown) return;

        // Cek rintangan dinding atau undakan
        if (IsWallAhead(movingRight))
        {
            TurnAround();
            return;
        }

        // Hanya cek jurang / tebing jika musuh sedang menapak di tanah
        if (IsGrounded() && IsLedgeAhead(movingRight))
        {
            TurnAround();
        }
    }

    private void TurnAround()
    {
        movingRight = !movingRight;
        lastTurnTime = Time.time;
        stuckTimer = 0f;
        if (rb != null)
        {
            float speed = patrolSpeed;
            float dir = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
        }
        UpdateSpriteFacing(movingRight);
    }

    // Failsafe Watchdog: jika posisi tidak bergerak selama 0.5s saat harusnya jalan, otomatis berbalik!
    private void CheckAntiStuckWatchdog()
    {
        if (Mathf.Abs(transform.position.x - lastXPosition) < 0.03f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= 0.5f && Time.time >= lastTurnTime + turnCooldown)
            {
                TurnAround();
            }
        }
        else
        {
            stuckTimer = 0f;
            lastXPosition = transform.position.x;
        }
    }

    private bool IsWallAhead(bool toRight)
    {
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();

        Bounds b = bodyCollider != null ? bodyCollider.bounds : new Bounds(transform.position, Vector3.one * 0.8f);
        float dir = toRight ? 1f : -1f;
        Vector2 castDir = toRight ? Vector2.right : Vector2.left;
        float startX = b.center.x + (dir * (b.extents.x + 0.04f));

        // Cek 2 ketinggian: ketinggian pinggang dan dekat kaki agar tidak terhalang undakan
        Vector2[] testOrigins = new Vector2[]
        {
            new Vector2(startX, b.center.y),
            new Vector2(startX, b.min.y + 0.2f)
        };

        foreach (var origin in testOrigins)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, castDir, wallCheckDistance, groundLayer);
            if (hit.collider != null && !hit.collider.isTrigger && hit.collider.gameObject != gameObject)
            {
                return true; // Menabrak dinding/rintangan padat
            }
        }

        return false;
    }

    private bool IsLedgeAhead(bool toRight)
    {
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();

        Bounds b = bodyCollider != null ? bodyCollider.bounds : new Bounds(transform.position, Vector3.one * 0.8f);
        float dir = toRight ? 1f : -1f;

        // Titik raycast tepat di depan ujung kaki
        Vector2 origin = new Vector2(b.center.x + (dir * (b.extents.x + 0.1f)), b.min.y + 0.1f);

        // Cek lantai padat (Ground layer) langsung di bawah kaki depan
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.4f, groundLayer);

        // Jika tidak ada collider Ground di bawah kaki depan, berarti tebing/ujung platform!
        return hit.collider == null || hit.collider.isTrigger;
    }


    private void FaceTarget(float targetX)
    {
        bool targetIsRight = targetX > transform.position.x;
        UpdateSpriteFacing(targetIsRight);
    }

    private void UpdateSpriteFacing(bool faceRight)
    {
        movingRight = faceRight;

        if (spriteRenderer != null)
        {
            // Sprite Pixel Adventure default menghadap kanan
            spriteRenderer.flipX = !faceRight;
        }

        // Cermin posisi EnemyWeapon / Firepoint agar pindah ke sisi kiri musuh saat menghadap kiri
        Transform weapon = transform.Find("EnemyWeapon");
        if (weapon != null)
        {
            Vector3 pos = weapon.localPosition;
            pos.x = Mathf.Abs(pos.x) * (faceRight ? 1f : -1f);
            weapon.localPosition = pos;
        }

        // Jika Firepoint berada langsung sebagai child musuh (bukan di dalam EnemyWeapon)
        Transform directFirepoint = transform.Find("Firepoint");
        if (directFirepoint != null)
        {
            Vector3 pos = directFirepoint.localPosition;
            pos.x = Mathf.Abs(pos.x) * (faceRight ? 1f : -1f);
            directFirepoint.localPosition = pos;
        }
    }

    // ==========================================
    // SHOOTING LOGIC
    // ==========================================
    private void ShootSingle()
    {
        if (bulletPrefab == null) return;

        // Trigger animasi tembak
        newAnimController?.PlayShoot();

        Vector2 spawnPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 shootDir = (playerTransform.position - (Vector3)spawnPos).normalized;

        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        Instantiate(bulletPrefab, spawnPos, rotation);

        if (muzzleFlashPrefab != null)
        {
            Instantiate(muzzleFlashPrefab, spawnPos, rotation);
        }
    }

    private IEnumerator ShootBurstRoutine()
    {
        isBurstShooting = true;

        for (int i = 0; i < burstCount; i++)
        {
            if (playerTransform == null) break;

            ShootSingle();
            yield return new WaitForSeconds(burstInterval);
        }

        isBurstShooting = false;
    }

    // ==========================================
    // CONTACT DAMAGE (Tabrak Player)
    // ==========================================
    private void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    private void HandlePlayerContact(GameObject other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time >= lastContactDamageTime + contactDamageCooldown)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                lastContactDamageTime = Time.time;
            }
        }
    }

    // Gizmos untuk visualisasi di Unity Editor
    private void OnDrawGizmosSelected()
    {
        // Detection Range (Kuning)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Stop Distance (Merah)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopShootingDistance);

        // Ledge Check (Hijau)
        float xOffset = (movingRight ? 1f : -1f) * ledgeForwardOffset;
        Vector3 origin = new Vector3(transform.position.x + xOffset, transform.position.y, 0f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + Vector3.down * ledgeCheckDistance);

        // Wall Check (Biru)
        Vector3 wallDir = movingRight ? Vector3.right : Vector3.left;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + wallDir * wallCheckDistance);
    }
}
