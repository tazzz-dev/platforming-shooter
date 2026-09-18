using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Terbang lurus sesuai rotasi spawn peluru
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Abaikan jika menabrak sesama Musuh, Senjata Musuh, atau Peluru Musuh
        if (collision.CompareTag("Enemy") || collision.CompareTag("Weapon") || collision.CompareTag("EnemyBullet"))
        {
            return;
        }

        // 2. Jika menabrak Player, berikan damage ke script PlayerHealth
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject); // Hancurkan peluru setelah kena Player
            return;
        }

        // 3. Jika menabrak objek lain (dinding/tanah), peluru langsung hancur
        Destroy(gameObject);
    }
}