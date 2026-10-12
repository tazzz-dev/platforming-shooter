using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Tuliskan nama scene gameplay kamu persis sama dengan nama file scene-nya")]
    [SerializeField] private string gameplaySceneName = "SampleScene";

    [Header("Panel Tambahan (Opsional)")]
    [SerializeField] private GameObject controlsPanel; // Panel popup cara main / tombol kontrol

    /// <summary>
    /// Dipanggil saat tombol PLAY ditekan
    /// </summary>
    public void PlayGame()
    {
        // Pastikan waktu game berjalan normal (jika sebelumnya sempat di-pause)
        Time.timeScale = 1f;

        // Muat scene gameplay
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Membuka / Menutup panel petunjuk kontrol (How To Play)
    /// </summary>
    public void ToggleControlsPanel(bool isOpen)
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(isOpen);
        }
    }

    /// <summary>
    /// Dipanggil saat tombol QUIT ditekan
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[MainMenu] Keluar dari Game!");
        Application.Quit();
    }
}
