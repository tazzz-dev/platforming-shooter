using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    [Header("Immunity Settings")]
    [SerializeField] private float immunityDuration = 3f; // Durasi imun dalam detik
    private bool isImmune = false;

    [Header("Visual Feedback (Optional)")]
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Fungsi utama untuk menerima damage
    public void TakeDamage(int damageAmount = 1)
    {
        // Jika sedang dalam masa imun, abaikan serangan
        if (isImmune) return;

        currentHealth -= damageAmount;
        Debug.Log("Player kena Hit! Sisa Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(ImmunityRoutine());
        }
    }

    // Coroutine untuk mengaktifkan timer imun & efek kelip-kelip visual
    private IEnumerator ImmunityRoutine()
    {
        isImmune = true;

        float timer = 0f;
        float flashInterval = 0.1f;

        // Efek visual kelip-kelip selama masa imun
        while (timer < immunityDuration)
        {
            if (spriteRenderer != null)
            {
                // Ubah transparansi sprite (efek kedip)
                Color color = spriteRenderer.color;
                color.a = (color.a == 1f) ? 0.3f : 1f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        // Kembalikan sprite ke normal setelah masa imun selesai
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        isImmune = false;
    }

    private void Die()
    {
        Debug.Log("Player Mati!");
        // Logika kematian (misal: Reload Scene / Animasi Mati)
        gameObject.SetActive(false);
    }
}