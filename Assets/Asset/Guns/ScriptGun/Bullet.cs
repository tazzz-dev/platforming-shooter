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
        // Beri kecepatan maju searah dengan rotasi peluru
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime); // Hancurkan peluru otomatis setelah beberapa detik
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cek jika menabrak tanah/musuh (abaikan jika menabrak Player)
        if (!collision.CompareTag("Player"))
        {
            // Nanti di sini ditambahkan fungsi kasih damage ke musuh
            Destroy(gameObject); // Hancurkan peluru saat kena objek
        }
    }
}