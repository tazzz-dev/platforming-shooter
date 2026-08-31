using UnityEngine;
using UnityEngine.InputSystem; // Tambahkan ini di baris paling atas

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private float dirX = 0f;
    private bool isGrounded;

    // Enum State Animasi
    private enum MovementState { idle, running, jumping, falling }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Sistem Baru: Membaca tombol A/D atau Arrow Keys secara presisi (-1, 0, 1)
        dirX = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) dirX = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) dirX = 1f;

        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Sistem Baru: Tombol Space untuk Lompat
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        // Balik arah sprite (Flip Horizontal)
        if (dirX > 0f)
        {
            sprite.flipX = false;
        }
        else if (dirX < 0f)
        {
            sprite.flipX = true;
        }

        // Penentuan State Animasi
        if (isGrounded)
        {
            if (dirX != 0f)
                state = MovementState.running;
            else
                state = MovementState.idle;
        }
        else
        {
            if (rb.linearVelocity.y > 0.1f)
                state = MovementState.jumping;
            else
                state = MovementState.falling;
        }

        if (anim != null)
        {
            anim.SetInteger("state", (int)state);
        }
    }

    // Menampilkan visual lingkaran GroundCheck di Scene tab (Warna Merah)
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}