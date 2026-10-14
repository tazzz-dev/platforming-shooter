using UnityEngine;

/// <summary>
/// Komponen trigger checkpoint untuk satu altar patung.
/// Menampilkan bulatan warna hijau di atas kepala patung saat checkpoint ini aktif.
/// </summary>
public class AltarCheckpoint : MonoBehaviour
{
    [Header("Pengaturan Posisi")]
    [Tooltip("Posisi tempat player akan spawn kembali saat respawn")]
    public Vector3 spawnPosition;

    [Header("Visual Bulatan Hijau")]
    [SerializeField] private GameObject greenOrbObject;
    [SerializeField] private SpriteRenderer orbSpriteRenderer;
    [SerializeField] private Color orbColor = new Color(0.2f, 1f, 0.4f, 0.95f); // Hijau terang menyala
    [SerializeField] private float orbSize = 0.45f;

    private bool isActiveCheckpoint = false;
    private Vector3 initialOrbLocalPos;

    public bool IsActive => isActiveCheckpoint;

    private void Awake()
    {
        SetupIndicator();
    }

    private void Start()
    {
        if (greenOrbObject != null)
        {
            initialOrbLocalPos = greenOrbObject.transform.localPosition;
            greenOrbObject.SetActive(isActiveCheckpoint);
        }
    }

    private void Update()
    {
        // Efek animasi melayang (floating) halus jika bulatan hijau sedang aktif
        if (isActiveCheckpoint && greenOrbObject != null)
        {
            float floatOffset = Mathf.Sin(Time.time * 3f) * 0.08f;
            greenOrbObject.transform.localPosition = initialOrbLocalPos + new Vector3(0f, floatOffset, 0f);
        }
    }

    /// <summary>
    /// Menyiapkan child GameObject visual bulatan hijau jika belum ada
    /// </summary>
    public void SetupIndicator(float localOffsetY = 1.35f)
    {
        if (greenOrbObject == null)
        {
            Transform existing = transform.Find("GreenOrb");
            if (existing != null)
            {
                greenOrbObject = existing.gameObject;
                orbSpriteRenderer = greenOrbObject.GetComponent<SpriteRenderer>();
            }
            else
            {
                greenOrbObject = new GameObject("GreenOrb");
                greenOrbObject.transform.SetParent(transform, false);
                greenOrbObject.transform.localPosition = new Vector3(0f, localOffsetY, 0f);

                orbSpriteRenderer = greenOrbObject.AddComponent<SpriteRenderer>();
                orbSpriteRenderer.sprite = GetCircleSprite();
                orbSpriteRenderer.color = orbColor;
                orbSpriteRenderer.sortingLayerName = "Default";
                orbSpriteRenderer.sortingOrder = 15; // Pastikan terlihat di depan patung

                greenOrbObject.transform.localScale = new Vector3(orbSize, orbSize, 1f);
            }

            initialOrbLocalPos = greenOrbObject.transform.localPosition;
            greenOrbObject.SetActive(false);
        }
    }

    /// <summary>
    /// Aktifkan atau nonaktifkan status checkpoint ini (menyalakan / mematikan bulatan hijau)
    /// </summary>
    public void SetActiveCheckpoint(bool active)
    {
        isActiveCheckpoint = active;

        if (greenOrbObject != null)
        {
            greenOrbObject.SetActive(active);

            // Efek pop scale kecil saat baru menyala
            if (active)
            {
                greenOrbObject.transform.localScale = new Vector3(orbSize * 1.3f, orbSize * 1.3f, 1f);
                StopAllCoroutines();
                StartCoroutine(ScaleDownToNormal());
            }
        }
    }

    private System.Collections.IEnumerator ScaleDownToNormal()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(orbSize * 1.3f, orbSize * 1.3f, 1f);
        Vector3 targetScale = new Vector3(orbSize, orbSize, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (greenOrbObject != null)
            {
                greenOrbObject.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            }
            yield return null;
        }

        if (greenOrbObject != null)
        {
            greenOrbObject.transform.localScale = targetScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Deteksi apakah yang menyentuh/melewati adalah Player
        bool isPlayer = other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null;
        if (!isPlayer) return;

        // Beritahukan manager bahwa checkpoint ini yang terakhir dilewati
        AltarCheckpointManager.Instance?.ActivateCheckpoint(this);
    }

    /// <summary>
    /// Generator sprite lingkaran halus bawaan Unity atau procedurally generated
    /// </summary>
    private static Sprite cachedCircleSprite;
    public static Sprite GetCircleSprite()
    {
        if (cachedCircleSprite != null) return cachedCircleSprite;

        // Coba ambil sprite lingkaran builtin Unity UI (Knob)
        Sprite builtinCircle = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        if (builtinCircle != null)
        {
            cachedCircleSprite = builtinCircle;
            return cachedCircleSprite;
        }

        // Fallback: Generate sprite lingkaran bulat bergradasi lembut
        int res = 64;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[res * res];
        float center = res * 0.5f;
        float radius = res * 0.46f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                if (dist <= radius - 1.5f)
                {
                    pixels[y * res + x] = Color.white;
                }
                else if (dist <= radius)
                {
                    float alpha = Mathf.Clamp01((radius - dist) / 1.5f);
                    pixels[y * res + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * res + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        cachedCircleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
        return cachedCircleSprite;
    }

    private void OnDrawGizmos()
    {
        // Visualisasi di Scene View
        Gizmos.color = isActiveCheckpoint ? Color.green : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(1.2f, 2f, 0f));

        if (isActiveCheckpoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + new Vector3(0f, 1.35f, 0f), 0.2f);
        }
    }
}
