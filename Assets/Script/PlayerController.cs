using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;            // speed in units/second
    public float acceleration = 10f;        // optional smoothing
    public bool useSmoothing = true;

    [Header("Animator")]
    public Animator animator;               // assign via inspector (optional)
    public string paramSpeed = "Speed";     // float parameter name
    public string paramMoveX = "MoveX";     // float parameter name
    public string paramMoveY = "MoveY";     // float parameter name

    Rigidbody2D rb;
    Vector2 targetVelocity;
    Vector2 currentVelocity; // for smoothing

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Read input (WASD / Arrow keys)
        float hx = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        float hy = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(hx, hy);

        // normalize so diagonal isn't faster
        if (input.sqrMagnitude > 1f) input = input.normalized;

        targetVelocity = input * moveSpeed;

        // Animator parameters (if ada)
        if (animator != null)
        {
           // animator.SetFloat(paramMoveX, input.x);
           // animator.SetFloat(paramMoveY, input.y);
            animator.SetFloat(paramSpeed, input.magnitude);
        }

        if (targetVelocity.x > 0.01f)
        {
            // Menghadap kanan
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (targetVelocity.x < -0.01f)
        {
            // Menghadap kiri
            GetComponent<SpriteRenderer>().flipX = true;
        }

    }

    void FixedUpdate()
    {
        if (useSmoothing)
        {
            // SmoothDamp for smoother acceleration/deceleration
            Vector2 newVel = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, Mathf.Max(0.01f, 1f / acceleration), Mathf.Infinity, Time.fixedDeltaTime);
            rb.linearVelocity = newVel;
        }
        else
        {
            rb.linearVelocity = targetVelocity;
        }
    }
}
