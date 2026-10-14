using UnityEngine;

/// <summary>
/// Trigger untuk mengubah zoom kamera secara otomatis saat Player masuk ke area tertentu.
/// 
/// Cara pakai:
/// 1. Buat GameObject kosong di Scene (misal: "ZoomArea_Top").
/// 2. Pasang script ini ke GameObject tersebut (otomatis menambahkan BoxCollider2D IsTrigger).
/// 3. Geser dan ubah ukuran kotak BoxCollider2D sesuai area yang ingin memicu zoom.
/// 4. Atur Target Zoom di Inspector:
///    - Angka lebih KECIL (misal 4 atau 5) = Kamera ZOOM IN (mendekat/fokus ke karakter)
///    - Angka lebih BESAR (misal 9 atau 11) = Kamera ZOOM OUT (pemandangan jadi luas/jauh)
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraZoomTrigger : MonoBehaviour
{
    [Header("Pengaturan Zoom Area")]
    [Tooltip("Ukuran zoom yang diinginkan saat player berada di area ini. Angka kecil = zoom in (dekat), angka besar = zoom out (luas).")]
    [SerializeField] private float targetZoom = 5f;

    [Tooltip("Kecepatan transisi kamera saat zoom.")]
    [SerializeField] private float transitionSpeed = 3f;

    [Tooltip("Jika true, saat player keluar dari area ini kamera akan otomatis kembali ke ukuran zoom semula.")]
    [SerializeField] private bool revertOnExit = true;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.SetZoom(targetZoom, transitionSpeed);
                Debug.Log($"[CameraZoomTrigger] Player memasuki area zoom '{gameObject.name}' -> Zoom diubah ke {targetZoom}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (revertOnExit && IsPlayer(other))
        {
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.ResetZoom(transitionSpeed);
                Debug.Log($"[CameraZoomTrigger] Player keluar dari area zoom '{gameObject.name}' -> Zoom dikembalikan ke semula.");
            }
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null;
    }

    // Visualisasi area trigger di Scene View agar mudah disesuaikan ukurannya
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.35f); // Warna biru transparan
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Vector3 center = transform.position + (Vector3)col.offset;
            Gizmos.DrawCube(center, col.size);
            Gizmos.color = new Color(0f, 0.7f, 1f, 0.9f);
            Gizmos.DrawWireCube(center, col.size);
        }
    }
}
