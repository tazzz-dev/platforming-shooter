using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 2; // Bisa diatur di Inspector (1, 2, 3, dst.)
    private int currentHealth;

    [Header("Visual Feedback")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Fungsi utama untuk menerima damage dari peluru Player
    public void TakeDamage(int damageAmount = 1)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " terkena hit! Sisa HP: " + currentHealth);

        // Triger efek kedip merah saat kena hit
        if (spriteRenderer != null && !isFlashing)
        {
            StartCoroutine(FlashRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;
        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " Mati!");
        // Kamu bisa menaruh efek partikel ledakan / SFX di sini nanti
        Destroy(gameObject);
    }
}