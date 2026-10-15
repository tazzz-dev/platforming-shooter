using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("UI Reference")]
    [SerializeField] private FloatingHealthBar healthBar;

    private SpriteRenderer spriteRenderer;
    private EnemySpriteAnimator spriteAnimator;
    private NewEnemyAnimController newAnimController;

    // Event statis setiap kali ada musuh yang terbunuh
    public static event System.Action OnEnemyKilled;

    private void Awake()
    {
        spriteRenderer    = GetComponent<SpriteRenderer>();
        spriteAnimator    = GetComponent<EnemySpriteAnimator>();
        newAnimController = GetComponent<NewEnemyAnimController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        // Inisialisasi jumlah hati di UI saat start
        if (healthBar != null)
        {
            healthBar.SetupHearts(maxHealth);
        }
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetupHearts(maxHealth);
        }
    }

    public void TakeDamage(int damageAmount = 1)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        // Visual flash merah saat terkena serangan
        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlashRoutine());
        }

        // Animasi Hit
        if (newAnimController != null)
            newAnimController.PlayGetHit();
        else if (spriteAnimator != null)
            spriteAnimator.PlayHitAnimation();

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

    private IEnumerator DamageFlashRoutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f); // Merah
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        OnEnemyKilled?.Invoke();

        // ── Segera matikan semua komponen agar tidak bergerak/damage saat death ──
        var ai  = GetComponent<EnemyAI>();
        var rb  = GetComponent<Rigidbody2D>();

        if (ai != null) ai.enabled = false;
        if (rb != null)
        {
            // Kinematic: matikan semua physics (gravity, forces) agar enemy
            // tidak jatuh dari peta, tapi collider tetap aktif sebagai tumpuan
            rb.linearVelocity  = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Matikan EnemySpriteAnimator agar tidak menimpa sprite death dari Animator
        if (spriteAnimator != null) spriteAnimator.enabled = false;

        if (newAnimController != null)
        {
            // Jalankan animasi death via Animator, baru destroy setelah selesai
            newAnimController.PlayDeath();
            StartCoroutine(DestroyAfterDelay(1.2f));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}