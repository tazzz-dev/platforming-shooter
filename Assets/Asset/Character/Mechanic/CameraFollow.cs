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

    [Header("Zoom Settings")]
    [Tooltip("Ukuran zoom kamera (default 7f agar lebih luas / tidak terlalu dekat). Semakin BESAR angkanya, semakin JAUH (zoom out).")]
    [SerializeField] private float currentZoom = 7f;
    [Tooltip("Batas maksimal zoom in (paling dekat)")]
    [SerializeField] private float minZoom = 4f;
    [Tooltip("Batas maksimal zoom out (paling jauh)")]
    [SerializeField] private float maxZoom = 12f;
    [Tooltip("Kecepatan transisi zoom")]
    [SerializeField] private float zoomSmoothSpeed = 5f;

    [Header("In-Game Zoom Controls (Optional)")]
    [Tooltip("Jika aktif, bisa zoom in/out saat bermain dengan tombol [ / ] atau tombol + / - di keyboard")]
    [SerializeField] private bool allowKeyboardZoom = true;
    [SerializeField] private float keyboardZoomStep = 1f;

    private Camera cam;
    private float defaultZoom;
    private float originalZoomSmoothSpeed;

    public static CameraFollow Instance { get; private set; }
    public float DefaultZoom => defaultZoom;
    public float CurrentZoom => currentZoom;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        defaultZoom = currentZoom;
        originalZoomSmoothSpeed = zoomSmoothSpeed;

        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = currentZoom;
        }
    }

    private void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();

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

        // 3. Kontrol Zooming
        HandleZoom();
    }

    private void HandleZoom()
    {
        if (cam == null) return;

        // Input keyboard in-game jika diaktifkan (New Input System)
        if (allowKeyboardZoom && UnityEngine.InputSystem.Keyboard.current != null)
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;

            // Tombol '[' atau '-' untuk Zoom In (mendekat)
            if (kb.leftBracketKey.wasPressedThisFrame || kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame)
            {
                currentZoom = Mathf.Clamp(currentZoom - keyboardZoomStep, minZoom, maxZoom);
            }
            // Tombol ']' atau '+' untuk Zoom Out (menjauh/lebih luas)
            else if (kb.rightBracketKey.wasPressedThisFrame || kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame)
            {
                currentZoom = Mathf.Clamp(currentZoom + keyboardZoomStep, minZoom, maxZoom);
            }
        }

        // Terapkan zoom dengan transisi halus
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, currentZoom, zoomSmoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Ubah ukuran zoom kamera dari script lain atau trigger
    /// </summary>
    public void SetZoom(float newZoom, float? customSpeed = null)
    {
        currentZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        if (customSpeed.HasValue)
        {
            zoomSmoothSpeed = customSpeed.Value;
        }
    }

    /// <summary>
    /// Kembalikan zoom ke nilai default semula
    /// </summary>
    public void ResetZoom(float? customSpeed = null)
    {
        currentZoom = defaultZoom;
        zoomSmoothSpeed = customSpeed.HasValue ? customSpeed.Value : originalZoomSmoothSpeed;
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
