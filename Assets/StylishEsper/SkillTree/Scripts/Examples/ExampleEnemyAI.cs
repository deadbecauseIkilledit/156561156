//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.SkillTree.Examples.Skills;
using Esper.SkillTree.Stats;
using UnityEngine;

namespace Esper.SkillTree.Examples
{
    /// <summary>
    /// Example enemy AI for Skill Tree's Endless Smash demo.
    /// </summary>
    public class ExampleEnemyAI : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed = 1f;

        [SerializeField]
        private Vector2 flipPositionOffset;

        [SerializeField]
        private Transform slashStartTransform;

        private Rigidbody2D rb;

        private Collider2D objectCollider;

        private Animator animator;

        private UnitStatsProvider unitStats;

        private SkillNode slash;

        private float attackTimePassed;

        private bool isAttacking;

        private bool isDead;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            objectCollider = GetComponent<Collider2D>();
            unitStats = GetComponent<UnitStatsProvider>();

            slash = SkillTree.GetSkillGraph("Enemy Attacks").GetNodeByKey("Slash");
        }

        private void Start()
        {
            // Stat callback to handle death when HP is 0
            unitStats.onStatReachedMin.AddListener(HandleHPReachedZero);
        }

        private void FixedUpdate()
        {
            if (ExampleGameManager.instance.isPaused || isDead)
            {
                return;
            }

            Attack();

            if (!isAttacking)
            {
                Move();
            }
        }

        /// <summary>
        /// Handles enemy death.
        /// </summary>
        private void Death()
        {
            if (isDead)
            {
                return;
            }

            // Visually display death
            isDead = true;
            animator.Play("Death");
            rb.simulated = false;
            objectCollider.enabled = false;

            // From from spawned enemy list
            ExampleGameManager.instance.RemoveEnemy(this);

            // Remove from field after a delay
            Destroy(gameObject, 4f);

            // Grant player exp
            ExampleGameManager.instance.playerController.GrantExp(50);
        }

        /// <summary>
        /// Calls Death() when HP is zero.
        /// </summary>
        /// <param name="stat">The upgradable stat.</param>
        private void HandleHPReachedZero(Stat stat)
        {
            if (stat.NameMatches("HP"))
            {
                Death();
            }
        }

        /// <summary>
        /// Sets the simulated value of the enemy's rigidbody.
        /// </summary>
        /// <param name="simulated">The simulated value.</param>
        public void SetRigidbodySimulated(bool simulated)
        {
            rb.simulated = simulated;
        }

        /// <summary>
        /// Moves the character towards the player.
        /// </summary>
        private void Move()
        {
            var x = ExampleGameManager.instance.playerController.transform.position.x > transform.position.x ? 1 : -1;
            var move = new Vector2(x, 0);
            bool isFlipped = move.x < 0;

            if (move.x != 0)
            {
                Flip(isFlipped);
            }
            else
            {
                isFlipped = transform.localScale.x == -1;
            }

            move.y = rb.linearVelocity.y;
            move.x *= moveSpeed;
            rb.linearVelocity = move;
        }

        /// <summary>
        /// Checks if this enemy is in attack range of the player.
        /// </summary>
        /// <returns>True if in attack range. Otherwise, false.</returns>
        private bool InRange()
        {
            var distance = Vector2.Distance(ExampleGameManager.instance.playerController.transform.position, transform.position);
            return distance <= slash.range;
        }

        /// <summary>
        /// Starts the attack if in range.
        /// </summary>
        private void Attack()
        {
            if (attackTimePassed < slash.cooldown)
            {
                attackTimePassed += Time.fixedDeltaTime;
                return;
            }

            if (!isAttacking && InRange())
            {
                animator.Play("Attack");
                Invoke(nameof(LaunchAttack), 0.3f);
                isAttacking = true;
                attackTimePassed = 0;
            }
        }

        /// <summary>
        /// Launches the slash attack.
        /// </summary>
        private void LaunchAttack()
        {
            var usedSkill = SkillTree.TryUseSkill(slash, unitStats);

            if (usedSkill == null)
            {
                return;
            }

            usedSkill.OnWindupComplete(() =>
            {
                ExampleSkill.InstantiateSkill<SlashSkill>(usedSkill, slashStartTransform.position);
            });

            Invoke(nameof(EndAttack), 0.2f);
        }

        /// <summary>
        /// Ends the attack.
        /// </summary>
        private void EndAttack()
        {
            isAttacking = false;
            SkillTree.CompleteSkillUse(slash, unitStats);
        }

        /// <summary>
        /// Flips the character.
        /// </summary>
        /// <param name="flip">If the character should be flipped.</param>
        private void Flip(bool flip)
        {
            if (flip && transform.localScale.x != -1)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                transform.position += (Vector3)flipPositionOffset;
            }
            else if (!flip && transform.localScale.x != 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
                transform.position -= (Vector3)flipPositionOffset;
            }
        }
    }
}