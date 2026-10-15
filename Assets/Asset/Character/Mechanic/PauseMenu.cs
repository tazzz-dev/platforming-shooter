using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel UI Pause Menu yang akan dimunculkan")]
    [SerializeField] private GameObject pauseMenuUI;
    [Tooltip("Panel UI Guide / Controls yang berisi petunjuk tombol")]
    [SerializeField] private GameObject guideMenuUI;

    [Header("Scene Settings")]
    [Tooltip("Nama scene Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public static bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        // Selalu pastikan waktu berjalan normal saat scene dimulai
        Time.timeScale = 1f;
        IsPaused = false;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (guideMenuUI != null)
        {
            guideMenuUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Deteksi tombol ESC (menggunakan New Input System)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Jika sedang buka panel Guide, tekan ESC akan menutup Guide kembali ke Pause
            if (guideMenuUI != null && guideMenuUI.activeSelf)
            {
                CloseGuide();
                return;
            }

            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    /// <summary>
    /// Melanjutkan permainan (Resume)
    /// </summary>
    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (guideMenuUI != null)
        {
            guideMenuUI.SetActive(false);
        }

        Debug.Log("[PauseMenu] Game dilanjutkan (Resume)");
    }

    /// <summary>
    /// Membuka panel Guide / Controls dari Pause Menu
    /// </summary>
    public void OpenGuide()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false); // Sembunyikan tombol-tombol pause
        }

        if (guideMenuUI != null)
        {
            guideMenuUI.SetActive(true); // Tampilkan panel guide petunjuk
        }

        Debug.Log("[PauseMenu] Buka Guide / Controls");
    }

    /// <summary>
    /// Menutup panel Guide / Controls dan kembali ke Pause Menu
    /// </summary>
    public void CloseGuide()
    {
        if (guideMenuUI != null)
        {
            guideMenuUI.SetActive(false); // Sembunyikan panel guide
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true); // Tampilkan kembali tombol pause
        }

        Debug.Log("[PauseMenu] Tutup Guide / Controls");
    }

    /// <summary>
    /// Menghentikan sementara permainan (Pause)
    /// </summary>
    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Debug.Log("[PauseMenu] Game di-pause");
    }

    /// <summary>
    /// Memulai ulang scene saat ini
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Kembali ke Main Menu
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Keluar dari game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[PauseMenu] Keluar dari game!");
        Application.Quit();
    }

    private void OnDestroy()
    {
        // Pastikan timeScale selalu dikembalikan ke normal saat pindah scene
        Time.timeScale = 1f;
        IsPaused = false;
    }
}
