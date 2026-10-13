using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.5f; // Kecepatan jalan santai (tanpa Shift)
    [SerializeField] private float runSpeed = 7f;    // Kecepatan lari kencang (tahan Shift)
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 11f; // Kekuatan lompatan kedua (Double Jump)
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 24f; // Kecepatan saat dash
    [SerializeField] private float dashDuration = 0.2f; // Lama waktu dash berlangsung
    [SerializeField] private float dashCooldown = 1f; // Jeda sebelum bisa dash lagi

    [Header("Shooting Animation Settings")]
    [SerializeField] private float shootAnimDuration = 0.35f; // Berapa lama animasi tembak aktif setelah klik (dinaikkan agar pose tembak terlihat jelas)

    [Header("Animation Settings")]
    [SerializeField] private bool useWalkAnimation = true; // Aktifkan jika menggunakan state animasi Walking terpisah (state = 5)

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private Camera mainCam;
    private float dirX = 0f;
    private bool isGrounded;
    private bool isRunning;

    // Variabel untuk fitur Double Jump
    private bool canDoubleJump;
    private bool isDoubleJumping;

    // Variabel untuk fitur Dash
    private bool isDashing;
    private float originalGravity;
    private float dashTimeLeft;
    private float lastDashTime = -100f;

    // Variabel untuk fitur Menembak
    private float shootTimer = 0f;
    private bool isShooting;

    // Variabel untuk Drop-Through Platform
    private Collider2D playerCollider;

    // Enum State Animasi
    // 0: idle, 1: running, 2: jumping, 3: falling, 4: doubleJump, 5: walking
    private enum MovementState { idle, running, jumping, falling, doubleJump, walking }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        mainCam = Camera.main;
        
        // Simpan nilai gravitasi asli agar bisa dikembalikan setelah dash
        originalGravity = rb.gravityScale;
    }

    private void Update()
    {
        // Abaikan input jika game sedang di-pause
        if (Time.timeScale == 0f) return;

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

        // 2. Cek tombol Dash (menggunakan tombol Ctrl Kiri atau Kanan)
        bool isCtrlPressed = (Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.rightCtrlKey.wasPressedThisFrame);
        if (isCtrlPressed && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
            return;
        }

        // 3. Deteksi Input Tembak (Klik Kiri Mouse)
        HandleShootingInput();

        // 4. Sistem Pergerakan Normal (Jalan santai vs Lari tahan Shift)
        dirX = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) dirX = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) dirX = 1f;

        // Cek tombol Shift: Karakter hanya berlari jika tombol Shift ditahan
        bool isHoldingShift = (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        isRunning = isHoldingShift && (dirX != 0f);

        // Tentukan kecepatan gerak berdasarkan apakah sedang lari atau jalan
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(dirX * currentSpeed, rb.linearVelocity.y);

        // Pengecekan Tanah (Grounded)
        if (groundCheck != null)
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // Jika baru mendarat di tanah, reset kemampuan lompat kedua
            if (isGrounded)
            {
                canDoubleJump = true;
                isDoubleJumping = false;
            }
        }

        // 5. Sistem Lompat & Double Jump (Spasi)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                // Lompatan pertama dari tanah
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                // Lompatan kedua di udara (Double Jump)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
                canDoubleJump = false;
                isDoubleJumping = true;
            }
        }

        // 6. Drop-Through Platform: Tekan S saat berdiri di atas DropThroughPlatform
        if (isGrounded && (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame))
        {
            TryDropThroughPlatform();
        }

        UpdateAnimationState();
    }

    private void HandleShootingInput()
    {
        // Cek apakah klik kiri mouse ditekan
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            shootTimer = shootAnimDuration;
        }

        if (shootTimer > 0f)
        {
            shootTimer -= Time.deltaTime;
            isShooting = true;
        }
        else
        {
            isShooting = false;
        }
    }

    /// <summary>
    /// Cek apakah di bawah player ada DropThroughPlatform.
    /// Jika ya, panggil DropThrough agar player jatuh menembus platform.
    /// </summary>
    private void TryDropThroughPlatform()
    {
        // Raycast ke bawah dari posisi GroundCheck untuk mendeteksi DropThroughPlatform
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            groundCheck != null ? groundCheck.position : transform.position,
            Vector2.down,
            groundCheckRadius + 0.3f,
            groundLayer
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            DropThroughPlatform dropPlatform = hit.collider.GetComponent<DropThroughPlatform>();
            if (dropPlatform != null && playerCollider != null)
            {
                dropPlatform.DropThrough(playerCollider);
                Debug.Log("[PlayerMovement] Drop melalui platform!");
                return;
            }
        }
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

    private bool HasParameter(string paramName)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        // Balik arah sprite (Flip Horizontal)
        // Jika sedang menembak, hadapkan karakter ke arah kursor mouse
        if (isShooting && mainCam != null && Mouse.current != null)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            if (mousePos.x > transform.position.x)
                sprite.flipX = false;
            else if (mousePos.x < transform.position.x)
                sprite.flipX = true;
        }
        else
        {
            // Jika tidak menembak, hadap sesuai arah jalan
            if (dirX > 0f)
                sprite.flipX = false;
            else if (dirX < 0f)
                sprite.flipX = true;
        }

        // Penentuan State Animasi
        if (isGrounded)
        {
            if (dirX != 0f)
            {
                if (isRunning)
                {
                    state = MovementState.running; // state = 1 (Lari saat tahan Shift)
                }
                else
                {
                    state = useWalkAnimation ? MovementState.walking : MovementState.running; // state = 5 (Jalan) atau fallback 1
                }
            }
            else
            {
                state = MovementState.idle; // state = 0
            }
        }
        else
        {
            // Saat di udara
            if (isDoubleJumping)
            {
                state = MovementState.doubleJump;
                // Jika mulai jatuh ke bawah, otomatis ganti ke falling
                if (rb.linearVelocity.y <= 0f)
                {
                    isDoubleJumping = false;
                    state = MovementState.falling;
                }
            }
            else if (rb.linearVelocity.y > 0.1f)
            {
                state = MovementState.jumping;
            }
            else
            {
                state = MovementState.falling;
            }
        }

        // Kirim status ke Animator
        if (anim != null)
        {
            anim.SetInteger("state", (int)state);

            // Hanya set isShooting jika parameternya sudah dibuat di Animator agar tidak muncul error
            if (HasParameter("isShooting"))
            {
                anim.SetBool("isShooting", isShooting);
            }
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