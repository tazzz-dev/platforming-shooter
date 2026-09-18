using UnityEngine;

public class ConstantTurret : MonoBehaviour
{
    [Header("Testing Settings")]
    [SerializeField] private GameObject enemyBulletPrefab; // Prefab EnemyBullet
    [SerializeField] private Transform firePoint;          // Firepoint
    [SerializeField] private float fireRate = 1.5f;        // Jeda waktu tembak (detik)

    private float nextFireTime;

    private void Update()
    {
        // Menembak secara berkala dan otomatis
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (enemyBulletPrefab != null && firePoint != null)
        {
            // Menembak sesuai arah rotasi/hadap Firepoint
            Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}