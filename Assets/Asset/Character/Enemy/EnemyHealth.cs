using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("UI Reference")]
    [SerializeField] private FloatingHealthBar healthBar;

    private void Start()
    {
        currentHealth = maxHealth;

        // Inisialisasi jumlah hati di UI saat start
        if (healthBar != null)
        {
            healthBar.SetupHearts(maxHealth);
        } 
        else
        {
            Debug.LogError("Healthbar belum ada wok");
        }
    }

    public void TakeDamage(int damageAmount = 1)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        // Update tampilan hati
        if (healthBar != null)
        {
            healthBar.UpdateHearts(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}