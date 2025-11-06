using System.Collections;
using UnityEngine;
using Pathfinding;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack,
    Idle
}

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRadius = 1.5f;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;

    [Header("Attack Damage")]
    public float attackDamage = 20f;
    public Transform attackPoint;
    public float attackRange = 0.7f;
    public LayerMask playerLayerMask;

    [Header("Attack Offsets")]
    public Vector2 rightOffset = new Vector2(0.7f, 0f);
    public Vector2 leftOffset = new Vector2(-0.7f, 0f);
    public Vector2 upOffset = new Vector2(0f, 0.7f);
    public Vector2 downOffset = new Vector2(0f, -0.7f);

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;
    private Seeker seeker;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float repathRate = 0.5f;
    public float nextWaypointDistance = 0.5f;

    [Header("Detection")]
    public float detectionRadius = 5f;
    public LayerMask playerLayer;

    private Path path;
    private int currentWaypoint = 0;
    private int currentPatrolIndex = 0;
    private EnemyState currentState = EnemyState.Patrol;
    private float nextRepathTime = 0f;

    private bool isAttacking = false;
    private float lastFlipX = 1f;
    private float flipThreshold = 0.1f;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (patrolPoints.Length > 0)
            UpdatePath();
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = EnemyState.Chase;
                    UpdatePath();
                }
                break;

            case EnemyState.Chase:
                if (distanceToPlayer <= attackRadius)
                {
                    rb.linearVelocity = Vector2.zero;
                    currentState = EnemyState.Attack;
                    TryAttack();
                }
                else if (distanceToPlayer > detectionRadius * 1.2f)
                {
                    currentState = EnemyState.Patrol;
                    currentPatrolIndex = FindClosestPatrolPoint();
                    UpdatePath();
                }
                break;
        }
    }

    private void TryAttack()
    {
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        isAttacking = true;
        animator.SetBool("IsAttacking", true);

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = Vector2.zero;

        // arah serangan dan orientasi
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            spriteRenderer.flipX = dir.x < 0;
            attackPoint.localPosition = dir.x < 0 ? leftOffset : rightOffset;
            animator.SetInteger("AttackDir", 0);
        }
        else
        {
            if (dir.y > 0)
            {
                attackPoint.localPosition = upOffset;
                animator.SetInteger("AttackDir", 1);
            }
            else
            {
                attackPoint.localPosition = downOffset;
                animator.SetInteger("AttackDir", 2);
            }
        }
    }

    // 🩸 Dipanggil oleh Animation Event saat frame serangan
    public void DealDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayerMask);
        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage, transform.position);
        }
    }

    // 🕒 Dipanggil oleh Animation Event di akhir animasi serangan
    public void EndAttack()
    {
        animator.SetBool("IsAttacking", false);
        isAttacking = false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRadius)
            TryAttack();
        else if (distanceToPlayer <= detectionRadius)
        {
            currentState = EnemyState.Chase;
            UpdatePath();
        }
        else
        {
            currentState = EnemyState.Patrol;
            currentPatrolIndex = FindClosestPatrolPoint();
            UpdatePath();
        }
    }

    private void FixedUpdate()
    {
        if (currentState == EnemyState.Attack) return;

        if (Time.time >= nextRepathTime)
        {
            UpdatePath();
            nextRepathTime = Time.time + repathRate;
        }

        FollowPath();
    }

    private void UpdatePath()
    {
        if (seeker.IsDone())
        {
            Vector2 targetPos = currentState switch
            {
                EnemyState.Patrol => patrolPoints[currentPatrolIndex].position,
                EnemyState.Chase => player.position,
                _ => transform.position
            };

            seeker.StartPath(rb.position, targetPos, OnPathComplete);
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void FollowPath()
    {
        if (path == null || currentWaypoint >= path.vectorPath.Count)
        {
            if (currentState == EnemyState.Patrol)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                UpdatePath();
            }
            return;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
            currentWaypoint++;

        HandleFlip();

        if (currentState != EnemyState.Attack)
            animator.SetFloat("Speed", direction.magnitude);
        else
            animator.SetFloat("Speed", 0f);
    }

    private void HandleFlip()
    {
        float dx = (currentState == EnemyState.Chase && player != null)
            ? player.position.x - transform.position.x
            : (currentState == EnemyState.Patrol && patrolPoints.Length > 0
                ? patrolPoints[currentPatrolIndex].position.x - transform.position.x
                : 0);

        if (Mathf.Abs(dx) > flipThreshold)
        {
            if (dx < 0 && lastFlipX != -1f)
            {
                spriteRenderer.flipX = true;
                lastFlipX = -1f;
            }
            else if (dx > 0 && lastFlipX != 1f)
            {
                spriteRenderer.flipX = false;
                lastFlipX = 1f;
            }
        }
    }

    private int FindClosestPatrolPoint()
    {
        int closest = 0;
        float minDist = Mathf.Infinity;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, patrolPoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }
        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
