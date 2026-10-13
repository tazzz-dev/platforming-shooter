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

    private float nextFireTime;
    private Transform playerTransform;

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

            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
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