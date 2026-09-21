using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform heartContainer; // Transform tempat meletakkan hati
    [SerializeField] private GameObject heartPrefab;    // Prefab UI Image Hati
    [SerializeField] private Sprite fullHeartSprite;   // Sprite Hati Penuh
    [SerializeField] private Sprite emptyHeartSprite;  // Sprite Hati Kosong (Opsional)

    private List<Image> heartImages = new List<Image>();

    // Panggil fungsi ini saat awal Game (Awake/Start)
    public void SetupHearts(int maxHealth)
    {
        // Bersihkan child lama jika ada
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }
        heartImages.Clear();

        // Spawn gambar hati sebanyak maxHealth
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            Image img = newHeart.GetComponent<Image>();
            img.sprite = fullHeartSprite;
            heartImages.Add(img);
        }
    }

    // Panggil fungsi ini setiap kali HP berubah
    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].color = Color.white; // Warna normal
            }
            else
            {
                if (emptyHeartSprite != null)
                {
                    heartImages[i].sprite = emptyHeartSprite;
                }
                else
                {
                    // Jika tidak punya sprite hati kosong, buat hati merah jadi transparan/hitam
                    heartImages[i].color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
        }
    }
}