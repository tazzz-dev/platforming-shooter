using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manager untuk Checkpoint Altar.
/// Pasang script ini langsung ke GameObject 'Tilemap (10)' (atau GameObject manager lain).
/// 
/// Fitur:
/// 1. Otomatis men-scan patung/altar di Tilemap dan membuat trigger checkpoint + bulatan hijau.
/// 2. Menyalakan bulatan hijau HANYA pada checkpoint terakhir yang dilewati player.
/// 3. Mematikan bulatan hijau pada checkpoint sebelumnya jika player menyentuh checkpoint baru.
/// 4. Otomatis mengupdate spawn point ke PlayerRespawn saat checkpoint dilewati.
/// </summary>
public class AltarCheckpointManager : MonoBehaviour
{
    public static AltarCheckpointManager Instance { get; private set; }

    [Header("Pengaturan Deteksi Altar")]
    [Tooltip("Tilemap yang berisi patung altar (default: otomatis mengambil Tilemap pada GameObject ini)")]
    [SerializeField] private Tilemap altarTilemap;

    [Header("Ukuran Trigger Collider")]
    [SerializeField] private Vector2 triggerSize = new Vector2(1.2f, 2.2f);
    [Tooltip("Posisi Y bulatan hijau di atas titik tengah trigger altar")]
    [SerializeField] private float orbOffsetY = 1.35f;

    [Header("Status Saat Ini")]
    [SerializeField] private AltarCheckpoint currentActiveCheckpoint;

    // Daftar semua checkpoint altar yang terdeteksi
    private readonly List<AltarCheckpoint> registeredCheckpoints = new List<AltarCheckpoint>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (altarTilemap == null)
        {
            altarTilemap = GetComponent<Tilemap>();
        }

        // Pastikan PlayerRespawn ada di scene agar respawn pasti berjalan
        EnsurePlayerRespawnExists();

        // Scan dan inisialisasi semua altar yang ada di Tilemap
        ScanAndGenerateAltarCheckpoints();
    }

    /// <summary>
    /// Pastikan ada PlayerRespawn di scene agar tidak terjadi error saat player mati
    /// </summary>
    private void EnsurePlayerRespawnExists()
    {
        if (PlayerRespawn.Instance == null && FindFirstObjectByType<PlayerRespawn>() == null)
        {
            GameObject respawnObj = new GameObject("PlayerRespawnManager");
            respawnObj.AddComponent<PlayerRespawn>();
            Debug.Log("[AltarCheckpointManager] PlayerRespawn tidak ditemukan, otomatis membuat PlayerRespawnManager baru di scene.");
        }
    }

    /// <summary>
    /// Membaca koordinat altar dari Tilemap dan membuat trigger collider + bulatan hijau otomatis
    /// </summary>
    public void ScanAndGenerateAltarCheckpoints()
    {
        if (altarTilemap == null) return;

        BoundsInt bounds = altarTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            // Cek apakah posisi ini ada tile
            if (!altarTilemap.HasTile(pos)) continue;

            // Setiap patung setinggi 2 tile vertikal.
            // Kita cari tile dasarnya (tile yang TIDAK punya tile di bawahnya)
            Vector3Int belowPos = pos + Vector3Int.down;
            if (altarTilemap.HasTile(belowPos))
            {
                // Ini tile bagian atas patung, lewati karena akan di-cover oleh tile bawah
                continue;
            }

            // Hitung posisi tengah altar (patung tinggi 2 tile)
            Vector3 cellCenter = altarTilemap.GetCellCenterWorld(pos);
            Vector3 altarCenter = cellCenter + new Vector3(0f, 0.5f, 0f); // Titik tengah antara tile bawah dan atas
            Vector3 spawnPos = cellCenter; // Tempat player respawn (di dasar patung)

            // Cek apakah child trigger untuk koordinat ini sudah pernah dibuat sebelumnya
            string triggerName = $"AltarCheckpoint_{pos.x}_{pos.y}";
            Transform existingTrigger = transform.Find(triggerName);

            AltarCheckpoint altarCheckpoint;
            if (existingTrigger != null)
            {
                altarCheckpoint = existingTrigger.GetComponent<AltarCheckpoint>();
            }
            else
            {
                // Buat GameObject trigger baru sebagai child
                GameObject triggerObj = new GameObject(triggerName);
                triggerObj.transform.SetParent(transform, false);
                triggerObj.transform.position = altarCenter;

                // Tambahkan BoxCollider2D trigger
                BoxCollider2D boxCol = triggerObj.AddComponent<BoxCollider2D>();
                boxCol.isTrigger = true;
                boxCol.size = triggerSize;

                // Tambahkan script AltarCheckpoint
                altarCheckpoint = triggerObj.AddComponent<AltarCheckpoint>();
                altarCheckpoint.spawnPosition = spawnPos;
                altarCheckpoint.SetupIndicator(orbOffsetY);
            }

            if (altarCheckpoint != null && !registeredCheckpoints.Contains(altarCheckpoint))
            {
                registeredCheckpoints.Add(altarCheckpoint);
            }
        }

        Debug.Log($"[AltarCheckpointManager] Berhasil mendeteksi {registeredCheckpoints.Count} altar checkpoint dari Tilemap!");
    }

    /// <summary>
    /// Mengaktifkan checkpoint yang baru dilewati oleh Player.
    /// Mematikan bulatan hijau di checkpoint sebelumnya dan menyalakannya di checkpoint ini.
    /// </summary>
    public void ActivateCheckpoint(AltarCheckpoint activatedAltar)
    {
        if (activatedAltar == null) return;

        // Jika checkpoint ini sudah merupakan checkpoint yang sedang aktif, tidak perlu ubah apa-apa
        if (currentActiveCheckpoint == activatedAltar) return;

        // 1. Matikan bulatan hijau pada checkpoint sebelumnya
        if (currentActiveCheckpoint != null)
        {
            currentActiveCheckpoint.SetActiveCheckpoint(false);
            Debug.Log($"[AltarCheckpointManager] Mematikan bulatan hijau checkpoint lama di: {currentActiveCheckpoint.transform.position}");
        }

        // 2. Nyalakan bulatan hijau pada checkpoint baru ini
        currentActiveCheckpoint = activatedAltar;
        currentActiveCheckpoint.SetActiveCheckpoint(true);

        // 3. Update spawn point player ke posisi altar ini
        PlayerRespawn.SetSpawnPoint(activatedAltar.spawnPosition);

        Debug.Log($"[AltarCheckpointManager] Checkpoint aktif diperbarui! Bulatan hijau menyala di: {activatedAltar.transform.position}");
    }
}
