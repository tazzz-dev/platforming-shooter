using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 20f;
    [Tooltip("Jarak maksimal peluru sebelum menghilang otomatis")]
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody2D rb;
    private Vector3 startPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPosition = transform.position;

        // Peluru terbang sesuai arah moncong saat di-spawn
        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }

        // Batas waktu cadangan agar tidak ada peluru yang melayang selamanya
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Hancurkan peluru jika sudah melampaui jarak maksimal dari titik tembak
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Fungsi untuk mengatur jarak jangkauan peluru dari senjata
    /// </summary>
    public void SetRange(float range)
    {
        maxDistance = range;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Abaikan jika menabrak Player, Senjata, atau sesama Peluru
        if (collision.CompareTag("Player") || collision.CompareTag("Weapon") || collision.CompareTag("Bullet"))
        {
            return;
        }

        // 2. Jika mengenai Musuh
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth = collision.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1);
            }

            Destroy(gameObject);
            return;
        }

        // 3. Jika menabrak objek padat (Tembok / Lantai / Tilemap)
        if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}