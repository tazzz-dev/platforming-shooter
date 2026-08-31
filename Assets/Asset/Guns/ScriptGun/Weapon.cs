using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.15f;

    private float nextFireTime = 0f;
    private Camera mainCam;
    private SpriteRenderer gunSprite;
    private Animator gunAnim; // Ditambahkan

    private void Awake()
    {
        mainCam = Camera.main;
        gunSprite = GetComponent<SpriteRenderer>();
        gunAnim = GetComponent<Animator>(); // Ditambahkan
    }

    private void Update()
    {
        AimAtMouse();

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void AimAtMouse()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (angle > 90f || angle < -90f)
        {
            gunSprite.flipY = true;
        }
        else
        {
            gunSprite.flipY = false;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Picu animasi tembakan
            if (gunAnim != null)
            {
                gunAnim.SetTrigger("shoot");
            }
        }
    }
}