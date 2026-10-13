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
    [Tooltip("Berapa lama (detik) platform terbuka saat player drop-down.")]
    [SerializeField] private float dropDuration = 0.45f;

    private PlatformEffector2D effector;
    private Collider2D platformCollider;
    private CompositeCollider2D compositeCollider;
    private Coroutine dropRoutine;

    public Collider2D PlatformCollider => (platformCollider != null) ? platformCollider : compositeCollider;

    private void Awake()
    {
        SetupEffector();

        // Pastikan platform selalu solid saat scene dimulai
        // (mencegah stuck di 180° jika game di-stop saat coroutine drop sedang berjalan)
        if (effector != null)
        {
            effector.rotationalOffset = 0f;
        }
    }

    private void OnValidate()
    {
        SetupEffector();
    }

    private void SetupEffector()
    {
        effector = GetComponent<PlatformEffector2D>();
        if (effector != null)
        {
            effector.useOneWay = true;
            effector.surfaceArc = 160f;
            effector.useOneWayGrouping = true;
            effector.rotationalOffset = 0f;
        }

        compositeCollider = GetComponent<CompositeCollider2D>();
        if (compositeCollider != null)
        {
            compositeCollider.usedByEffector = true;
            return;
        }

        platformCollider = GetComponent<Collider2D>();
        if (platformCollider != null)
        {
            platformCollider.usedByEffector = true;
        }
    }

    /// <summary>
    /// Dipanggil oleh PlayerMovement saat player tekan S di atas platform ini.
    /// Membalik rotationalOffset PlatformEffector2D sehingga player otomatis jatuh menembus ke bawah.
    /// </summary>
    public void DropThrough(Collider2D playerCollider)
    {
        if (dropRoutine != null)
        {
            StopCoroutine(dropRoutine);
        }
        dropRoutine = StartCoroutine(DropRoutine(playerCollider));
    }

    private IEnumerator DropRoutine(Collider2D playerCollider)
    {
        if (effector == null) effector = GetComponent<PlatformEffector2D>();

        // Ambil SEMUA collider yang ada pada player (body, feet, dll)
        Collider2D[] allPlayerColliders = (playerCollider != null) 
            ? playerCollider.transform.root.GetComponentsInChildren<Collider2D>() 
            : new Collider2D[0];

        Collider2D activeCol = (compositeCollider != null) ? compositeCollider : platformCollider;

        // 1. Matikan collision untuk semua collider player
        if (activeCol != null)
        {
            foreach (var col in allPlayerColliders)
            {
                if (col != null && col.enabled)
                {
                    Physics2D.IgnoreCollision(col, activeCol, true);
                }
            }
        }

        // 2. Putar arah solid effector (180 derajat)
        if (effector != null)
        {
            effector.rotationalOffset = 180f;
        }

        yield return new WaitForSeconds(dropDuration);

        // 3. Kembalikan arah effector ke semula
        if (effector != null)
        {
            effector.rotationalOffset = 0f;
        }

        // 4. Nyalakan kembali collision
        if (activeCol != null)
        {
            foreach (var col in allPlayerColliders)
            {
                if (col != null)
                {
                    Physics2D.IgnoreCollision(col, activeCol, false);
                }
            }
        }

        dropRoutine = null;
    }
}
