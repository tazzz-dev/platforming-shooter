using System.Collections;
using UnityEngine;

/// <summary>
/// Platform tipe 2: DROP-THROUGH PLATFORM (bisa naik dari bawah DAN bisa turun dengan pencet S)
/// - Player bisa lompat menembus dari bawah ke atas
/// - Player bisa turun dengan menekan S (tombol bawah) saat berdiri di atas platform
/// 
/// Cara setup:
/// 1. Tambahkan script ini ke GameObject platform
/// 2. Pastikan ada PlatformEffector2D (otomatis di-setup)
/// 3. Script ini akan di-trigger dari PlayerMovement saat player pencet S
/// 
/// Cara kerja teknis:
/// - Pakai PlatformEffector2D dengan useOneWay = true (sama seperti OneWayPlatform)
/// - Saat player menekan S: collider sementara dimatikan (IgnoreCollision) selama 0.3 detik
///   sehingga player jatuh menembus platform
/// </summary>
[RequireComponent(typeof(PlatformEffector2D))]
public class DropThroughPlatform : MonoBehaviour
{
    [Tooltip("Berapa lama (detik) player menembus platform saat drop-down.")]
    [SerializeField] private float dropDuration = 0.3f;

    private PlatformEffector2D effector;
    private Collider2D platformCollider;

    public Collider2D PlatformCollider => platformCollider;

    private void Awake()
    {
        effector = GetComponent<PlatformEffector2D>();

        // Cari Collider2D apapun yang ada (BoxCollider2D, TilemapCollider2D, dll)
        platformCollider = GetComponent<Collider2D>();

        if (platformCollider == null)
        {
            Debug.LogError("[DropThroughPlatform] Tidak ada Collider2D di GameObject ini! Tambahkan TilemapCollider2D atau BoxCollider2D terlebih dahulu.");
            return;
        }
        effector.surfaceArc = 180f;
        effector.useOneWayGrouping = true;

        platformCollider.usedByEffector = true;
    }

    /// <summary>
    /// Dipanggil oleh PlayerMovement saat player tekan S di atas platform ini.
    /// Sementara menonaktifkan tabrakan antara player dan platform selama dropDuration detik.
    /// </summary>
    public void DropThrough(Collider2D playerCollider)
    {
        StartCoroutine(DropRoutine(playerCollider));
    }

    private IEnumerator DropRoutine(Collider2D playerCollider)
    {
        // Abaikan collision antara player dan platform ini sementara
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

        yield return new WaitForSeconds(dropDuration);

        // Aktifkan kembali collision
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
}
