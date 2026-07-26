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

        public bool IsAttacking { get; set; }
        public bool IsInPoseState;
        public bool IsInResetState;

        private GoapAction GoapActionScript;

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
            GoapActionScript =
                GameObject.FindObjectOfType<GoapAction>();
            AttachAnimationClipEvents();
        }

        protected override void Start()
        {
            StartStateMachines();
            StartSubStateMachines();

        }

        protected override void OnEnable()
        {
            base.OnEnable();
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
            IsAttacking = true;
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
        /// Returns true during any state where player-driven movement must be
        /// suppressed: attacks, cross-sword clash, hit reactions, turn animations.
        /// IsAttacking is an explicit flag (set in Attack, cleared in ReturnToIdle)
        /// so there are no animation-transition gaps during multi-hit combos.
        /// </summary>
        public bool IsBlockingMovement()
        {
            return IsAttacking                     ||
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
                   IsAnimationTagPlaying("attack") ||
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
                    // Real dash: run-up → physical movement toward enemy → damage
                    StartCoroutine(MomentumDashAttack());
                    return 100;
                }
                else if (queued == "heroAttackThree")
                {
                    // Dash slash: run-up → play animation in place
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
            IsAttacking = false;
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
        /// Plays a brief run phase then executes a full dash toward the nearest enemy.
        /// Falls back to a dash-slash attack if no target is available.
        /// </summary>
        public IEnumerator MomentumDashAttack(float runDuration = 0.22f)
        {
            NpcHeroAnimator.SetBool("heroRun", true);
            yield return new WaitForSeconds(runDuration);
            NpcHeroAnimator.SetBool("heroRun", false);

            // Find nearest live target
            var targets = GoapHeroAction.NpcTargetAttributes;
            GameObject closestTarget = null;
            float minDist = float.MaxValue;
            if (targets != null)
            {
                foreach (var t in targets)
                {
                    if (t == null) continue;
                    var enemy = t.GetComponent<Enemy>();
                    if (enemy != null && enemy.IsDead) continue;
                    float d = Vector2.Distance(transform.position, t.transform.position);
                    if (d < minDist) { minDist = d; closestTarget = t.gameObject; }
                }
            }

            if (closestTarget == null)
            {
                // No target in range — fall back to dash-slash in place
                Attack("heroAttackThree");
                yield return new WaitForSeconds(0.9f);
                ReturnToIdle();
                yield break;
            }

            const float DashOffset = 1f;
            var dashEnd = closestTarget.transform.position +
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
