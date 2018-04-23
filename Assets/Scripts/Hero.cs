using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using UnityEngine;

namespace Assets.Scripts
{
    public class Hero : MovingObject
    {
        public const float HeroStep = .5f;
        private bool _heroFlipX;
        public Animator NpcHeroAnimator;
        public NpcHeroAttributesComponent NpcHeroAttributes;
        private Coroutine _moveHeroCoroutine;

        public bool IsAttacking { get; set; }

        void Awake()
        {
            NpcHeroAttributes = gameObject.GetComponent<NpcHeroAttributesComponent>();
            NpcHeroAnimator = GetComponent<Animator>();
            NpcRenderer = GetComponent<Renderer>();
            AttachAnimationClipEvents();
        }

        private void AttachAnimationClipEvents()
        {

        }

        public void FaceTarget(GameObject target)
        {
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
            if (NpcHeroAnimator.GetCurrentAnimatorStateInfo(0).Equals(null) ||
                NpcHeroAnimator.GetCurrentAnimatorStateInfo(0).IsTag(animationTag))
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
            var xDir = transform.position.x < 0 ? HeroStep : -HeroStep;
            var end = new Vector2(transform.position.x, 0) + new Vector2(xDir, 0);
            if (!IsCoroutineMoving && GoapHeroAction.NpcTargetAttributes.Count < 1)
            {
                var walkType = _heroFlipX ? "heroWalkBack" : "heroWalk";
                NpcHeroAnimator.SetTrigger(walkType);
                _moveHeroCoroutine = StartCoroutine(PerformMovementTo(end, 1.6f, true, null, 0, NpcHeroAnimator));
            }
        }
    }
}
