using UnityEngine;

/// <summary>
/// Checkpoint — letakkan di GameObject dengan CircleCollider2D (IsTrigger = true).
/// Saat Player menyentuh checkpoint:
///   1. Set spawn point ke posisi checkpoint ini.
///   2. (Opsional) Unlock senjata tertentu di WeaponManager.
///   3. Ubah warna lingkaran menjadi hijau (sudah diaktifkan).
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Pengaturan Checkpoint")]
    [Tooltip("Index senjata yang di-unlock saat checkpoint ini disentuh. Isi -1 jika tidak ada senjata yang di-unlock.")]
    [SerializeField] private int weaponIndexToUnlock = -1;

    [Tooltip("Jika true, senjata yang di-unlock langsung dipakai otomatis.")]
    [SerializeField] private bool autoEquipWeapon = true;

    [Header("Visual Lingkaran")]
    [Tooltip("Radius lingkaran checkpoint (satuan Unity).")]
    [SerializeField] private float circleRadius = 1f;

    [Tooltip("Warna checkpoint saat belum disentuh.")]
    [SerializeField] private Color colorInactive = new Color(1f, 0.9f, 0f, 0.7f); // Kuning

    [Tooltip("Warna checkpoint setelah disentuh / aktif.")]
    [SerializeField] private Color colorActive = new Color(0f, 1f, 0.3f, 0.9f); // Hijau

    // Status internal
    private bool isActivated = false;
    private CircleCollider2D circleCollider;

    [Header("Referensi Visual")]
    [Tooltip("Drag SpriteRenderer lingkaran dari child 'CheckpointVisual' ke sini. Jika kosong, otomatis dicari saat runtime.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // --- Setup Collider ---
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = circleRadius;

        // --- Setup SpriteRenderer ---
        // Prioritas 1: sudah di-assign manual dari Inspector (drag & drop)
        if (spriteRenderer == null)
        {
            // Prioritas 2: cari dari child bernama "CheckpointVisual"
            Transform visualChild = transform.Find("CheckpointVisual");
            if (visualChild != null)
            {
                spriteRenderer = visualChild.GetComponent<SpriteRenderer>();
                float diameter = circleRadius * 2f;
                visualChild.localScale = new Vector3(diameter, diameter, 1f);
            }
        }

        if (spriteRenderer == null)
        {
            // Prioritas 3: buat otomatis jika belum ada sama sekali
            GameObject visual = new GameObject("CheckpointVisual");
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = Vector3.zero;

            spriteRenderer = visual.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 0;

            float diameter = circleRadius * 2f;
            visual.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // Jika SpriteRenderer tidak punya sprite, auto-assign lingkaran
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetUnityCircleSprite();
            Debug.Log("[Checkpoint] Sprite kosong! Auto-assign sprite lingkaran.");
        }

        // Pastikan sorting order cukup tinggi agar terlihat di atas tilemap
        if (spriteRenderer.sortingOrder == 0)
            spriteRenderer.sortingOrder = 5;

        // Set warna awal (belum disentuh = kuning)
        spriteRenderer.color = colorInactive;
        Debug.Log($"[Checkpoint] Awake selesai. SpriteRenderer={spriteRenderer.name}, Warna={colorInactive}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hanya proses sekali & hanya saat Player yang masuk
        if (isActivated) return;
        if (!other.CompareTag("Player")) return;

        isActivated = true;

        // 1. Set spawn point ke posisi checkpoint ini
        PlayerRespawn.SetSpawnPoint(transform.position);
        Debug.Log($"[Checkpoint] Spawn point diperbarui ke: {transform.position}");

        // 2. Unlock senjata jika ada yang dikonfigurasi
        if (weaponIndexToUnlock >= 0)
        {
            WeaponManager wm = other.GetComponentInParent<WeaponManager>();
            if (wm == null)
                wm = FindFirstObjectByType<WeaponManager>();

            if (wm != null)
            {
                wm.UnlockWeapon(weaponIndexToUnlock, autoEquipWeapon);
                Debug.Log($"[Checkpoint] Senjata index {weaponIndexToUnlock} terbuka!");
            }
            else
            {
                Debug.LogWarning("[Checkpoint] WeaponManager tidak ditemukan di scene!");
            }
        }

        // 3. Ubah warna ke aktif (hijau)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorActive;
            Debug.Log($"[Checkpoint] Warna diubah ke HIJAU: {colorActive}");
        }
        else
        {
            Debug.LogError("[Checkpoint] spriteRenderer NULL saat trigger! Warna tidak bisa diubah.");
        }
    }

    // Ambil sprite lingkaran bawaan Unity dari resource internal
    private Sprite GetUnityCircleSprite()
    {
        // Unity menyediakan sprite "Knob" yang merupakan lingkaran putih
        // Ini tersedia via Resources internal Unity UI
        Sprite circle = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        if (circle == null)
        {
            // Fallback: buat texture lingkaran secara manual
            circle = CreateCircleSprite(64);
        }
        return circle;
    }

    // Fallback: generate sprite lingkaran dari texture
    private Sprite CreateCircleSprite(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
        float center = resolution / 2f;
        float radius = resolution / 2f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                pixels[y * resolution + x] = (dist <= radius) ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    // Tampilkan garis besar lingkaran di Editor (bahkan saat objek tidak dipilih)
    private void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? colorActive : colorInactive;
        Gizmos.DrawWireSphere(transform.position, circleRadius);
    }

    // Tampilkan label nama saat dipilih di Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, circleRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (circleRadius + 0.2f),
            weaponIndexToUnlock >= 0
                ? $"Checkpoint\nUnlock Senjata Slot {weaponIndexToUnlock}"
                : "Checkpoint\n(Tidak ada senjata)"
        );
#endif
    }
}
