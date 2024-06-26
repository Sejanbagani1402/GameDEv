using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // Reference to the Animator component
    private Animator animator;

    // Movement variables
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool facingRight = true; // To keep track of character facing direction

    // AI variables
    public Transform player; // Reference to the player
    public float attackRange = 2.2f; // Range within which the enemy will attack
    public float attackCooldown = 1f; // Time between attacks
    private float lastAttackTime;
    public Health playerHealth; // Reference to player's Health component

    // Parameters for animator
    private int isRunningHash;
    private int groundedHash;
    private int attackHash;
    private int jumpHash;
    private int hurtHash;
    private int deathHash;

    void Start()
    {
        Debug.Log("EnemyAI Start: Initializing components and parameters.");
        // Get the Animator component
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Initialize parameter hashes
        isRunningHash = Animator.StringToHash("isRunning");
        groundedHash = Animator.StringToHash("Grounded");
        attackHash = Animator.StringToHash("Attack");
        jumpHash = Animator.StringToHash("Jump");
        hurtHash = Animator.StringToHash("Hurt");
        deathHash = Animator.StringToHash("Death");

        // Freeze rotation to prevent character from rotating when jumping
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Get the player's Health component
        playerHealth = player.GetComponent<Health>();

        if (playerHealth == null)
        {
            Debug.LogError("Player Health component not found.");
        }
    }

    void Update()
    {
        Debug.Log("EnemyAI Update: Handling movement and attack.");
        HandleMovement();
        HandleAttack();
    }

    void HandleMovement()
    {
        if (playerHealth.CurrentHealth <= 0)
        {
            Debug.Log("Player is dead. Stopping enemy movement.");
            // Stop movement if player is dead
            rb.velocity = Vector2.zero;
            animator.SetBool(isRunningHash, false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log("Distance to player: " + distanceToPlayer);

        if (distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 targetPosition = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;
            Debug.Log("Moving to position: " + targetPosition);

            rb.MovePosition(targetPosition);

            // Update animator
            animator.SetBool(isRunningHash, true);

            // Flip character direction based on movement direction
            if (direction.x > 0 && !facingRight)
            {
                Debug.Log("Flipping to face right.");
                Flip();
            }
            else if (direction.x < 0 && facingRight)
            {
                Debug.Log("Flipping to face left.");
                Flip();
            }
        }
        else
        {
            Debug.Log("Player within attack range. Stopping movement.");
            // Stop moving
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool(isRunningHash, false);
        }
    }

    void HandleAttack()
    {
        if (playerHealth.CurrentHealth <= 0)
        {
            Debug.Log("Player is dead. Stopping attacks.");
            // Stop attacking if player is dead
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log("Checking attack range. Distance: " + distanceToPlayer + ", Attack range: " + attackRange);

        if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            Debug.Log("Attacking player. Time: " + Time.time);
            // Attack animation
            animator.SetTrigger(attackHash);
            lastAttackTime = Time.time;

            // Deal damage to the player
            if (playerHealth != null)
            {
                Debug.Log("Dealing damage to the player.");
                playerHealth.TakeDamage(10); // Adjust the damage amount as needed
            }
            else
            {
                Debug.LogError("Player Health component is null.");
            }
        }
    }

    void Flip()
    {
        Debug.Log("Flipping character direction.");
        // Switch the way the enemy is facing.
        facingRight = !facingRight;

        // Multiply the enemy's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            Debug.Log("Collided with ground. Setting isGrounded to true.");
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            Debug.Log("Exited collision with ground. Setting isGrounded to false.");
            isGrounded = false;
        }
    }
}
