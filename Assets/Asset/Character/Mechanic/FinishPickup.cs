using UnityEngine;

/// <summary>
/// Item pickup yang saat disentuh player akan menampilkan panel "Game Finish".
/// Letakkan di ujung/akhir map. Membutuhkan Collider2D (auto di-set jadi trigger).
/// Hubungkan finishPanelManager ke GameObject yang punya GameFinishPanel.cs.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FinishPickup : MonoBehaviour
{
    [Header("Referensi Panel")]
    [Tooltip("GameObject yang memiliki komponen GameFinishPanel")]
    [SerializeField] private GameFinishPanel finishPanelManager;

    [Header("Visual & Animasi Melayang")]
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed     = 2.5f;
    [SerializeField] private Color itemColor      = new Color(1f, 0.85f, 0.1f, 1f); // Kuning emas

    [Header("Rotasi")]
    [SerializeField] private float rotateSpeed = 90f; // Derajat per detik

    [Header("Audio (Opsional)")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;

    [Header("Efek Partikel (Opsional)")]
    [SerializeField] private GameObject pickupEffectPrefab;

    private Vector3  startPosition;
    private bool     isCollected = false;
    private SpriteRenderer sr;

    private void Awake()
    {
        // Pastikan collider trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        sr = GetComponent<SpriteRenderer>();

        // Jika belum punya sprite, pakai sprite bintang/lingkaran sederhana
        if (sr != null && sr.sprite == null)
        {
            sr.sprite       = AltarCheckpoint.GetCircleSprite();
            sr.color        = itemColor;
            sr.sortingOrder = 10;
        }
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isCollected) return;

        // Animasi melayang naik-turun
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotasi perlahan (untuk efek bintang berputar)
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        // Hanya bereaksi jika yang menyentuh adalah Player
        bool isPlayer = other.CompareTag("Player")
                     || other.GetComponent<PlayerMovement>() != null
                     || other.GetComponentInParent<PlayerMovement>() != null;

        if (!isPlayer) return;

        isCollected = true;

        // Mainkan suara pickup jika ada
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);

        // Munculkan efek partikel jika ada
        if (pickupEffectPrefab != null)
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        // Tampilkan panel Finish
        if (finishPanelManager != null)
        {
            finishPanelManager.ShowPanel();
        }
        else
        {
            // Coba cari di scene jika tidak di-assign manual
            GameFinishPanel panel = FindFirstObjectByType<GameFinishPanel>();
            if (panel != null) panel.ShowPanel();
            else Debug.LogWarning("[FinishPickup] GameFinishPanel tidak ditemukan! Assign di Inspector.");
        }

        // Sembunyikan / hancurkan item
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, "FINISH");
        #endif
    }
}
