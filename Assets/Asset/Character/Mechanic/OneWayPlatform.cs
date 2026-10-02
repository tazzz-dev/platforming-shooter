using System.Collections;
using UnityEngine;

/// <summary>
/// Platform tipe 1: ONE-WAY PLATFORM (hanya bisa dinaiki dari bawah, tidak bisa turun)
/// - Player bisa lompat menembus dari bawah ke atas
/// - Setelah mendarat di atas, Player tidak bisa turun menekan S
/// 
/// Cara setup:
/// 1. Tambahkan script ini ke GameObject platform
/// 2. Pastikan ada PlatformEffector2D di GameObject yang sama (otomatis di-setup oleh script ini)
/// 3. Pastikan Composite Collider atau Collider2D menggunakan "Used By Effector" = true
/// </summary>
[RequireComponent(typeof(PlatformEffector2D))]
public class OneWayPlatform : MonoBehaviour
{
    private PlatformEffector2D effector;
    private Collider2D platformCollider;

    private void Awake()
    {
        effector = GetComponent<PlatformEffector2D>();

        // Cari Collider2D apapun yang ada (BoxCollider2D, TilemapCollider2D, dll)
        platformCollider = GetComponent<Collider2D>();

        if (platformCollider == null)
        {
            Debug.LogError("[OneWayPlatform] Tidak ada Collider2D di GameObject ini! Tambahkan TilemapCollider2D atau BoxCollider2D terlebih dahulu.");
            return;
        }
        effector.surfaceArc = 180f; // Hanya sisi atas yang solid
        effector.useOneWayGrouping = true;

        // Collider harus pakai effector
        platformCollider.usedByEffector = true;
    }
}
