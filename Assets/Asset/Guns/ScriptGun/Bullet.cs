using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Peluru terbang sesuai arah moncong saat di-spawn
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // UPDATE: Filter agar peluru tidak bunuh diri/nabrak player/nabrak senjata
        if (collision.CompareTag("Player") || collision.CompareTag("Weapon") || collision.CompareTag("Bullet"))
        {
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth = collision.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1); // memberikan damage 1 peluru ke player
            }

            // Hancurkan peluru jika kena tanah atau musuh
            Destroy(gameObject);
            return;
        }
    }
}