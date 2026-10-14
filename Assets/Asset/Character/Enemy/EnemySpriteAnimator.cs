using UnityEngine;

public class EnemySpriteAnimator : MonoBehaviour
{
    public enum AnimState { Idle, Run, Hit }

    [Header("Sprite Sequences")]
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private Sprite[] runSprites;
    [SerializeField] private Sprite[] hitSprites;

    [Header("Animation Settings")]
    [SerializeField] private float frameRate = 12f; // Frame per detik

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private AnimState currentState = AnimState.Idle;
    private float timer = 0f;
    private int currentFrame = 0;
    private bool isPlayingHit = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (spriteRenderer == null) return;

        // Tentukan state jika tidak sedang memainkan animasi Hit
        if (!isPlayingHit)
        {
            if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                SetState(AnimState.Run);
            }
            else
            {
                SetState(AnimState.Idle);
            }
        }

        UpdateFrame();
    }

    private void SetState(AnimState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        currentFrame = 0;
        timer = 0f;
    }

    private void UpdateFrame()
    {
        Sprite[] activeSprites = GetActiveSpriteArray();
        if (activeSprites == null || activeSprites.Length == 0) return;

        timer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(frameRate, 1f);

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame++;

            if (currentFrame >= activeSprites.Length)
            {
                if (isPlayingHit)
                {
                    // Selesai animasi Hit, kembali ke normal
                    isPlayingHit = false;
                    currentFrame = 0;
                    activeSprites = GetActiveSpriteArray();
                }
                else
                {
                    // Looping animasi Idle/Run
                    currentFrame = 0;
                }
            }

            if (activeSprites != null && currentFrame < activeSprites.Length)
            {
                spriteRenderer.sprite = activeSprites[currentFrame];
            }
        }
    }

    private Sprite[] GetActiveSpriteArray()
    {
        switch (currentState)
        {
            case AnimState.Run:
                return (runSprites != null && runSprites.Length > 0) ? runSprites : idleSprites;
            case AnimState.Hit:
                return (hitSprites != null && hitSprites.Length > 0) ? hitSprites : idleSprites;
            default:
                return idleSprites;
        }
    }

    public void PlayHitAnimation()
    {
        if (hitSprites != null && hitSprites.Length > 0)
        {
            isPlayingHit = true;
            SetState(AnimState.Hit);
        }
    }

    public void SetupSprites(Sprite[] idle, Sprite[] run, Sprite[] hit = null)
    {
        idleSprites = idle;
        runSprites = run;
        hitSprites = hit;
        currentFrame = 0;
        timer = 0f;
    }
}
