using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public bool useSmoothing = true;

    [Header("Animator")]
    public Animator animator;
    public string paramSpeed = "Speed";
    public string paramMoveX = "MoveX";
    public string paramMoveY = "MoveY";

    private Rigidbody2D rb;
    private Vector2 targetVelocity;
    private Vector2 currentVelocity;

    private SpriteRenderer spriteRenderer;
    private PlayerAttack playerAttack; //  reference ke script attack

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAttack = GetComponent<PlayerAttack>(); //  ambil komponen
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        float hx = Input.GetAxisRaw("Horizontal");
        float hy = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(hx, hy);
        if (input.sqrMagnitude > 1f) input = input.normalized;

        targetVelocity = input * moveSpeed;

        if (animator != null)
            animator.SetFloat(paramSpeed, input.magnitude);

        // hanya ubah arah jika TIDAK menyerang
        if (!playerAttack.isAttacking)
        {
            if (targetVelocity.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (targetVelocity.x < -0.01f)
                spriteRenderer.flipX = true;
        }
    }

    void FixedUpdate()
    {
        if (useSmoothing)
        {
            Vector2 newVel = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity,
                Mathf.Max(0.01f, 1f / acceleration), Mathf.Infinity, Time.fixedDeltaTime);
            rb.linearVelocity = newVel;
        }
        else
        {
            rb.linearVelocity = targetVelocity;
        }
    }
}
