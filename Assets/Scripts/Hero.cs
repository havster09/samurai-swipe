using System;
using System.Collections.Generic;
using System.Linq;
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
        public const float ResetPositionThreshold = 1f;
        public GameObject CurrentTarget;
        public bool HeroFlipX;
        public Animator NpcHeroAnimator;
        public DashEndStateMachineHandler DashEndStateMachineHandlerScript;

        public delegate void OnHeroBlocked();
        public static event OnHeroBlocked onHeroBlocked;

        public bool IsAttacking { get; set; }
        public bool IsInPoseState;
        public bool IsInResetState;
        public GameObject Target;

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
            DashEndStateMachineHandlerScript =
                GameObject.FindObjectOfType<DashEndStateMachineHandler>();
            AttachAnimationClipEvents();
        }

        protected override void Start()
        {
            StartSubStateMachines();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        private void AttachAnimationClipEvents()
        {
            var blockClip = NpcHeroAnimator.runtimeAnimatorController.animationClips[23];

            var blockEventMid = new AnimationEvent();
            blockEventMid.time = blockClip.length / 2;
            blockEventMid.functionName = "HeroBlockEndEventHandler";
            blockClip.AddEvent(blockEventMid);
        }

        private void StartSubStateMachines()
        {
            DashEndStateMachineHandlerScript.StartDashEndStateMachineHandler();
        }

        private void HeroBlockEndEventHandler()
        {
            MoveBack(CurrentTarget, .1f, 1f, () => HeroBlock(false));
            // todo reafactor this
            if (onHeroBlocked != null)
            {
                onHeroBlocked();
            }
        }

        public void HeroBlock(bool state, GameObject target = null)
        {
            if (target != null)
            {
                FaceTarget(target);
                GoapHeroAction.Instance.TargetNpcAttribute = null;
            }
            NpcHeroAnimator.SetBool("heroBlock", state);
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
            var heroAttacks = new List<string>();

            var damage = 0;
            if (target.transform.position.y > 0)
            {
                heroAttacks.AddRange(new List<string>
                {
                    "heroAttackFour", // slash high
                    "heroAttackSix", // crouch slash high
                });
                damage = 100;
            }
            else if (NpcHeroAttributesComponent.Instance.Rage > 10)
            {

                heroAttacks.AddRange(new List<string>
                {
                    "heroDoubleSlashMid",
                    "heroDoubleSlashHigh",
                    "heroDoubleSlashLow",
                });
                damage = 50;
            }
            else
            {
                heroAttacks.AddRange(new List<string>
                {
                    "heroAttackOne", // slash down
                    "heroAttackSeven" // step strong slash
                });
                damage = 100;
            }
            Attack(heroAttacks[Random.Range(0, heroAttacks.Count)]);
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
