using TMPro;
using UnityEngine;

/// <summary>
/// Menampilkan info peluru senjata aktif di pojok kanan bawah layar.
/// Tampilkan: "[currentAmmo] / [maxAmmo]" dan tanda "RELOADING..." saat reload.
///
/// Setup:
/// 1. Buat Canvas (Screen Space - Overlay) jika belum ada.
/// 2. Tambahkan 2 TextMeshPro-UGUI di dalam Canvas:
///    - ammoText  : menampilkan "7 / 7"
///    - reloadText: menampilkan "RELOADING..."
/// 3. Attach script ini ke GameObject manapun (misal GameManager atau Canvas).
/// 4. Drag kedua TMP text ke slot di Inspector.
/// </summary>
public class AmmoUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text untuk menampilkan jumlah peluru, contoh: '7 / 7'")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Tooltip("Text yang muncul saat sedang reload, contoh: 'RELOADING...'")]
    [SerializeField] private TextMeshProUGUI reloadText;

    // Cache komponen senjata aktif
    private Pistol   activePistol;
    private Shotgun  activeShotgun;
    private Uzi      activeUzi;

    private void OnEnable()
    {
        WeaponManager.OnWeaponChanged += HandleWeaponChanged;
    }

    private void OnDisable()
    {
        WeaponManager.OnWeaponChanged -= HandleWeaponChanged;
    }

    private void Start()
    {
        // Cari senjata yang aktif saat game mulai
        RefreshActiveWeapon();
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    /// <summary>
    /// Dipanggil otomatis saat WeaponManager ganti senjata.
    /// </summary>
    private void HandleWeaponChanged(int index, string weaponName)
    {
        RefreshActiveWeapon();
    }

    /// <summary>
    /// Cari senjata yang sedang aktif (enabled) di scene.
    /// </summary>
    private void RefreshActiveWeapon()
    {
        activePistol  = null;
        activeShotgun = null;
        activeUzi     = null;

        // Cari senjata aktif (hanya yang GameObject-nya enabled)
        Pistol[] pistols = FindObjectsByType<Pistol>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (pistols.Length > 0) activePistol = pistols[0];

        Shotgun[] shotguns = FindObjectsByType<Shotgun>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (shotguns.Length > 0) activeShotgun = shotguns[0];

        Uzi[] uzis = FindObjectsByType<Uzi>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (uzis.Length > 0) activeUzi = uzis[0];
    }

    /// <summary>
    /// Update tampilan teks setiap frame berdasarkan senjata aktif.
    /// </summary>
    private void UpdateUI()
    {
        int  current    = 0;
        int  max        = 0;
        bool reloading  = false;

        // Ambil data dari senjata yang aktif
        if (activePistol != null)
        {
            current   = activePistol.CurrentAmmo;
            max       = activePistol.MaxAmmo;
            reloading = activePistol.IsReloading;
        }
        else if (activeShotgun != null)
        {
            current   = activeShotgun.CurrentAmmo;
            max       = activeShotgun.MaxAmmo;
            reloading = activeShotgun.IsReloading;
        }
        else if (activeUzi != null)
        {
            current   = activeUzi.CurrentAmmo;
            max       = activeUzi.MaxAmmo;
            reloading = activeUzi.IsReloading;
        }

        // Update teks peluru
        if (ammoText != null)
        {
            ammoText.text = $"{current} / {max}";

            // Warna merah kalau peluru sedikit (<= 25% dari max), putih kalau normal
            ammoText.color = (max > 0 && current <= max * 0.25f)
                ? Color.red
                : Color.white;
        }

        // Tampilkan / sembunyikan teks RELOADING
        if (reloadText != null)
        {
            reloadText.gameObject.SetActive(reloading);
        }
    }
}
