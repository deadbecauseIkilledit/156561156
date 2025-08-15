//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: September 2024
// Description: Character controller for the Endless Smash demo.
//***************************************************************************************

using Esper.SkillTree.Examples.Skills;
using Esper.SkillTree.Stats;
using Esper.SkillTree.UI;
using UnityEngine;

namespace Esper.SkillTree.Examples
{
    public class ExamplePlatformerCharacterController : MonoBehaviour
    {
        private ExampleInputHandler input;

        private Rigidbody2D rb;

        private Animator animator;

        private SkillNode bolt;

        private UnitStatsProvider unitStats;

        private SpriteRenderer spriteRenderer;

        [Header("Movement")]
        [SerializeField]
        private float moveSpeed = 1f;

        [SerializeField]
        private float runMultiplier = 2f;

        [SerializeField]
        private float speedAnimationBlend = 0.25f;

        [SerializeField]
        private float speedChangeRate = 1f;

        [SerializeField]
        private Vector2 flipPositionOffset;

        [SerializeField]
        private AudioClip[] footstepSounds;

        [Header("Jump")]
        [SerializeField]
        private float jumpStrength = 1f;

        [SerializeField]
        private float jumpDelay = 0.15f;

        [SerializeField]
        private AudioClip jumpSound;

        [Header("Grounded")]
        [SerializeField]
        private LayerMask groundLayers;

        [SerializeField]
        private Vector2 groundedOffset;

        private Vector2 originalGroundedOffset;

        [SerializeField]
        private float groundedRadius;

        [SerializeField]
        private bool grounded;

        [Header("Skills")]
        [SerializeField]
        private Transform boltStartTransform;

        [SerializeField]
        private Transform particleFollowTransform;

        public bool invincible;

        [Header("Level Up")]
        [SerializeField]
        private ParticleSystem levelUpEffect;

        [SerializeField]
        private AudioClip levelUpSound;

        private bool isLaunchingJump;

        private bool attackAnimRunning;

        private Vector3 spawnPoint;

        private UsedSkill currentSkill;

        [HideInInspector]
        public bool isDead;

        private void Awake()
        {
            input = GetComponent<ExampleInputHandler>();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            unitStats = GetComponent<UnitStatsProvider>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            bolt = SkillTree.GetSkillGraph("Player Basics").GetNodeByKey("Bolt");
            originalGroundedOffset = groundedOffset;
        }

        private void Start()
        {
            spawnPoint = transform.position;

            // Stat callback to handle death when HP is 0
            unitStats.onStatReachedMin.AddListener(HandleHPReachedZero);

            // Stat callback to handle level up
            unitStats.onStatReachedMax.AddListener(HandleEXPReachedMax);

            SkillTree.playerSkillPoints = 100;
        }

        private void FixedUpdate()
        {
            if (ExampleGameManager.instance.isPaused)
            {
                input.attack = false;
                input.jump = false;
            }

            if (isDead)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                return;
            }

            BasicAttack();
            GroundedCheck();

            if (!attackAnimRunning)
            {
                Jump();
                Move();
            }
        }

