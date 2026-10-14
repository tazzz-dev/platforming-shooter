using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Hazard Settings")]
    [Tooltip("Jika true, menyentuh duri/hazard ini akan langsung membunuh player seketika berapapun darahnya.")]
    [SerializeField] private bool instantKill = true;

    [Tooltip("Jumlah damage yang diberikan jika instantKill dinonaktifkan.")]
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcessHazard(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessHazard(collision.gameObject);
    }

    private void ProcessHazard(GameObject target)
    {
        // Cek apakah yang menyentuh adalah Player (baik tag maupun script Player)
        bool isPlayer = target.CompareTag("Player") || target.GetComponentInParent<PlayerMovement>() != null;
        if (!isPlayer) return;

        PlayerHealth playerHealth = target.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            if (instantKill)
            {
                playerHealth.InstantKill();
            }
            else
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}