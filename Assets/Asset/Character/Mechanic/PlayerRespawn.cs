using System.Collections;
using UnityEngine;

/// <summary>
/// Menyimpan posisi spawn point terkini dan mengurus proses respawn Player.
/// Dipanggil oleh Checkpoint saat Player menyentuhnya, dan oleh PlayerHealth saat Player mati.
/// </summary>
public class PlayerRespawn : MonoBehaviour
{
    // Singleton — agar bisa diakses dari mana saja tanpa perlu referensi
    public static PlayerRespawn Instance { get; private set; }

    [Header("Spawn Point Default")]
    [Tooltip("Posisi spawn awal (otomatis diset ke posisi Player saat Start jika belum ada checkpoint yang disentuh).")]
    [SerializeField] private Transform defaultSpawnPoint;

    [Header("Pengaturan Respawn")]
    [Tooltip("Jeda sebelum Player muncul kembali (detik).")]
    [SerializeField] private float respawnDelay = 1.5f;

    // Posisi spawn saat ini (diubah oleh Checkpoint)
    private static Vector3 currentSpawnPoint;

    // Referensi ke objek Player
    private GameObject playerObject;

    private void Awake()
    {
        // Setup Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Cari objek Player
        playerObject = GameObject.FindGameObjectWithTag("Player");

        // Set spawn point awal:
        // Prioritas: defaultSpawnPoint yang di-assign > posisi Player saat ini > origin
        if (defaultSpawnPoint != null)
        {
            currentSpawnPoint = defaultSpawnPoint.position;
        }
        else if (playerObject != null)
        {
            currentSpawnPoint = playerObject.transform.position;
        }
        else
        {
            currentSpawnPoint = Vector3.zero;
        }

        Debug.Log($"[PlayerRespawn] Spawn point awal: {currentSpawnPoint}");
    }

    /// <summary>
    /// Dipanggil oleh Checkpoint saat Player menyentuhnya.
    /// Bisa dipanggil sebagai static method tanpa butuh Instance.
    /// </summary>
    public static void SetSpawnPoint(Vector3 position)
    {
        currentSpawnPoint = position;
        Debug.Log($"[PlayerRespawn] Spawn point diperbarui ke: {position}");
    }

    /// <summary>
    /// Dipanggil oleh PlayerHealth saat Player mati.
    /// Memulai proses respawn dengan delay.
    /// </summary>
    public void Respawn()
    {
        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag("Player");

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log($"[PlayerRespawn] Player mati! Respawn dalam {respawnDelay} detik...");

        // Tunggu sebentar sebelum respawn
        yield return new WaitForSeconds(respawnDelay);

        if (playerObject != null)
        {
            // Pindahkan Player ke posisi spawn
            playerObject.transform.position = currentSpawnPoint;

            // Aktifkan kembali Player (jika sempat dinonaktifkan)
            playerObject.SetActive(true);

            // Reset health Player
            PlayerHealth health = playerObject.GetComponent<PlayerHealth>();
            if (health != null)
                health.ResetHealth();

            // Pastikan Rigidbody tidak membawa momentum saat respawn
            Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            Debug.Log($"[PlayerRespawn] Player respawn di: {currentSpawnPoint}");
        }
    }
}
