using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("References")]
    public Image healthBarFill; // drag UI image fill bar di Inspector
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    [Header("Hit Feedback")]
    public float knockbackForce = 5f;
    public float flashDuration = 0.1f;
    public int flashCount = 3;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(float damage, Vector2 attackerPosition)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update health bar
        if (healthBarFill != null)
            healthBarFill.fillAmount = currentHealth / maxHealth;

        // Knockback
        Vector2 knockbackDir = (transform.position - (Vector3)attackerPosition).normalized;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        // Flash merah
        StartCoroutine(FlashRed());

        // Jika mati
        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashRed()
    {
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");
        // Tambahkan logika respawn / game over di sini
    }
}
