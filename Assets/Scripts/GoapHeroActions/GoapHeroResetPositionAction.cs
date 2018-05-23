using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroResetPositionAction : GoapHeroAction
    {
        public GoapHeroResetPositionAction()
        {
            addEffect("resetPosition", true);
        }

        public override void reset()
        {
            HasResetPosition = false;
            TargetNpcAttribute = null;
            target = gameObject;
        }

        public override bool isDone()
        {
            return HasResetPosition;
        }

        public override bool requiresInRange()
        {
            return true;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return !Hero.Instance.IsAnimationTagPlaying("attack") &&
                Hero.Instance.NpcHeroAnimator.GetFloat("heroDashAttack") < .1f;
        }

        public override bool Move()
        {
            Vector2 currentHeroPosition = gameObject.transform.position;
            if (
                Hero.Instance.IsFrozenPosition() ||
                Hero.Instance.IsCoroutineMoving
                )
            {
                return false;
            }

            var walkResetType = 
                (Hero.Instance.HeroFlipX && currentHeroPosition.x < 0) ||
                (!Hero.Instance.HeroFlipX && currentHeroPosition.x > 0) ? 
                "heroWalkBackLoop" : "heroWalkLoop";

            if (Mathf.Abs(currentHeroPosition.x) > (Hero.HeroStep + ResetPositionThreshold))
            {
                float step = MoveSpeed / 2 * Time.deltaTime;
                gameObject.transform.position =
                    Vector3.MoveTowards(gameObject.transform.position, new Vector3(0, 0), step);
                Hero.Instance.NpcHeroAnimator.SetBool(walkResetType, true);
            }
            else
            {
                Hero.Instance.NpcHeroAnimator.SetBool(walkResetType, false);
                setInRange(true);
                return true;
            }
            return false;
        }

        public override bool perform(GameObject agent)
        {
            HasResetPosition = true;
            return HasResetPosition;
        }
    }
}
