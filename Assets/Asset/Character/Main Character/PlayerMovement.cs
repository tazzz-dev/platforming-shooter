using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 24f; // Kecepatan saat dash
    [SerializeField] private float dashDuration = 0.2f; // Lama waktu dash berlangsung
    [SerializeField] private float dashCooldown = 1f; // Jeda sebelum bisa dash lagi

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private float dirX = 0f;
    private bool isGrounded;

    // Variabel untuk fitur Dash
    private bool isDashing;
    private float originalGravity;
    private float dashTimeLeft;
    private float lastDashTime = -100f;

    // Enum State Animasi
    private enum MovementState { idle, running, jumping, falling }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        
        // Simpan nilai gravitasi asli agar bisa dikembalikan setelah dash
        originalGravity = rb.gravityScale;
    }

    private void Update()
    {
        // 1. Logika Dash (Saat sedang Dash, abaikan input pergerakan lain)
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            
            if (dashTimeLeft <= 0f)
            {
                isDashing = false;
                rb.gravityScale = originalGravity; // Kembalikan gravitasi normal
            }
            return; // Berhenti di sini, jangan lanjutkan ke kode jalan/lompat
        }

        // 2. Cek tombol Dash (menggunakan tombol Shift Kiri)
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
            return;
        }

        // 3. Sistem Pergerakan Normal
        dirX = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) dirX = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) dirX = 1f;

        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // 4. Sistem Lompat (Spasi)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        UpdateAnimationState();
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time; // Catat waktu kapan terakhir kali dash
        
        // Tentukan arah dash (kiri/kanan) berdasarkan arah hadap sprite karakter saat ini
        float dashDirection = sprite.flipX ? -1f : 1f;
        
        // Lontarkan karakter ke depan dengan cepat dan hilangkan efek jatuh (gravitasi) sementara
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
        rb.gravityScale = 0f;
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