using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAttack : MonoBehaviour
{
    public Camera mainCamera;
    public float comboResetTime = 0.6f;

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

        // 🔹 Ganti combo index
        comboIndex = (comboIndex % 2) + 1;

        // 🔹 Kirim ke Animator
        animator.SetInteger("AttackDir", attackDir);
        animator.SetInteger("ComboIndex", comboIndex);
        animator.SetTrigger("IsAttacking");

        lastAttackTime = Time.time;
    }
}
