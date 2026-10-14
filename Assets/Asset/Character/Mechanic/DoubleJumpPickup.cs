using UnityEngine;

/// <summary>
/// Item Pickup untuk membuka kemampuan Double Jump.
/// Letakkan di area map yang diinginkan (dengan Collider2D IsTrigger).
/// Saat karakter menyentuhnya, kemampuan Double Jump akan terbuka secara permanen!
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DoubleJumpPickup : MonoBehaviour
{
    [Header("Visual & Animasi Melayang")]
    [Tooltip("Tinggi ayunan animasi melayang")]
    [SerializeField] private float floatAmplitude = 0.15f;
    [Tooltip("Kecepatan ayunan animasi melayang")]
    [SerializeField] private float floatSpeed = 3f;

    [Header("Audio (Opsional)")]
    [Tooltip("Suara SFX saat item diambil")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;

    [Header("Partikel / Efek (Opsional)")]
    [Tooltip("Prefab efek partikel saat diambil")]
    [SerializeField] private GameObject pickupEffectPrefab;

    private Vector3 startPosition;
    private bool isCollected = false;

    private void Awake()
    {
        // Pastikan collider adalah trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Jika objek belum punya sprite, otomatis beri sprite lingkaran cahaya biru/cyan
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
        {
            sr.sprite = AltarCheckpoint.GetCircleSprite();
            sr.color = new Color(0.2f, 0.85f, 1f, 0.95f); // Cyan terang bercahaya
            sr.sortingOrder = 10;
        }
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Animasi melayang (hover) naik-turun halus
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        // Cek apakah yang menyentuh adalah Player
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = other.GetComponentInParent<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            isCollected = true;

            // Buka kemampuan Double Jump pada Player!
            playerMovement.UnlockDoubleJump();

            // Mainkan suara jika ada
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
            }

            // Munculkan partikel efek jika ada
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            Debug.Log($"[DoubleJumpPickup] Item Double Jump berhasil diambil oleh pemain!");

            // Hancurkan item
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualisasi di Scene View
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
