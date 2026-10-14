using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menampilkan 3 kotak slot indikator senjata (Pistol, Shotgun, Uzi) di Canvas.
/// - Menunjukkan senjata apa saja yang sudah terbuka atau masih terkunci.
/// - Memberi highlight bingkai menyala pada senjata yang sedang aktif dipegang pemain.
/// </summary>
public class WeaponSlotsUI : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlotItem
    {
        public string weaponName;
        [Tooltip("Gambar icon senjata")]
        public Image weaponIcon;
        [Tooltip("Gambar bingkai highlight saat senjata ini sedang aktif")]
        public Image activeHighlight;
        [Tooltip("Objek/Ikon penanda terkunci (gembok atau panel gelap)")]
        public GameObject lockOverlay;
        [Tooltip("Teks nomor tombol (misal '1', '2', '3')")]
        public TextMeshProUGUI slotNumberText;
    }

    [Header("Daftar Slot Senjata")]
    [Tooltip("Slot 0 = Pistol (Tombol 1), Slot 1 = Shotgun (Tombol 2), Slot 2 = Uzi (Tombol 3)")]
    [SerializeField] private List<WeaponSlotItem> slotItems = new List<WeaponSlotItem>();

    [Header("Warna Status Icon")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 0.6f); // Gelap redup
    [SerializeField] private Color activeHighlightColor = new Color(1f, 0.85f, 0.2f, 1f); // Kuning emas

    private WeaponManager weaponManager;

    private void OnEnable()
    {
        WeaponManager.OnWeaponChanged += HandleWeaponChanged;
        WeaponManager.OnWeaponUnlocked += HandleWeaponUnlocked;
    }

    private void OnDisable()
    {
        WeaponManager.OnWeaponChanged -= HandleWeaponChanged;
        WeaponManager.OnWeaponUnlocked -= HandleWeaponUnlocked;
    }

    private void Start()
    {
        FindWeaponManager();
        RefreshAllSlots();
    }

    private void Update()
    {
        if (weaponManager == null)
        {
            FindWeaponManager();
            RefreshAllSlots();
        }
    }

    private void FindWeaponManager()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
    }

    /// <summary>
    /// Update status semua slot (terkunci, terbuka, dan highlight senjata aktif)
    /// </summary>
    public void RefreshAllSlots()
    {
        if (weaponManager == null) return;

        int activeIndex = weaponManager.GetCurrentWeaponIndex();
        string activeWeaponName = weaponManager.GetCurrentWeaponName();

        for (int i = 0; i < slotItems.Count; i++)
        {
            // Cek status unlock berdasarkan nama senjata (jika diisi) atau index
            bool isUnlocked;
            bool isActive;

            if (!string.IsNullOrEmpty(slotItems[i].weaponName))
            {
                isUnlocked = weaponManager.IsWeaponNameUnlocked(slotItems[i].weaponName);
                isActive = slotItems[i].weaponName.Equals(activeWeaponName, System.StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                isUnlocked = weaponManager.IsWeaponUnlocked(i);
                isActive = (i == activeIndex);
            }

            UpdateSlotVisual(slotItems[i], isUnlocked, isActive);
        }
    }

    private void UpdateSlotVisual(WeaponSlotItem slot, bool isUnlocked, bool isActive)
    {
        if (slot == null) return;

        // 1. Icon Senjata (terang jika terbuka, gelap jika terkunci)
        if (slot.weaponIcon != null)
        {
            slot.weaponIcon.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // 2. Lock Overlay (aktif jika senjata masih terkunci)
        if (slot.lockOverlay != null)
        {
            slot.lockOverlay.SetActive(!isUnlocked);
        }

        // 3. Highlight Bingkai Senjata Aktif (hanya menyala jika terbuka & sedang aktif dipakai)
        if (slot.activeHighlight != null)
        {
            slot.activeHighlight.gameObject.SetActive(isActive && isUnlocked);
            if (isActive)
            {
                slot.activeHighlight.color = activeHighlightColor;
            }
        }
    }

    private void HandleWeaponChanged(int activeIndex, string weaponName)
    {
        RefreshAllSlots();
    }

    private void HandleWeaponUnlocked(string weaponName, int kills)
    {
        RefreshAllSlots();

        // Cari slot yang baru terbuka untuk efek pop scale
        for (int i = 0; i < slotItems.Count; i++)
        {
            if (slotItems[i].weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (slotItems[i].weaponIcon != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(PopScaleRoutine(slotItems[i].weaponIcon.rectTransform));
                }
                break;
            }
        }
    }

    private System.Collections.IEnumerator PopScaleRoutine(RectTransform target)
    {
        if (target == null) yield break;

        Vector3 originalScale = Vector3.one;
        Vector3 maxScale = originalScale * 1.35f;
        float duration = 0.25f;
        float elapsed = 0f;

        // Scale Up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(originalScale, maxScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        // Scale Down
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(maxScale, originalScale, elapsed / duration);
            yield return null;
        }

        target.localScale = originalScale;
    }
}
