using UnityEngine;
using System.Collections;

public class EnemyPistol : MonoBehaviour
{
    [Header("Target & Orbit Settings")]
    [SerializeField] private Transform enemyBodyTransform; // Transform badan musuh
    [SerializeField] private float orbitDistance = 0.8f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float attackRange = 8f;

    [Header("VFX Settings")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Burst Settings")]
    [SerializeField] private bool useBurst = false;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.14f;

    [Header("Animation Sync")]
    [Tooltip("Delay (detik) antara animasi mulai dan peluru keluar.\n" +
             "Hitung: frame_tembak / fps_animasi\n" +
             "Contoh Enemy2_Shoot 9 frame @12fps, tembak di frame 6 → 6/12 = 0.5")]
    [SerializeField] private float shootFireDelay = 0.35f;

    private float nextFireTime;
    private Transform playerTransform;
    private bool isBursting = false;
    private NewEnemyAnimController newAnimController;
    private EnemyAI enemyAI;

    public void ConfigureWeapon(Transform body, GameObject bullet, GameObject muzzle,
                                float rate, float range, bool burst = false,
                                int bCount = 3, float bInterval = 0.14f)
    {
        enemyBodyTransform = body;
        enemyBulletPrefab  = bullet;
        muzzleFlashPrefab  = muzzle;
        fireRate           = rate;
        attackRange        = range;
        useBurst           = burst;
        burstCount         = bCount;
        burstInterval      = bInterval;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        // Auto-detect firePoint jika belum di-assign
        if (firePoint == null)
        {
            Transform fp = transform.Find("Firepoint");
            if (fp != null) firePoint = fp;
            else firePoint = transform; // Fallback jika tidak ada child Firepoint
        }

        // Cari animator bridge dan EnemyAI di parent (EnemyWeapon adalah child dari enemy)
        newAnimController = GetComponentInParent<NewEnemyAnimController>();
        enemyAI           = GetComponentInParent<EnemyAI>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (playerTransform == null || enemyBodyTransform == null) return;

        // JIKA EnemyAI sedang patroli (tidak mendeteksi/terfokus pada player), JANGAN tembak!
        if (enemyAI != null && !enemyAI.IsPlayerEngaged)
        {
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= attackRange)
        {
            AimAtPlayer();

            if (Time.time >= nextFireTime && !isBursting)
            {
                if (useBurst)
                    StartCoroutine(ShootBurstRoutine());
                else
                    TriggerShoot();

                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void AimAtPlayer()
    {
        if (playerTransform == null) return;

        // Hadapkan senjata / firepoint ke arah Player
        Vector3 aimDirection = (playerTransform.position - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Flip Y agar tidak terbalik rotasinya jika menghadap kiri
        if (Mathf.Abs(angle) > 90f) transform.localScale = new Vector3(1f, -1f, 1f);
        else transform.localScale = new Vector3(1f, 1f, 1f);
    }

    // ─── Shooting ─────────────────────────────────────────────────────────────

    private IEnumerator ShootBurstRoutine()
    {
        isBursting = true;
        for (int i = 0; i < burstCount; i++)
        {
            if (playerTransform == null || enemyBodyTransform == null) break;
            TriggerShoot();
            // Jeda antar burst harus >= delay agar peluru sebelumnya sudah keluar
            yield return new WaitForSeconds(Mathf.Max(burstInterval, shootFireDelay + 0.05f));
        }
        isBursting = false;
    }

    /// <summary>Trigger animasi, lalu spawn peluru setelah delay sesuai frame animasi.</summary>
    private void TriggerShoot()
    {
        if (enemyBulletPrefab == null) return;

        // Step 1: putar animasi tembak
        newAnimController?.PlayShoot();

        // Step 2: spawn peluru setelah delay (sinkron dengan frame tembak di animasi)
        StartCoroutine(SpawnBulletAfterDelay());
    }

    private IEnumerator SpawnBulletAfterDelay()
    {
        // Tunggu frame "tembak" di animasi
        yield return new WaitForSeconds(shootFireDelay);

        // Jika enemy sudah mati saat delay berjalan, hentikan
        if (enemyBulletPrefab == null || this == null) yield break;

        // Dapatkan titik spawn dan rotasi arah ke Player dari firePoint
        Transform spawnPoint = (firePoint != null) ? firePoint : transform;
        Vector2 spawnPos = spawnPoint.position;
        
        Quaternion bulletRot = spawnPoint.rotation;
        if (playerTransform != null)
        {
            Vector2 dirToPlayer = ((Vector2)playerTransform.position - spawnPos).normalized;
            float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            bulletRot = Quaternion.Euler(0f, 0f, angle);
        }

        Instantiate(enemyBulletPrefab, spawnPos, bulletRot);

        if (muzzleFlashPrefab != null)
            Instantiate(muzzleFlashPrefab, spawnPos, bulletRot, spawnPoint);
    }
}