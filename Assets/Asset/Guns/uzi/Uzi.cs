using UnityEngine;
using UnityEngine.InputSystem;

public class Uzi : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float orbitDistance = 0.8f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.08f; // Sangat cepat untuk Uzi
    [SerializeField] private float spreadAngle = 15f; // Semakin besar angkanya, semakin acak arah pelurunya
    [Tooltip("Jarak maksimal peluru Uzi sebelum menghilang (dalam unit kotak, misal 10 unit)")]
    [SerializeField] private float bulletRange = 10f;

    [Header("Ammo & Reload Settings")]
    [SerializeField] private int maxAmmo = 25;
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private bool allowManualReload = true;

    [Header("VFX Settings")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Scale Settings")]
    [SerializeField] private Vector3 gunScale = new Vector3(0.4f, 0.4f, 1f); 

    private int currentAmmo;
    private bool isReloading = false;
    private Coroutine reloadCoroutine;
    private float nextFireTime = 0f;
    private Camera mainCam;
    private SpriteRenderer gunSprite;
    private Animator gunAnim;

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        mainCam = Camera.main;
        gunSprite = GetComponent<SpriteRenderer>();
        gunAnim = GetComponent<Animator>(); 
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

        // Menggunakan isPressed agar bisa diberondong (ditahan)
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
        Debug.Log($"[Uzi] Sedang reload... ({reloadTime} detik)");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadCoroutine = null;
        Debug.Log("[Uzi] Reload selesai! Amunisi penuh.");
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

        // Flip Y menggunakan custom gunScale agar ukuran tetap proporsional
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

        // Menghitung rotasi acak (spread)
        float randomAngle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
        Quaternion bulletRotation = Quaternion.Euler(0, 0, firePoint.eulerAngles.z + randomAngle);

        // Memunculkan peluru dengan rotasi yang sudah diacak
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetRange(bulletRange);
        }

        if (muzzleFlashPrefab != null)
        {
            Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
        }

        // Memanggil animasi tembakan saat peluru dimunculkan
        if (gunAnim != null)
        {
            gunAnim.SetTrigger("shoot");
        }

        // Jika peluru habis setelah tembakan ini, langsung mulai reload
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }
}
