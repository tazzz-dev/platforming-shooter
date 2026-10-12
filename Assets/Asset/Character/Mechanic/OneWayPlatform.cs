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
        SetupEffector();
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
            effector.surfaceArc = 160f; // 160 derajat memastikan hanya atas yang solid
            effector.useOneWayGrouping = true;
        }

        // Cari Composite Collider jika ada di Tilemap
        CompositeCollider2D composite = GetComponent<CompositeCollider2D>();
        if (composite != null)
        {
            composite.usedByEffector = true;
            return;
        }

        // Jika tidak pakai composite, cari Collider2D biasa (TilemapCollider2D / BoxCollider2D)
        platformCollider = GetComponent<Collider2D>();
        if (platformCollider != null)
        {
            platformCollider.usedByEffector = true;
        }
        else
        {
            Debug.LogError("[OneWayPlatform] Tidak ada Collider2D atau CompositeCollider2D di GameObject ini!");
        }
    }
}
