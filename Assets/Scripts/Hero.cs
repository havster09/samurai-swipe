using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using UnityEngine;

namespace Assets.Scripts
{
    public class Hero : MovingObject
    {
        private bool _heroFlipX;
        public Animator NpcHeroAnimator;
        private GoapHeroAction _goapHeroAction;
        public NpcHeroAttributesComponent NpcHeroAttributes;

        public bool IsAttacking { get; set; }

        void Awake()
        {
            NpcHeroAttributes = gameObject.GetComponent<NpcHeroAttributesComponent>();
            NpcHeroAnimator = GetComponent<Animator>();
            _goapHeroAction = GetComponent<GoapHeroAction>();
            AttachAnimationClipEvents();
        }

        private void AttachAnimationClipEvents()
        {

        }

        public void FaceTarget()
        {
            float targetDistance = _goapHeroAction.target.transform.position.x - transform.position.x;

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

        public void Attack(string attackType)
        {
            IsAttacking = true;
            NpcHeroAnimator.SetTrigger(attackType);
        }

        public void BloodCover(bool state)
        {
            NpcHeroAnimator.SetBool("heroBloodCover", state);
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
            return NpcHeroAnimator.GetBool("heroBloodCover");
        }

        protected override void OnCantMove<T>(T component)
        {
            throw new System.NotImplementedException();
        }
    }
}
