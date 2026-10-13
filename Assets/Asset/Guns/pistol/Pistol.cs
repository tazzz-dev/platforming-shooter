using UnityEngine;
using UnityEngine.InputSystem;

public class Pistol : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float orbitDistance = 0.8f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [Tooltip("Jeda interval minimal antar tembakan/tap (dalam detik). 0.25s = maksimal 4 peluru per detik")]
    [SerializeField] private float fireRate = 0.25f;
    [Tooltip("Jarak maksimal peluru pistol sebelum menghilang (dalam unit kotak)")]
    [SerializeField] private float bulletRange = 14f;

    [Header("Ammo & Reload Settings")]
    [SerializeField] private int maxAmmo = 7;
    [SerializeField] private float reloadTime = 1f;
    [SerializeField] private bool allowManualReload = true;

    [Header("VFX Settings")]
    [SerializeField] private GameObject muzzleFlashPrefab;

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
        // Reset status reload jika senjata diaktifkan kembali
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

        // Jika sedang reload, tidak bisa menembak
        if (isReloading) return;

        // Menggunakan wasPressedThisFrame agar harus di-tap (semi-otomatis), tidak bisa ditahan
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
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
        Debug.Log($"[Pistol] Sedang reload... ({reloadTime} detik)");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadCoroutine = null;
        Debug.Log("[Pistol] Reload selesai! Amunisi penuh.");
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

        if (Mathf.Abs(angle) > 90f)
        {
            transform.localScale = new Vector3(gunScale.x, -gunScale.y, gunScale.z);
        }
        else
        {
            transform.localScale = gunScale;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        currentAmmo--;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetRange(bulletRange);
        }

        if (muzzleFlashPrefab != null)
        {
            Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
        }

        if (gunAnim != null)
        {
            gunAnim.SetTrigger("shoot");
        }

        // Mainkan SFX tembakan
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