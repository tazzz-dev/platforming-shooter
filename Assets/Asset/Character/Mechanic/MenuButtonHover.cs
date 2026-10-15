using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Efek Hover komprehensif untuk tombol UI Menu:
/// 1. Mengubah warna teks TextMeshPro saat mouse hover.
/// 2. Mengubah warna background Image tombol saat mouse hover.
/// 3. Efek animasi membesar sedikit (scale zoom-in) yang halus.
/// 4. (Opsional) Menampilkan penanda panah atau icon saat disorot.
/// </summary>
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Warna Teks (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Color textNormalColor = new Color(0.95f, 0.85f, 0.55f, 1f); // Emas Pucat
    [SerializeField] private Color textHoverColor = new Color(1f, 0.25f, 0.25f, 1f);      // Merah Menyala

    [Header("Warna Background Kotak (Image)")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color bgNormalColor = new Color(0f, 0f, 0f, 0.35f);          // Hitam transparan
    [SerializeField] private Color bgHoverColor = new Color(0.4f, 0.05f, 0.05f, 0.65f);  // Merah gelap anggun

    [Header("Animasi Scale Pop (Membesar Halus)")]
    [SerializeField] private bool enableScaleEffect = true;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Indikator Tambahan (Opsional)")]
    [Tooltip("Objek panah atau icon yang hanya muncul saat tombol disorot (misal '▶')")]
    [SerializeField] private GameObject hoverIndicator;

    [Header("Audio SFX (Opsional)")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 0.8f;

    private Vector3 targetScale = Vector3.one;
    private bool isHovered = false;

    private void Awake()
    {
        // Otomatis cari Text dan Image jika belum di-drag manual di Inspector
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();

        SetVisuals(false);

        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (enableScaleEffect)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        SetVisuals(true);

        if (enableScaleEffect)
        {
            targetScale = Vector3.one * hoverScale;
        }

        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(true);
        }

        if (hoverSound != null)
        {
            AudioSource.PlayClipAtPoint(hoverSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero, soundVolume);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        SetVisuals(false);

        if (enableScaleEffect)
        {
            targetScale = Vector3.one;
        }

        if (hoverIndicator != null)
        {
            hoverIndicator.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Sedikit mengecil saat ditekan/diklik (responsif)
        if (enableScaleEffect)
        {
            targetScale = Vector3.one * 0.98f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (enableScaleEffect)
        {
            targetScale = isHovered ? Vector3.one * hoverScale : Vector3.one;
        }
    }

    private void SetVisuals(bool hovered)
    {
        if (buttonText != null)
        {
            buttonText.color = hovered ? textHoverColor : textNormalColor;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = hovered ? bgHoverColor : bgNormalColor;
        }
    }
}