        /// <summary>
        /// Plays a random footstep sound.
        /// </summary>
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (footstepSounds.Length > 0)
                {
                    var index = Random.Range(0, footstepSounds.Length);
                    AudioSource.PlayClipAtPoint(footstepSounds[index], new Vector3(transform.position.x, transform.position.y, -9f));
                }
            }
        }

        /// <summary>
        /// Grants specified amount of EXP to the player.
        /// </summary>
        /// <param name="exp">The amount of EXP.</param>
        public void GrantExp(float exp)
        {
            // Increase exp and store and excess exp
            StatValue excessExp = unitStats.IncreaseStat("EXP", exp);

            if (excessExp > 0)
            {
                GrantExp(excessExp.ToFloat());
            }
        }

        /// <summary>
        /// Sets the simulated value of the player's rigidbody.
        /// </summary>
        /// <param name="simulated">The simulated value.</param>
        public void SetRigidbodySimulated(bool simulated)
        {
            rb.simulated = simulated;
        }

        /// <summary>
        /// Handles player death.
        /// </summary>
        private void Death()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            animator.Play("Death");

            // Don't allow skills to be used
            DefaultSkillBar.instance.canUseSkill = false;

            Invoke(nameof(Respawn), 4f);
        }

        /// <summary>
        /// Respawns the player.
        /// </summary>
        private void Respawn()
        {
            unitStats.GetStat("HP").ResetValues();
            transform.position = spawnPoint;
            isDead = false;
            animator.Play("Idle Walk Run Blend");
            ExampleGameManager.instance.statusUI.UpdateUI();
            DefaultSkillBar.instance.canUseSkill = true;
        }


        /// <summary>
        /// Levels up the player.
        /// </summary>
        /// <param name="stat">The upgradable stat.</param>
        private void HandleEXPReachedMax(Stat stat)
        {
            if (stat.Identity.abbreviation == "EXP")
            {
                // Play effect & sound
                levelUpEffect.Play();
                AudioSource.PlayClipAtPoint(levelUpSound, transform.position);

                // Level up
                unitStats.LevelUp();
                stat.currentValue.SetValue(0);

                var dmg = SkillTree.playerStats.GetStat("DMG");
                dmg.currentValue = dmg.BaseValue;

                // Let Skill Tree know the new player level
                SkillTree.SetPlayerLevel(unitStats.level);

                // Gain a skill point
                SkillTree.playerSkillPoints++;
            }
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
        /// Handles basic attack.
        /// </summary>
        private void BasicAttack()
        {
            if (input.attack && !attackAnimRunning)
            {
                SkillTree.TryUseSkill(bolt, unitStats);
            }
            else if (attackAnimRunning)
            {
                animator.SetFloat("VelocityY", rb.linearVelocity.y);
                animator.SetFloat("Speed", 0);
            }

            input.attack = false;
        }

        /// <summary>
        /// Prepares a projectile skill.
        /// </summary>
        public void ReadyProjectileSkill(UsedSkill usedSkill)
        {
            currentSkill = usedSkill;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x / 4f, rb.linearVelocity.y);
            animator.Play("BasicAttack");

            attackAnimRunning = true;
            Invoke(nameof(EndAttackAnimation), 1.167f);
        }

        /// <summary>
        /// Prepares Darkness Falls skill.
        /// </summary>
        public void ReadyDarknessFallsSkill(UsedSkill usedSkill)
        {
            invincible = true;
            currentSkill = usedSkill;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.Play("DarknessFalls");

            attackAnimRunning = true;
            Invoke(nameof(EndAttackAnimation), 1.167f);

            Invoke(nameof(PlayJumpSound), 0.25f);
        }

        private void PlayJumpSound()
        {
            AudioSource.PlayClipAtPoint(jumpSound, transform.position);
        }

        /// <summary>
        /// Fires the Bolt skill.
        /// </summary>
        /// <param name="usedSkill">The used skill reference.</param>
        public void FireBoltSkill(UsedSkill usedSkill)
        {
            var skill = ExampleSkill.InstantiateSkill<BoltSkill>(usedSkill, boltStartTransform.position);
            skill.SetDirection(transform.localScale.x == -1 ? Vector2.left : Vector2.right);
        }

        /// <summary>
        /// Fires the Death Bolt skill.
        /// </summary>
        /// <param name="usedSkill">The used skill reference.</param>
        public void FireDeathBoltSkill(UsedSkill usedSkill)
        {
            var skill = ExampleSkill.InstantiateSkill<DeathBoltSkill>(usedSkill, boltStartTransform.position);
            skill.SetDirection(transform.localScale.x == -1 ? Vector2.left : Vector2.right);
        }

        /// <summary>
        /// Uses the Dark Born skill.
        /// </summary>
        /// <param name="usedSkill">The used skill reference.</param>
        public void UseShadowWalkerSkill(UsedSkill usedSkill)
        {
            currentSkill = usedSkill;
            var skill = ExampleSkill.InstantiateSkill<ShadowWalker>(usedSkill, particleFollowTransform.position);
            skill.ApplySkill(spriteRenderer, particleFollowTransform);
            CompleteSkillUse();
        }

        /// <summary>
        /// Fires The Black Death skill.
        /// </summary>
        /// <param name="usedSkill">The used skill reference.</param>
        public void FireTheBlackDeathSkill(UsedSkill usedSkill)
        {
            var dir = transform.localScale.x == -1 ? Vector2.left : Vector2.right;
            var skill = ExampleSkill.InstantiateSkill<TheBlackDeath>(usedSkill, boltStartTransform.position + (Vector3)dir + new Vector3(0, 0.5f));
            skill.SetDirection(transform.localScale.x == -1 ? Vector2.left : Vector2.right);
        }

        /// <summary>
        /// Uses the Shadow Walker skill.
        /// </summary>
        /// <param name="usedSkill">The used skill reference.</param>
        public void UseDarkBornSkill(UsedSkill usedSkill)
        {
            currentSkill = usedSkill;
            var skill = ExampleSkill.InstantiateSkill<DarkBorn>(usedSkill, particleFollowTransform.position);
            skill.ApplySkill(particleFollowTransform);
            CompleteSkillUse();
        }

        /// <summary>
        /// Uses the Darkness Falls skill.
        /// </summary>
        /// <param name="usedSkill">The used skill reference.</param>
        public void UseDarknessFallsSkill(UsedSkill usedSkill)
        {
            invincible = false;
            ExampleSkill.InstantiateSkill<DarknessFalls>(usedSkill, boltStartTransform.position);
            CompleteSkillUse();
        }

        /// <summary>
        /// Ends the bolt attack animation.
        /// </summary>
        private void EndAttackAnimation()
        {
            if (input.move.x != 0)
            {
                speedAnimationBlend /= 2;
            }
            else
            {
                speedAnimationBlend = 0;
            }

            input.attack = false;
            input.jump = false;
            attackAnimRunning = false;

            CompleteSkillUse();
        }

        /// <summary>
        /// Tells Skill Tree that the current skill's windup or animation is complete.
        /// </summary>
        private void CompleteSkillUse()
        {
            SkillTree.CompleteSkillUse(currentSkill.skill, unitStats);
        }

        /// <summary>
        /// Moves the character.
        /// </summary>
        private void Move()
        {
            var move = input.move;
            bool isFlipped = move.x < 0;

            if (move.x != 0)
            {
                Flip(isFlipped);
            }
            else
            {
                isFlipped = transform.localScale.x == -1;
            }

            float targetSpeed = input.run ? moveSpeed * runMultiplier : moveSpeed;

            if (move == Vector2.zero)
            {
                targetSpeed = 0f;
            }

            speedAnimationBlend = Mathf.Lerp(speedAnimationBlend, targetSpeed, Time.fixedDeltaTime * speedChangeRate);

            if (speedAnimationBlend < 0.01f)
            {
                speedAnimationBlend = 0f;
            }

            var speed = speedAnimationBlend * (isFlipped ? speedAnimationBlend * -1 : speedAnimationBlend);

            move.y = rb.linearVelocity.y;
            move.x = speed;
            rb.linearVelocity = move;

            animator.SetFloat("VelocityY", move.y);
            animator.SetFloat("Speed", Mathf.Abs(speed));
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
                groundedOffset = new Vector2(originalGroundedOffset.x * -1, originalGroundedOffset.y);
                transform.position += (Vector3)flipPositionOffset;
            }
            else if (!flip && transform.localScale.x != 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
                groundedOffset = originalGroundedOffset;
                transform.position -= (Vector3)flipPositionOffset;
            }
        }

        /// <summary>
        /// Handles jumping.
        /// </summary>
        private void Jump()
        {
            if (input.jump && !isLaunchingJump && grounded)
            {
                Invoke(nameof(JumpLaunch), jumpDelay);
                animator.Play("JumpLaunch");
                isLaunchingJump = true;
            }
            
            input.jump = false;
        }

        /// <summary>
        /// Starts the jump.
        /// </summary>
        private void JumpLaunch()
        {
            AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            rb.AddForce(new Vector2(0, jumpStrength));
            Invoke(nameof(EndJumpLaunch), 0.25f);
        }

        private void EndJumpLaunch()
        {
            isLaunchingJump = false;
        }

        /// <summary>
        /// Checks if the character is grounded.
        /// </summary>
        private void GroundedCheck()
        {
            Vector3 circlePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z) - (Vector3)groundedOffset;
            grounded = Physics2D.OverlapCircle(circlePosition, groundedRadius, groundLayers);
            animator.SetBool("Grounded", grounded);
        }

        private void OnDrawGizmos()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (grounded)
            {
                Gizmos.color = transparentGreen;
            }
            else
            {
                Gizmos.color = transparentRed;
            }

            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y, transform.position.z) - (Vector3)groundedOffset, groundedRadius);
        }
    }
}