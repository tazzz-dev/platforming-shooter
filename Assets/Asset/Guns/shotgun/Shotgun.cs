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


    [Header("Ammo & Reload Settings")]
    [SerializeField] private int maxAmmo = 4;
    [SerializeField] private float reloadTime = 1.8f;
    [SerializeField] private bool allowManualReload = true;

    [Header("VFX Settings")]
    [SerializeField] private GameObject muzzleFlashPrefab; // Tambahkan ini di bagian atas kelas

    [Header("Audio Settings")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] [Range(0f, 1f)] private float shootVolume = 1f;

    [Header("Scale Settings")]
    [SerializeField] private Vector3 gunScale = new Vector3(1f, 1f, 1f);

    private int currentAmmo;
    private bool isReloading = false;
    private Coroutine reloadCoroutine;
    private float nextFireTime = 0f;
    private Camera mainCam;
    private Animator gunAnim;
    private AudioSource audioSource;

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        mainCam = Camera.main;
        gunAnim = GetComponent<Animator>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 21;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        currentAmmo = maxAmmo;
    }

    private void Start()
    {
        FindPlayerIfNeeded();
    }

    private void OnEnable()
    {
        FindPlayerIfNeeded();
        isReloading = false;
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }

    private void OnDisable()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        isReloading = false;
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
        if (Time.timeScale == 0f) return;

        HandleOrbitAndAim();

        // Manual Reload menggunakan tombol R
        if (allowManualReload && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (!isReloading && currentAmmo < maxAmmo)
            {
                StartReload();
                return;
            }
        }

        // Jangan izinkan menembak saat sedang reload
        if (isReloading) return;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                StartReload();
            }
        }
    }

    public void StartReload()
    {
        if (isReloading || currentAmmo >= maxAmmo) return;
        reloadCoroutine = StartCoroutine(ReloadRoutine());
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log($"[Shotgun] Sedang reload... ({reloadTime} detik)");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadCoroutine = null;
        Debug.Log("[Shotgun] Reload selesai! Amunisi penuh.");
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

        // Flip visual menggunakan gunScale agar bisa diatur di Inspector
        if (Mathf.Abs(angle) > 90f) transform.localScale = new Vector3(gunScale.x, -gunScale.y, gunScale.z);
        else transform.localScale = gunScale;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        currentAmmo--;

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

        // 4. Mainkan SFX tembakan
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
        }

        // Jika peluru habis setelah tembakan ini, langsung mulai reload
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }
}