using UnityEngine;
using UnityEngine.InputSystem;

public class Shotgun : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float orbitDistance = 1.0f;

    [Header("Shotgun Mechanics")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.6f;
    [SerializeField] private int pelletsCount = 5;      // Peluru menyebar
    [SerializeField] private float spreadAngle = 15f;   // Sudut sebaran
    [Tooltip("Jarak maksimal peluru shotgun sebelum menghilang (jarak dekat, rekomendasi: 6-7 unit)")]
    [SerializeField] private float bulletRange = 6.5f;


    [Header("VFX Settings")]
    [SerializeField] private GameObject muzzleFlashPrefab; // Tambahkan ini di bagian atas kelas

    private float nextFireTime = 0f;
    private Camera mainCam;
    private Animator gunAnim;

    private void Awake()
    {
        mainCam = Camera.main;
        gunAnim = GetComponent<Animator>();
    }

    private void Start()
    {
        FindPlayerIfNeeded();
    }

    private void OnEnable()
    {
        FindPlayerIfNeeded();
    }

    private void FindPlayerIfNeeded()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private void Update()
    {
        HandleOrbitAndAim();

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void HandleOrbitAndAim()
    {
        if (playerTransform == null) return;

        Vector3 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;

        Vector3 aimDirection = (mousePos - playerTransform.position).normalized;
        transform.position = playerTransform.position + (aimDirection * orbitDistance);

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Flip visual tanpa merusak pixel art (Scale tetap 1,1,1)
        if (Mathf.Abs(angle) > 90f) transform.localScale = new Vector3(1f, -1f, 1f);
        else transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 1. Spawn Peluru Menyebar (Looping Pellets)
        for (int i = 0; i < pelletsCount; i++)
        {
            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            Quaternion spreadRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, randomSpread);
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, spreadRotation);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetRange(bulletRange);
            }
        }

        // 2. Spawn Efek Muzzle Flash (1x Tembak)
        if (muzzleFlashPrefab != null)
        {
            Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
        }

        // 3. Trigger Animasi Tembak
        if (gunAnim != null)
        {
            gunAnim.SetTrigger("shoot");
        }
    }
}