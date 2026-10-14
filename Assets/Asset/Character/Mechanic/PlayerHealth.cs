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

    // Getter publik untuk UI (HealthUI / FloatingHealthBar)
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // Event saat darah berubah (current, max)
    public static event System.Action<int, int> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Fungsi utama untuk menerima damage
    public void TakeDamage(int damageAmount = 1)
    {
        // Jika sedang dalam masa imun, abaikan serangan
        if (isImmune) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        Debug.Log("Player kena Hit! Sisa Health: " + currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(ImmunityRoutine());
        }
    }

    /// <summary>
    /// Membunuh player seketika berapapun sisa darahnya (misal terkena duri/spike atau jatuh ke jurang),
    /// mengabaikan masa imun.
    /// </summary>
    public void InstantKill()
    {
        currentHealth = 0;
        isImmune = false;
        StopAllCoroutines();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("[PlayerHealth] Player terkena Instant Kill (Spike/Hazard)!");
        Die();
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

        // Jika ada sistem respawn, gunakan itu. Jika tidak, nonaktifkan saja.
        if (PlayerRespawn.Instance != null)
        {
            // Nonaktifkan Player sementara, lalu biarkan PlayerRespawn yang mengaktifkan kembali
            gameObject.SetActive(false);
            PlayerRespawn.Instance.Respawn();
        }
        else
        {
            // Fallback: nonaktifkan saja jika tidak ada PlayerRespawn di scene
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Reset health ke maksimum. Dipanggil oleh PlayerRespawn saat respawn.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Pastikan sprite kembali normal (tidak sedang efek kedip)
        StopAllCoroutines();
        isImmune = false;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        Debug.Log($"[PlayerHealth] Health direset ke {maxHealth}");
    }
}