using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.FsmHeroStates;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using Assets.Scripts.GoapHeroSubStates;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Hero : MovingObject
    {
        public static Hero Instance;
        public const float HeroStep = .5f;
        public const float ResetPositionThreshold = 8f;
        public GameObject CurrentTarget;
        public bool HeroFlipX;
        public Animator NpcHeroAnimator;
        public FsmHeroBaseStateMachineHandler FsmHeroBaseStateMachineHandlerScript;
        public DashEndStateMachineHandler DashEndStateMachineHandlerScript;

        public bool IsInPoseState;
        // True while a momentum-attack coroutine owns the attack slot.
        // Included in IsAttackBusy so perform() doesn't double-fire.
        private bool _momentumPending;
        public bool IsInResetState;

        [Header("Dash Attack")]
        [Tooltip("Seconds enemies cannot attack after a dash lands. Hero finishes animation during this window.")]
        [SerializeField] private float dashPauseDuration = 1.2f;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            NpcHeroAnimator = GetComponent<Animator>();
            NpcRenderer = GetComponent<Renderer>();
            FsmHeroBaseStateMachineHandlerScript =
                GameObject.FindObjectOfType<FsmHeroBaseStateMachineHandler>();
            DashEndStateMachineHandlerScript =
                GameObject.FindObjectOfType<DashEndStateMachineHandler>();
            AttachAnimationClipEvents();
        }

        protected override void Start()
        {
            StartStateMachines();
            StartSubStateMachines();

        }

        private static bool _heroClipEventsAttached = false;

        private void AttachAnimationClipEvents()
        {
            // AnimationClip assets are shared; attach once per play session.
            if (_heroClipEventsAttached) return;
            _heroClipEventsAttached = true;

            var clips = NpcHeroAnimator.runtimeAnimatorController.animationClips;

            AttachHeroClipEvent(clips, "block", "HeroBlockEndEventHandler");
            AttachHeroClipEvent(clips, "hit",   "HeroHitEndEventHandler");
        }

        private static void AttachHeroClipEvent(AnimationClip[] clips, string namePart, string functionName)
        {
            var clip = System.Array.Find(clips,
                c => c.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0);
            if (clip == null)
            {
                Debug.LogWarning(string.Format("[Hero] AttachEvents: clip '{0}' not found", namePart));
                return;
            }
            clip.AddEvent(new AnimationEvent { time = clip.length, functionName = functionName });
        }

        private void StartStateMachines()
        {
            if (FsmHeroBaseStateMachineHandlerScript != null)
                FsmHeroBaseStateMachineHandlerScript.StartFsmHeroIdleStateMachineHandler();
        }

        private void StartSubStateMachines()
        {
            if (DashEndStateMachineHandlerScript != null)
                DashEndStateMachineHandlerScript.StartDashEndStateMachineHandler();
        }

        private void HeroBlockEndEventHandler()
        {
            MoveBack(CurrentTarget, .1f, 1f, () => HeroBlock(false));
            TimingUtilities.Instance.WaitFor(() => NpcHeroAnimator.Play("heroIdle"), .2f);
            // todo use broadcast callback for block event
        }

        public void HeroBlock(bool state, GameObject target = null)
        {
            if (target != null)
            {
                FaceTarget(target);
            }
            NpcHeroAnimator.SetBool("heroBlock", state);
        }

        private void HeroHitEndEventHandler()
        {
            MoveBack(CurrentTarget, .1f, 1f);
            TimingUtilities.Instance.WaitFor(() => HeroHit(false), .1f);
            // todo add delegate
            //if (onHeroHit != null)
            //{
            //    onHeroHit();
            //}
        }

        public void HeroHit(bool state, GameObject target = null)
        {
            if (target != null)
            {
                FaceTarget(target);
            }
            NpcHeroAnimator.SetBool("heroHit", state);
            if (state)
            {
                EnemyHit(1);
            }
        }

        public void EnemyHit(int damage)
        {
            GetBloodEffect("Blood", "BloodEffect1");
            IsHit = true;
        }

        public void FaceTarget(GameObject target)
        {
            CurrentTarget = target;
            float targetDistance = target.transform.position.x - transform.position.x;

            if (targetDistance < 0 && HeroFlipX == false)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                HeroFlipX = true;
            }
            else if (targetDistance > 0 && HeroFlipX)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                HeroFlipX = false;
            }
        }

        public void CrossSword(bool state)
        {
            NpcHeroAnimator.SetBool("heroBlock", false);
            NpcHeroAnimator.SetBool("heroCrossSword", state);
        }

        public void Attack(string attackType)
        {
            NpcHeroAnimator.SetBool("heroRun", false);
            StopWalkAnimation();
            NpcHeroAnimator.SetTrigger(attackType);
        }

        public void BloodCover(bool state)
        {
            NpcHeroAnimator.SetBool("heroBloodCover", state);
            NpcHeroAttributesComponent.Instance.Rage = 0;
        }
        public void TurnPose(bool state)
        {
            NpcHeroAnimator.SetBool("heroTurnPause", state);
        }

        /// <summary>
        /// Instantly flips the hero's facing direction with no animation.
        /// </summary>
        public void TurnAround()
        {
            if (IsBlockingMovement()) return;

            HeroFlipX = !HeroFlipX;
            transform.localRotation = HeroFlipX
                ? Quaternion.Euler(0, 180, 0)
                : Quaternion.Euler(0, 0, 0);

            Debug.Log(string.Format("[Turn] now facing {0}", HeroFlipX ? "LEFT" : "RIGHT"));
        }

        public bool IsAnimationTagPlaying(string animationTag)
        {
            return NpcHeroAnimator.GetCurrentAnimatorStateInfo(0).IsTag(animationTag);
        }

        /// <summary>
        /// True while an attack animation is active OR while the animator is
        /// transitioning into one.  Derived entirely from Animator state — no
        /// manual boolean to keep in sync.
        /// </summary>
        public bool IsAttackBusy
        {
            get
            {
                if (_momentumPending) return true;
                if (IsAnimationTagPlaying("attack")) return true;
                return NpcHeroAnimator.IsInTransition(0) &&
                       NpcHeroAnimator.GetNextAnimatorStateInfo(0).IsTag("attack");
            }
        }

        /// <summary>
        /// True while the hero must not be moved by player input or GOAP.
        /// Derived entirely from Animator state — no manual flags to sync.
        /// </summary>
        public bool IsBlockingMovement()
        {
            return IsAttackBusy                    ||
                   IsAnimationTagPlaying("cross")  ||
                   IsAnimationTagPlaying("rest")   ||
                   IsAnimationTagPlaying("hit")    ||
                   NpcHeroAnimator.GetBool("heroTurnPause");
        }

        public override bool IsFrozenPosition()
        {
            return NpcHeroAnimator.GetBool("heroBloodCover") ||
                   NpcHeroAnimator.GetBool("heroCleanWeapon") ||
                   IsAnimationTagPlaying("rest") ||
                   IsAttackBusy ||
                   NpcHeroAnimator.GetBool("heroTurnPause");
        }

        public void WipeBlood()
        {
            NpcHeroAnimator.SetTrigger("heroWipeBlood");
        }

        public void CleanWeapon()
        {
            NpcHeroAnimator.SetTrigger("heroCleanWeapon");
        }

        public void HeroResetPosition()
        {
            if (GoapHeroAction.NpcTargetAttributes.Count < 1)
            {
                NpcHeroAnimator.SetBool("heroWalkBackReset", true);
            }
        }

        public bool IsVulnerable()
        {
            return IsAnimationTagPlaying("idle");
        }

        public int GetAttackTypeAndDamage(GameObject target)
        {
            Broadcaster<Transform>.SendEvent("FindChild", transform);

            // ── Player queue takes priority ───────────────────────────────────────
            string queued = HeroSwipeController.DequeueAttack();
            if (queued != null)
            {
                Debug.Log(string.Format("[Attack] dequeued -> {0}", queued));

                if (queued == "heroDashAttack")
                {
                    // Async: run-up → dash → damage after animation starts
                    StartCoroutine(MomentumDashAttack(target));
                    return 0;
                }
                else if (queued == "heroDoubleSlashHigh" ||
                         queued == "heroDoubleSlashMid"  ||
                         queued == "heroDoubleSlashLow")
                {
                    // Async: animation fires first, damage applied once it is active
                    StartCoroutine(DelayedDoubleSlash(queued, target));
                    return 0;
                }
                else if (queued == "heroAttackThree")
                {
                    StartCoroutine(MomentumAttack("heroAttackThree"));
                    return 100;
                }
                else
                {
                    Attack(queued);
                }

                return SwipeAttackMap.DamageForTrigger(queued);
            }

            // ── No swipe queued — fall back to original contextual GOAP attack immediately ──
            Debug.Log("[GOAP] FALLBACK ATTACK  no swipe queued — using random contextual");
            var heroAttacks = new System.Collections.Generic.List<string>();
            var damage = 0;

            if (target.transform.position.y > 0)
            {
                heroAttacks.Add("heroAttackFour");
                heroAttacks.Add("heroAttackSix");
                damage = 100;
            }
            else if (SlashRenderer.Instance.CrossSlashCounter > 1)
            {
                heroAttacks.Add("heroDoubleSlashMid");
                heroAttacks.Add("heroDoubleSlashHigh");
                heroAttacks.Add("heroDoubleSlashLow");
                damage = 50;
            }
            else
            {
                heroAttacks.Add("heroAttackOne");
                heroAttacks.Add("heroAttackThree");
                heroAttacks.Add("heroAttackFour");
                heroAttacks.Add("heroAttackSix");
                heroAttacks.Add("heroAttackSeven");
                damage = 100;
            }

            string fallback = heroAttacks[UnityEngine.Random.Range(0, heroAttacks.Count)];
            Attack(fallback);
            return damage;
        }

        public bool IsAttackable()
        {
            return !IsInPoseState && !IsInResetState;
        }

        // ── Momentum attacks (run-up before dash/slash) ───────────────────────────

        /// <summary>
        /// Clears all movement and attack animator booleans so the FSM can
        /// settle back to idle naturally.
        /// </summary>
        /// <summary>
        /// Sets heroWalkLoop or heroWalkBackLoop based on whether the movement
        /// direction matches the hero's current facing direction.
        /// Pass the raw X delta (targetX - heroX) or any signed movement value.
        /// </summary>
        public void SetWalkAnimation(float movementDirX)
        {
            float facing  = HeroFlipX ? -1f : 1f;         // -1 = facing left, +1 = facing right
            bool  forward = Mathf.Sign(movementDirX) == facing;
            NpcHeroAnimator.SetBool("heroWalkLoop",     forward);
            NpcHeroAnimator.SetBool("heroWalkBackLoop", !forward);
        }

        /// <summary>Clears both walk animation booleans.</summary>
        public void StopWalkAnimation()
        {
            NpcHeroAnimator.SetBool("heroWalkLoop",     false);
            NpcHeroAnimator.SetBool("heroWalkBackLoop", false);
        }

        private void ReturnToIdle()
        {
            _momentumPending = false;
            NpcHeroAnimator.SetBool("heroRun", false);
            StopWalkAnimation();
        }

        /// <summary>
        /// Plays a brief run phase then fires <paramref name="attackTrigger"/>.
        /// After the expected animation window, resets to idle.
        /// </summary>
        public IEnumerator MomentumAttack(string attackTrigger, float runDuration = 0.22f, float postDelay = 0.9f)
        {
            NpcHeroAnimator.SetBool("heroRun", true);
            yield return new WaitForSeconds(runDuration);
            NpcHeroAnimator.SetBool("heroRun", false);

            Attack(attackTrigger);

            yield return new WaitForSeconds(postDelay);
            ReturnToIdle();
        }

        /// <summary>
        /// Run-up → full dash toward target → damage applied once dash animation is active.
        /// Returns 0 to GetAttackTypeAndDamage so GOAP does not double-apply damage.
        /// </summary>
        public IEnumerator MomentumDashAttack(GameObject target, float runDuration = 0.22f)
        {
            _momentumPending = true;

            NpcHeroAnimator.SetBool("heroRun", true);
            yield return new WaitForSeconds(runDuration);
            NpcHeroAnimator.SetBool("heroRun", false);

            // Find nearest live target (prefer the GOAP-supplied one)
            GameObject dashTarget = null;
            if (target != null)
            {
                var te = target.GetComponent<Enemy>();
                if (te != null && !te.IsDead) dashTarget = target;
            }

            if (dashTarget == null)
            {
                // Fall back to nearest in list
                var targets = GoapHeroAction.NpcTargetAttributes;
                float minDist = float.MaxValue;
                if (targets != null)
                {
                    foreach (var t in targets)
                    {
                        if (t == null) continue;
                        var e = t.GetComponent<Enemy>();
                        if (e != null && e.IsDead) continue;
                        float d = Vector2.Distance(transform.position, t.transform.position);
                        if (d < minDist) { minDist = d; dashTarget = t.gameObject; }
                    }
                }
            }

            if (dashTarget == null)
            {
                Attack("heroAttackThree");
                yield return new WaitForSeconds(0.9f);
                ReturnToIdle();
                yield break;
            }

            const float DashOffset = 1f;
            var dashEnd    = dashTarget.transform.position +
                             new Vector3(HeroFlipX ? -DashOffset : DashOffset, 0f, 0f);
            var dashEndPos = HeroFlipX
                ? Vector3.Max(dashEnd, new Vector3(-3.5f, 0f, 0f))
                : Vector3.Min(dashEnd, new Vector3(3.5f, 0f, 0f));
            var dashRayCastEnd = dashEndPos +
                                 new Vector3(HeroFlipX ? -DashOffset : DashOffset, 0f, 0f);

            var box      = GetComponent<BoxCollider2D>();
            var startPos = new Vector2(transform.position.x, box != null ? box.offset.y : 0f);
            var hits     = Physics2D.RaycastAll(
                startPos,
                HeroFlipX ? Vector2.left : Vector2.right,
                Vector2.Distance(startPos, dashRayCastEnd),
                512);

            Dash(dashEndPos, 6f, hits);

            // Wait for the dash animation to start, then apply damage
            yield return new WaitUntil(() => IsAnimationTagPlaying("dash") || IsAnimationTagPlaying("dashEnd"));
            var enemyScript = dashTarget.GetComponent<Enemy>();
            if (enemyScript != null && !enemyScript.IsDead)
            {
                enemyScript.EnemyHitSuccess(SwipeAttackMap.DamageForTrigger("heroDashAttack"));
            }

            // Dramatic pause: freeze enemy attacks while hero finishes dash animation.
            // Enemies can still reposition. Chain effects subscribe to CombatPauseManager events.
            if (CombatPauseManager.Instance != null)
                CombatPauseManager.Instance.TriggerPause(dashPauseDuration);

            _momentumPending = false;
        }

        /// <summary>
        /// Fires a double-slash trigger and waits for the attack animation to start
        /// before applying damage.  Ensures the visual precedes the hit.
        /// Returns 0 to GetAttackTypeAndDamage so GOAP does not double-apply damage.
        /// </summary>
        public IEnumerator DelayedDoubleSlash(string trigger, GameObject target)
        {
            _momentumPending = true;

            Attack(trigger);

            // Wait until the attack animation is actually running
            yield return new WaitUntil(() => IsAnimationTagPlaying("attack"));

            var enemyScript = target != null ? target.GetComponent<Enemy>() : null;
            if (enemyScript != null && !enemyScript.IsDead)
            {
                enemyScript.EnemyHitSuccess(SwipeAttackMap.DamageForTrigger(trigger));
            }

            // Wait for animation to finish before releasing the slot
            yield return new WaitUntil(() => !IsAnimationTagPlaying("attack"));

            ReturnToIdle();
        }

        // ─────────────────────────────────────────────────────────────────────────

        public void Dash(Vector3 end, float speed, RaycastHit2D[] hits)
        {
            SlashRenderer.Instance.RemoveSlash();
            NpcHeroAnimator.SetFloat("heroDashAttack", 1);
            StartCoroutine(PerformMovementTo(end, speed, false,
                () =>
                {
                    NpcHeroAnimator.SetFloat("heroDashAttack", 0);
                    DamageRayCastTargets(hits);
                    ReturnToIdle();
                }, 0));
        }

        public void HandleDashEndEvent(RaycastHit2D[] hits)
        {
            NpcHeroAnimator.SetFloat("heroDashAttack", 0);
            DamageRayCastTargets(hits);
            ReturnToIdle();
        }

        public void DamageRayCastTargets(RaycastHit2D[] hits)
        {
            
            hits.ToList().ForEach(h =>
            {
                var slashTarget = h.collider.gameObject;
                var enemyScript = slashTarget.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.EnemyHitSuccess(100);
                }
            });
        }
    }
}
