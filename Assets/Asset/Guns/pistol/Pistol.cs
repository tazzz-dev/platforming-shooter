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

    [Header("VFX Settings")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Scale Settings")]
    [SerializeField] private Vector3 gunScale = new Vector3(1f, 1f, 1f);

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

        // Menggunakan wasPressedThisFrame agar harus di-tap (semi-otomatis), tidak bisa ditahan
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
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
    }
}