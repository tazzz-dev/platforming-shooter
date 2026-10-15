using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Panel "Game Finish" yang ditampilkan saat player mengambil FinishPickup.
/// Attach ke GameObject Panel di Canvas.
/// Susun di Inspector:
///   - titleText      → TMP_Text judul  (mis. "Selamat!")
///   - messageText    → TMP_Text isi pesan  (mis. "Game sudah selesai...")
///   - panelRoot      → root Panel GameObject (yang di-toggle)
/// </summary>
public class GameFinishPanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root GameObject dari panel ini (yang akan di-show/hide)")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("TMP Text untuk judul besar, mis. 'Selamat!'")]
    [SerializeField] private TMP_Text titleText;

    [Tooltip("TMP Text untuk pesan panjang di bawah judul")]
    [SerializeField] private TMP_Text messageText;

    [Header("Konten Teks")]
    [TextArea(2, 3)]
    [SerializeField] private string titleContent = "FINISH!";

    [TextArea(4, 8)]
    [SerializeField] private string messageContent =
        "Selamat! Kamu telah menyelesaikan petualangan ini.\n\n" +
        "Terima kasih sudah bermain!\n" +
        "Game sudah berakhir — sampai jumpa di petualangan berikutnya!";

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Pengaturan Pause")]
    [Tooltip("Apakah waktu dihentikan saat panel ditampilkan?")]
    [SerializeField] private bool pauseTimeOnShow = true;

    // ─────────────────────────────────────────────

    private void Awake()
    {
        // Pastikan scale tidak 0
        transform.localScale = Vector3.one;
        if (panelRoot != null)
        {
            panelRoot.transform.localScale = Vector3.one;
            panelRoot.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // ─── Public API ───────────────────────────────

    /// <summary>Tampilkan panel Game Finish</summary>
    public void ShowPanel()
    {
        // Paksa scale ke (1, 1, 1) agar selalu terlihat jelas
        transform.localScale = Vector3.one;

        // Update teks sebelum muncul
        if (titleText   != null) titleText.text   = titleContent;
        if (messageText != null) messageText.text = messageContent;

        // Tampilkan panel
        if (panelRoot != null)
        {
            panelRoot.transform.localScale = Vector3.one;
            panelRoot.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        // Freeze waktu agar input gameplay tidak aktif
        if (pauseTimeOnShow)
            Time.timeScale = 0f;

        Debug.Log("[GameFinishPanel] Panel Finish ditampilkan.");
    }

    /// <summary>Sembunyikan panel</summary>
    public void HidePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    // ─── Tombol ──────────────────────────────────

    /// <summary>Tombol "Main Menu" — kembali ke Main Menu</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>Tombol "Keluar" — keluar dari aplikasi</summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameFinishPanel] Quit Game");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>Tombol "Restart" — ulangi scene saat ini</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
