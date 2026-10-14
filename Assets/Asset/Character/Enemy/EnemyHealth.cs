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

    // Event statis setiap kali ada musuh yang terbunuh
    public static event System.Action OnEnemyKilled;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteAnimator = GetComponent<EnemySpriteAnimator>();
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

        // Animasi Hit jika ada
        if (spriteAnimator != null)
        {
            spriteAnimator.PlayHitAnimation();
        }

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
        Destroy(gameObject);
    }
}