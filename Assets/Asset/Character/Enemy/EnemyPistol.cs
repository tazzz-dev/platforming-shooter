using UnityEngine;

public class EnemyPistol : MonoBehaviour
{
    [Header("Target & Orbit Settings")]
    [SerializeField] private Transform enemyBodyTransform; // Transform badan musuh
    [SerializeField] private float orbitDistance = 0.8f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject enemyBulletPrefab; // Prefab EnemyBullet (Warna Merah)
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float attackRange = 8f; // Jarak tembak musuh

    [Header("VFX Settings")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Burst Settings")]
    [SerializeField] private bool useBurst = false;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.14f;

    private float nextFireTime;
    private Transform playerTransform;
    private bool isBursting = false;

    public void ConfigureWeapon(Transform body, GameObject bullet, GameObject muzzle, float rate, float range, bool burst = false, int bCount = 3, float bInterval = 0.14f)
    {
        enemyBodyTransform = body;
        enemyBulletPrefab = bullet;
        muzzleFlashPrefab = muzzle;
        fireRate = rate;
        attackRange = range;
        useBurst = burst;
        burstCount = bCount;
        burstInterval = bInterval;
    }

    private void Start()
    {
        // Otomatis mencari Player berdasarkan Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (playerTransform == null || enemyBodyTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Hanya mengincar & menembak jika Player masuk dalam jarak tembak
        if (distanceToPlayer <= attackRange)
        {
            HandleAimAndOrbit();

            if (Time.time >= nextFireTime && !isBursting)
            {
                if (useBurst)
                {
                    StartCoroutine(ShootBurstRoutine());
                }
                else
                {
                    Shoot();
                }
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private System.Collections.IEnumerator ShootBurstRoutine()
    {
        isBursting = true;
        for (int i = 0; i < burstCount; i++)
        {
            if (playerTransform == null || enemyBodyTransform == null) break;
            Shoot();
            yield return new WaitForSeconds(burstInterval);
        }
        isBursting = false;
    }

    private void HandleAimAndOrbit()
    {
        // Senjata berputar menghadap lokasi Player
        Vector3 aimDirection = (playerTransform.position - enemyBodyTransform.position).normalized;
        transform.position = enemyBodyTransform.position + (aimDirection * orbitDistance);

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Flip Y agar senjata musuh tidak terbalik
        if (Mathf.Abs(angle) > 90f) transform.localScale = new Vector3(1f, -1f, 1f);
        else transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void Shoot()
    {
        if (enemyBulletPrefab == null || firePoint == null) return;

        // Spawn peluru merah khusus musuh
        Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);

        // Muzzle Flash
        if (muzzleFlashPrefab != null)
        {
            Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
        }
    }
}