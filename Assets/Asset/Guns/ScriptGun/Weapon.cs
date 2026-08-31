using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float orbitDistance = 0.8f;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.15f;

    [Header("Scale Settings")]
    [SerializeField] private Vector3 gunScale = new Vector3(0.4f, 0.4f, 1f); // Atur ukuran ideal senjata di sini

    private float nextFireTime = 0f;
    private Camera mainCam;
    private SpriteRenderer gunSprite;
    private Animator gunAnim;

    private void Awake()
    {
        mainCam = Camera.main;
        gunSprite = GetComponent<SpriteRenderer>();
        gunAnim = GetComponent<Animator>(); // Mengambil komponen Animator Gun
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
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Memanggil animasi tembakan saat peluru dimunculkan
            if (gunAnim != null)
            {
                gunAnim.SetTrigger("shoot");
            }
        }
    }
}