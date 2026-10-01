using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public string weaponName;
        public GameObject weaponObject;
        public bool isUnlocked = true;
    }

    [Header("Daftar Senjata")]
    [Tooltip("Slot 0 = Tombol 1 (Pistol), Slot 1 = Tombol 2 (Shotgun), Slot 2 = Tombol 3 (Uzi)")]
    [SerializeField] private List<WeaponSlot> weapons = new List<WeaponSlot>();

    [Header("Senjata Awal")]
    [SerializeField] private int currentWeaponIndex = 0;

    [Header("Pengaturan Input")]
    [SerializeField] private bool allowMouseScroll = true;

    // Event untuk UI jika ingin menampilkan ikon/nama senjata aktif
    public static event Action<int, string> OnWeaponChanged;

    private void Awake()
    {
        // 1. Jika pengguna memasukkan file Prefab dari tab Project, otomatis alihkan ke objek yang ada di Hierarchy / Scene
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weaponObject != null)
            {
                // Jika objek berasal dari Asset Project (bukan objek di scene)
                if (!weapons[i].weaponObject.scene.IsValid())
                {
                    string targetName = weapons[i].weaponObject.name;
                    GameObject sceneInstance = GameObject.Find(targetName);

                    if (sceneInstance != null)
                    {
                        Debug.Log($"[WeaponManager] Berhasil menghubungkan slot '{weapons[i].weaponName}' ke objek di Hierarchy: {sceneInstance.name}");
                        weapons[i].weaponObject = sceneInstance;
                    }
                    else
                    {
                        // Jika belum ada di scene, spawn ke scene
                        GameObject newInstance = Instantiate(weapons[i].weaponObject, transform.position, Quaternion.identity);
                        newInstance.name = targetName;
                        weapons[i].weaponObject = newInstance;
                        Debug.Log($"[WeaponManager] Men-spawn senjata baru ke scene: {targetName}");
                    }
                }
            }
        }

        // 2. Jika daftar senjata kosong, otomatis cari Pistol, Shotgun, Uzi yang ada di Hierarchy
        if (weapons.Count == 0)
        {
            string[] defaultGunNames = { "Pistol", "Shotgun", "Uzi" };
            foreach (string gName in defaultGunNames)
            {
                GameObject gObj = GameObject.Find(gName);
                if (gObj != null)
                {
                    weapons.Add(new WeaponSlot { weaponName = gName, weaponObject = gObj, isUnlocked = true });
                    Debug.Log($"[WeaponManager] Otomatis mendeteksi '{gName}' dari Hierarchy!");
                }
            }
        }
    }

    private void Start()
    {
        // Pastikan hanya senjata aktif yang menyala saat mulai game
        EquipWeapon(currentWeaponIndex);
    }

    private void Update()
    {
        HandleNumberInput();

        if (allowMouseScroll)
        {
            HandleScrollInput();
        }
    }

    private void HandleNumberInput()
    {
        if (Keyboard.current == null) return;

        // Tombol 1 -> Slot 0
        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            Debug.Log("[WeaponManager] Tombol 1 ditekan");
            TrySwitchWeapon(0);
        }
        // Tombol 2 -> Slot 1
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            Debug.Log("[WeaponManager] Tombol 2 ditekan");
            TrySwitchWeapon(1);
        }
        // Tombol 3 -> Slot 2
        else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            Debug.Log("[WeaponManager] Tombol 3 ditekan");
            TrySwitchWeapon(2);
        }
        // Tombol 4 -> Slot 3 (untuk senjata masa depan)
        else if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame)
        {
            Debug.Log("[WeaponManager] Tombol 4 ditekan");
            TrySwitchWeapon(3);
        }
    }

    private void HandleScrollInput()
    {
        if (Mouse.current == null) return;

        float scrollY = Mouse.current.scroll.ReadValue().y;
        if (scrollY > 0f)
        {
            // Scroll Atas -> Senjata Berikutnya
            SwitchToNextWeapon();
        }
        else if (scrollY < 0f)
        {
            // Scroll Bawah -> Senjata Sebelumnya
            SwitchToPreviousWeapon();
        }
    }

    public void TrySwitchWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
        {
            Debug.LogWarning($"[WeaponManager] Slot senjata nomor {index + 1} belum didaftarkan di Inspector!");
            return;
        }

        // Cek apakah senjata sudah di-unlock
        if (!weapons[index].isUnlocked)
        {
            Debug.Log($"[WeaponManager] Senjata '{weapons[index].weaponName}' masih terkunci!");
            return;
        }

        // Jika sudah aktif, tidak perlu ganti lagi
        if (index == currentWeaponIndex && weapons[index].weaponObject != null && weapons[index].weaponObject.activeSelf)
        {
            return;
        }

        EquipWeapon(index);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;

        // 1. Matikan SEMUA objek senjata yang ada di scene (termasuk jika ada pistol duplikat di dalam/luar Player)
        Pistol[] allPistols = FindObjectsByType<Pistol>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in allPistols)
        {
            p.gameObject.SetActive(false);
        }

        Shotgun[] allShotguns = FindObjectsByType<Shotgun>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var s in allShotguns)
        {
            s.gameObject.SetActive(false);
        }

        Uzi[] allUzis = FindObjectsByType<Uzi>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var u in allUzis)
        {
            u.gameObject.SetActive(false);
        }

        // 2. Matikan juga semua objek yang terdaftar di list
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weaponObject != null)
            {
                weapons[i].weaponObject.SetActive(false);
            }
        }

        // 3. Aktifkan HANYA senjata yang dipilih
        if (weapons[index].weaponObject != null)
        {
            weapons[index].weaponObject.SetActive(true);
            currentWeaponIndex = index;
            Debug.Log($"[WeaponManager] Menggunakan: {weapons[index].weaponName}");
            OnWeaponChanged?.Invoke(index, weapons[index].weaponName);
        }
    }

    public void SwitchToNextWeapon()
    {
        if (weapons.Count <= 1) return;

        int nextIndex = currentWeaponIndex;
        for (int i = 0; i < weapons.Count; i++)
        {
            nextIndex = (nextIndex + 1) % weapons.Count;
            if (weapons[nextIndex].isUnlocked && weapons[nextIndex].weaponObject != null)
            {
                EquipWeapon(nextIndex);
                break;
            }
        }
    }

    public void SwitchToPreviousWeapon()
    {
        if (weapons.Count <= 1) return;

        int prevIndex = currentWeaponIndex;
        for (int i = 0; i < weapons.Count; i++)
        {
            prevIndex = (prevIndex - 1 + weapons.Count) % weapons.Count;
            if (weapons[prevIndex].isUnlocked && weapons[prevIndex].weaponObject != null)
            {
                EquipWeapon(prevIndex);
                break;
            }
        }
    }

    // ==========================================
    // SISTEM UNLOCK (UNTUK CHECKPOINT NANTI)
    // ==========================================

    /// <summary>
    /// Membuka senjata berdasarkan index slot.
    /// </summary>
    public void UnlockWeapon(int index, bool autoEquip = true)
    {
        if (index < 0 || index >= weapons.Count) return;

        weapons[index].isUnlocked = true;
        Debug.Log($"[WeaponManager] Senjata '{weapons[index].weaponName}' TERBUKA!");

        if (autoEquip)
        {
            EquipWeapon(index);
        }
    }

    /// <summary>
    /// Membuka senjata berdasarkan nama.
    /// </summary>
    public void UnlockWeaponByName(string weaponName, bool autoEquip = true)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weaponName.Equals(weaponName, StringComparison.OrdinalIgnoreCase))
            {
                UnlockWeapon(i, autoEquip);
                return;
            }
        }
    }

    public bool IsWeaponUnlocked(int index)
    {
        if (index >= 0 && index < weapons.Count)
            return weapons[index].isUnlocked;
        return false;
    }

    public int GetCurrentWeaponIndex() => currentWeaponIndex;
    public string GetCurrentWeaponName() => (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count) ? weapons[currentWeaponIndex].weaponName : "";
}
