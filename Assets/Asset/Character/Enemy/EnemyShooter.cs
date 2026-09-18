using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;

    private float nextFireTime;

    private void Update()
    {
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
            Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}