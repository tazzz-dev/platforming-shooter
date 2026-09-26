using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.5f, -10f);

    [Header("Dead Zone Settings (Toleransi Kotak/Tiles)")]
    [Tooltip("Jarak toleransi horizontal dan vertikal sebelum kamera mulai mengikuti (3 kotak = isi 3)")]
    [SerializeField] private Vector2 deadZone = new Vector2(3f, 2f);

    [Header("Camera Movement Smoothness")]
    [Tooltip("Seberapa halus kamera bergerak (semakin besar semakin cepat merespons)")]
    [SerializeField] private float smoothSpeed = 5f;

    private void Start()
    {
        // Otomatis mencari Player jika belum dipasang di Inspector
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position;

        // Hitung jarak pemain dari titik tengah kamera
        float deltaX = targetPos.x - currentPos.x;
        float deltaY = targetPos.y - (currentPos.y - offset.y);

        Vector3 desiredPos = currentPos;

        // 1. Cek batas Horizontal (Jika jarak lebih dari deadZone.x / 3 kotak)
        if (Mathf.Abs(deltaX) > deadZone.x)
        {
            float directionX = Mathf.Sign(deltaX);
            desiredPos.x = targetPos.x - (directionX * deadZone.x);
        }

        // 2. Cek batas Vertikal (Lompat / Jatuh)
        if (Mathf.Abs(deltaY) > deadZone.y)
        {
            float directionY = Mathf.Sign(deltaY);
            desiredPos.y = targetPos.y - (directionY * deadZone.y) + offset.y;
        }

        // Pastikan posisi Z kamera tetap berada di belakang layar (-10)
        desiredPos.z = offset.z;

        // Geser posisi kamera dengan halus (Smooth Lerp)
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }

    // Menampilkan kotak toleransi (Dead Zone) berwarna kuning di jendela Scene Unity
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        center.z = 0f;
        center.y += offset.y;
        Gizmos.DrawWireCube(center, new Vector3(deadZone.x * 2f, deadZone.y * 2f, 0f));
    }
}
