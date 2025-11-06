using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAttack : MonoBehaviour
{
    public bool isAttacking { get; private set; }
    public bool isInAttackDelay { get; private set; }

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.7f;
    public float attackDamage = 20f;
    public LayerMask enemyLayer;
    public float knockbackForce = 5f;

    // Offset posisi attackPoint tergantung arah
    public Vector2 rightOffset = new Vector2(0.5f, 0f);
    public Vector2 leftOffset = new Vector2(-0.5f, 0f);
    public Vector2 upOffset = new Vector2(0f, 0.5f);
    public Vector2 downOffset = new Vector2(0f, -0.5f);

    public Camera mainCamera;
    [Header("Timing")]
    public float comboResetTime = 0.6f;
    public float postAttackDelay = 0.25f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private int comboIndex = 0;
    private float lastAttackTime;
    private int attackDir; // 0 = kanan, 1 = bawah, 2 = atas

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleAttack();
        }
    }

    void HandleAttack()
    {
        // Reset combo jika terlalu lama
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboIndex = 0;
        }

        // Hitung arah relatif mouse
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - transform.position;

        // Tambahkan logika untuk flip arah horizontal
        if (dir.x < 0)
            spriteRenderer.flipX = true;   // menghadap kiri
        else if (dir.x > 0)
            spriteRenderer.flipX = false;  // menghadap kanan

        // Tentukan arah utama (untuk memilih animasi)
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            if (dir.y > 0) attackDir = 2;  // atas
            else attackDir = 1;            // bawah
        }
        else
        {
            attackDir = 0;                 // kanan (horizontal)
        }

        // Tentukan arah utama (untuk memilih animasi)
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            if (dir.y > 0)
            {
                attackDir = 2; // atas
                attackPoint.localPosition = upOffset;
            }
            else
            {
                attackDir = 1; // bawah
                attackPoint.localPosition = downOffset;
            }
        }
        else
        {
            attackDir = 0; // horizontal (kanan / kiri)
            if (dir.x < 0)
            {
                spriteRenderer.flipX = true;
                attackPoint.localPosition = leftOffset;
            }
            else
            {
                spriteRenderer.flipX = false;
                attackPoint.localPosition = rightOffset;
            }
        }

        // Ganti combo index
        comboIndex = (comboIndex % 2) + 1;

        // Kirim ke Animator
        animator.SetInteger("AttackDir", attackDir);
        animator.SetInteger("ComboIndex", comboIndex);
        animator.SetTrigger("IsAttacking");

        // --- Aktifkan state serangan ---
        isAttacking = true;
        isInAttackDelay = false;
        lastAttackTime = Time.time;

        // Reset otomatis setelah animasi + sedikit delay
        CancelInvoke(nameof(EndAttack));
        Invoke(nameof(EndAttack), comboResetTime);
    }

    private void EndAttack()
    {
        isAttacking = false;
        isInAttackDelay = true; // mulai masa delay

        // Setelah delay singkat, baru movement boleh flip lagi
        Invoke(nameof(EndAttackDelay), postAttackDelay);
    }

    private void EndAttackDelay()
    {
        isInAttackDelay = false;
    }

    // Fungsi dipanggil lewat Animation Event di frame serangan
    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        Debug.Log("Attack hit triggered!");

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage, transform.position, knockbackForce);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }



}
