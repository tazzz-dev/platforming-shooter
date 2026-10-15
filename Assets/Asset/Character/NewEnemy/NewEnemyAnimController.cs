using UnityEngine;

/// <summary>
/// Jembatan antara EnemyAI.cs dan Unity Animator component.
/// Pasang script ini ke prefab enemy bersama komponen Animator.
/// 
/// Parameter Animator yang dipakai:
///   - Speed (Float)       : kecepatan horisontal rb, untuk Idle <-> Walk/Run
///   - IsGrounded (Bool)   : apakah enemy di tanah
///   - TriggerAttack (Trigger) : khusus Enemy1 (Punch)
///   - TriggerShoot  (Trigger) : khusus Enemy2 & Enemy3 (Shoot)
///   - TriggerGetHit (Trigger) : kena serangan
///   - TriggerDeath  (Trigger) : mati
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class NewEnemyAnimController : MonoBehaviour
{
    // Hash parameter agar lebih cepat dari string lookup
    private static readonly int PARAM_SPEED   = Animator.StringToHash("Speed");
    private static readonly int PARAM_ATTACK  = Animator.StringToHash("TriggerAttack");
    private static readonly int PARAM_SHOOT   = Animator.StringToHash("TriggerShoot");
    private static readonly int PARAM_GETHIT  = Animator.StringToHash("TriggerGetHit");
    private static readonly int PARAM_DEATH   = Animator.StringToHash("TriggerDeath");

    private Animator    animator;
    private Rigidbody2D rb;

    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb       = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead) return;

        // Update speed parameter (pakai absolut kecepatan X)
        float speedX = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat(PARAM_SPEED, speedX);
    }

    // ─── Public API (dipanggil dari EnemyAI atau EnemyHealth) ───────────────

    /// <summary>Trigger animasi punch/melee (Enemy1)</summary>
    public void PlayAttack()
    {
        if (isDead) return;
        animator.SetTrigger(PARAM_ATTACK);
    }

    /// <summary>Trigger animasi tembak (Enemy2 & Enemy3)</summary>
    public void PlayShoot()
    {
        if (isDead) return;
        animator.SetTrigger(PARAM_SHOOT);
    }

    /// <summary>Trigger animasi kena serangan</summary>
    public void PlayGetHit()
    {
        if (isDead) return;
        animator.SetTrigger(PARAM_GETHIT);
    }

    /// <summary>Trigger animasi mati — tidak bisa balik lagi</summary>
    public void PlayDeath()
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger(PARAM_DEATH);
    }

    /// <summary>Reset state (saat enemy respawn/direcycle)</summary>
    public void ResetAnimState()
    {
        isDead = false;
        animator.ResetTrigger(PARAM_ATTACK);
        animator.ResetTrigger(PARAM_SHOOT);
        animator.ResetTrigger(PARAM_GETHIT);
        animator.ResetTrigger(PARAM_DEATH);
    }
}
