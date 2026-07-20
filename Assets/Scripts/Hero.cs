using System;
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

        private void AttachAnimationClipEvents()
        {
            var blockClip = NpcHeroAnimator.runtimeAnimatorController.animationClips[23];
            
            var blockEventEnd = new AnimationEvent();
            blockEventEnd.time = blockClip.length;
            blockEventEnd.functionName = "HeroBlockEndEventHandler";
            blockClip.AddEvent(blockEventEnd);

            var hitClip = NpcHeroAnimator.runtimeAnimatorController.animationClips[28];

            var hitEventEnd = new AnimationEvent();
            hitEventEnd.time = hitClip.length;
            hitEventEnd.functionName = "HeroHitEndEventHandler";
            hitClip.AddEvent(hitEventEnd);
        }

        private void StartStateMachines()
        {
            FsmHeroBaseStateMachineHandlerScript.StartFsmHeroIdleStateMachineHandler();
        }
        private void StartSubStateMachines()
        {
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
            NpcHeroAnimator.SetTrigger(attackType);
            CheckEnemiesInRangeOfAttack();
        }

        private void CheckEnemiesInRangeOfAttack()
        {
            var totalNpcInRangeOfAttack = GoapActionScript.GetActiveNpcAttributesComponentsInRangeByDirection(gameObject, 1.2f);
            totalNpcInRangeOfAttack.ToList()
                .ForEach((npc) =>
                {
                    var enemyScript = npc.GetComponent<Enemy>();
                    enemyScript.EnemyHitSuccess(50);
                });
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

        public bool IsAnimationTagPlaying(string animationTag)
        {
            if (NpcHeroAnimator.GetCurrentAnimatorStateInfo(0).IsTag(animationTag))
            {
                return true;
            }
            return false;
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
                Attack(queued);
                // Derive damage tier from the attack name
                if (queued == "heroDoubleSlashHigh" || queued == "heroDoubleSlashLow" ||
                    queued == "heroDoubleSlashMid"  || queued == "heroAttackSeven")
                    return 150;
                if (queued == "heroAttackThree" || queued == "heroAttackFour" ||
                    queued == "heroAttackFive"   || queued == "heroDashAttack")
                    return 100;
                return 60;
            }

            // ── No swipe queued — fall back to original contextual GOAP attack immediately ──
            // Hero keeps fighting without pause; player swipes only override the next move.
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

        public void Dash(Vector3 end, float speed, RaycastHit2D[] hits)
        {
            SlashRenderer.Instance.RemoveSlash();
            NpcHeroAnimator.SetFloat("heroDashAttack", 1);
            StartCoroutine(PerformMovementTo(end, speed, false,
                () =>
                {
                    NpcHeroAnimator.SetFloat("heroDashAttack", 0);
                    DamageRayCastTargets(hits);
                }, 0));
        }

        public void HandleDashEndEvent(RaycastHit2D[] hits)
        {
            NpcHeroAnimator.SetFloat("heroDashAttack", 0);
            DamageRayCastTargets(hits);
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
