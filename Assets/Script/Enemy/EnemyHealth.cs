using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public GameObject healthBarPrefab;
    public Canvas mainCanvas; // drag canvas UI utama ke sini
    public Vector3 healthBarOffset = new Vector3(0, 1f, 0);
    private EnemyHealthBar healthBarInstance;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isHit = false;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Spawn health bar ke dalam Canvas UI
        if (healthBarPrefab != null && mainCanvas != null)
        {
            GameObject bar = Instantiate(healthBarPrefab, mainCanvas.transform);
            healthBarInstance = bar.GetComponent<EnemyHealthBar>();

            healthBarInstance.Initialize(transform);
            healthBarInstance.offset = healthBarOffset;
            healthBarInstance.SetHealth(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition, float knockbackForce)
    {
        if (isHit) return;
        isHit = true;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        StartCoroutine(FlashRed());

        Vector2 knockDir = (transform.position - attackerPosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);

        if (healthBarInstance != null)
            healthBarInstance.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }

        Invoke(nameof(ResetHit), 0.3f);
    }

    private void ResetHit() => isHit = false;

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    private void Die()
    {
        if (healthBarInstance != null)
            Destroy(healthBarInstance.gameObject);

        Destroy(gameObject);
    }
}
