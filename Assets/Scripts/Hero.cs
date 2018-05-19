﻿using System;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using Assets.Scripts.GoapHeroSubStates;
using UnityEngine;

namespace Assets.Scripts
{
    public class Hero : MovingObject
    {
        public const float HeroStep = .5f;
        public GameObject CurrentTarget;
        public bool _heroFlipX;
        public Animator NpcHeroAnimator;
        public NpcHeroAttributesComponent NpcHeroAttributes;
        public DashEndStateMachineHandler DashEndStateMachineHandlerScript;

        public bool IsAttacking { get; set; }

        void Awake()
        {
            NpcHeroAttributes = gameObject.GetComponent<NpcHeroAttributesComponent>();
            NpcHeroAnimator = GetComponent<Animator>();
            NpcRenderer = GetComponent<Renderer>();
            DashEndStateMachineHandlerScript =
                GameObject.FindObjectOfType<DashEndStateMachineHandler>();
            AttachAnimationClipEvents();
        }

        void Start()
        {
            StartSubStateMachines();
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
            MoveBack(CurrentTarget, .35f, 5f, () => HeroBlock(false));
        }

        public void HeroBlock(bool state, GameObject target = null)
        {
            if (target != null) FaceTarget(target);
            NpcHeroAnimator.SetBool("heroBlock", state);
        }

        public void FaceTarget(GameObject target)
        {
            CurrentTarget = target;
            float targetDistance = target.transform.position.x - transform.position.x;

            if (targetDistance < 0 && _heroFlipX == false)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                _heroFlipX = true;
            }
            else if (targetDistance > 0 && _heroFlipX)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                _heroFlipX = false;
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
            NpcHeroAttributes.Rage = 0;
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

        public bool HeroVulnerable()
        {
            return IsAnimationTagPlaying("idle");
        }

        public void Dash(Vector3 end, float speed, RaycastHit2D[] hits)
        {
            NpcHeroAnimator.SetFloat("heroDashAttack", 1);
            StartCoroutine(PerformMovementTo(end, speed, false,
                () =>
                {
                    NpcHeroAnimator.SetFloat("heroDashAttack", 0);
                    DamageRayCastTargets(hits);
                }, 0));
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
