using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HealthUI - Menampilkan indikator darah Player di pojok kiri atas layar.
/// Mendukung Health Container (deretan gambar ikon hati pixel) 
/// dan/atau teks jumlah darah ("HP: 5 / 5").
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("Heart Container Settings (Deretan Gambar Hati)")]
    [Tooltip("GameObject container penampung hati di Canvas (gunakan Horizontal Layout Group)")]
    [SerializeField] private Transform heartContainer;

    [Tooltip("Prefab UI Image Hati (misal HeartImage.prefab)")]
    [SerializeField] private GameObject heartPrefab;

    [Tooltip("Sprite hati merah penuh")]
    [SerializeField] private Sprite fullHeartSprite;

    [Tooltip("Sprite hati kosong/abu-abu (opsional)")]
    [SerializeField] private Sprite emptyHeartSprite;

    [Tooltip("Ukuran tiap gambar hati di layar (lebar, tinggi)")]
    [SerializeField] private Vector2 heartSize = new Vector2(36f, 36f);

    [Header("UI Text (Opsional)")]
    [Tooltip("Teks opsional untuk menampilkan angka darah, contoh: 'HP: 5 / 5'. Kosongkan jika hanya ingin gambar hati.")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private string textPrefix = "HP: ";

    // Daftar icon hati yang sedang aktif di layar
    private readonly List<Image> spawnedHearts = new List<Image>();

    // Cache referensi PlayerHealth
    private PlayerHealth playerHealth;
    private int lastMaxHealth = -1;
    private int lastCurrentHealth = -1;

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        FindPlayerHealth();
        if (playerHealth != null)
        {
            SetupHearts(playerHealth.MaxHealth);
            UpdateDisplay(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void Update()
    {
        if (playerHealth == null || !playerHealth.gameObject.activeInHierarchy)
        {
            FindPlayerHealth();
        }

        if (playerHealth != null)
        {
            // Re-setup jika max health berubah
            if (playerHealth.MaxHealth != lastMaxHealth)
            {
                SetupHearts(playerHealth.MaxHealth);
            }

            // Update jika health berubah
            if (playerHealth.CurrentHealth != lastCurrentHealth)
            {
                UpdateDisplay(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
    }

    private void FindPlayerHealth()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = playerObj.GetComponentInChildren<PlayerHealth>();
            }
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }
    }

    /// <summary>
    /// Buat gambar hati sejumlah maxHealth di dalam heartContainer
    /// </summary>
    public void SetupHearts(int maxHealth)
    {
        lastMaxHealth = maxHealth;

        if (heartContainer == null) return;

        // Bersihkan child lama jika ada
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedHearts.Clear();

        if (maxHealth <= 0) return;

        // Buat deretan hati baru
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newHeart;
            if (heartPrefab != null)
            {
                newHeart = Instantiate(heartPrefab, heartContainer);
            }
            else
            {
                newHeart = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                newHeart.transform.SetParent(heartContainer, false);
            }

            newHeart.name = $"Heart_{i}";

            // Pastikan RectTransform berukuran pas untuk Canvas layar
            RectTransform rt = newHeart.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.sizeDelta = heartSize;
            }

            Image img = newHeart.GetComponent<Image>();
            if (img != null)
            {
                if (fullHeartSprite != null)
                {
                    img.sprite = fullHeartSprite;
                }
                img.color = Color.white;
                spawnedHearts.Add(img);
            }
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (max != lastMaxHealth)
        {
            SetupHearts(max);
        }
        UpdateDisplay(current, max);
    }

    private void UpdateDisplay(int current, int max)
    {
        lastCurrentHealth = current;

        // 1. Update gambar deretan hati
        for (int i = 0; i < spawnedHearts.Count; i++)
        {
            if (spawnedHearts[i] == null) continue;

            if (i < current)
            {
                // Hati penuh (merah)
                if (fullHeartSprite != null)
                {
                    spawnedHearts[i].sprite = fullHeartSprite;
                }
                spawnedHearts[i].color = Color.white;
            }
            else
            {
                // Hati kosong/hilang
                if (emptyHeartSprite != null)
                {
                    spawnedHearts[i].sprite = emptyHeartSprite;
                    spawnedHearts[i].color = Color.white;
                }
                else
                {
                    // Jika tidak punya sprite hati kosong, buat hati menjadi redup / hitam transparan
                    spawnedHearts[i].color = new Color(0.2f, 0.2f, 0.2f, 0.4f);
                }
            }
        }

        // 2. Update teks opsional jika ada
        if (healthText != null)
        {
            healthText.text = $"{textPrefix}{current} / {max}";
            healthText.color = (current <= 2 && current > 0) ? new Color(1f, 0.3f, 0.3f, 1f) : Color.white;
        }
    }
}
