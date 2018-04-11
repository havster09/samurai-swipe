using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using UnityEngine;

namespace Assets.Scripts
{
    public class Hero : MovingObject
    {
        private bool _heroFlipX;
        public Animator NpcHeroAnimator;
        public NpcHeroAttributesComponent NpcHeroAttributes;

        public bool IsAttacking { get; set; }

        void Awake()
        {
            NpcHeroAttributes = gameObject.GetComponent<NpcHeroAttributesComponent>();
            NpcHeroAnimator = GetComponent<Animator>();
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

        public void Attack(string attackType, NpcAttributesComponent targetNpcAttribute)
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

        public bool IsAnimationPlaying(string animationTag)
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
                   IsAnimationPlaying("rest") ||
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

        public void ResetPosition()
        {
            throw new System.NotImplementedException();
        }
    }
}
