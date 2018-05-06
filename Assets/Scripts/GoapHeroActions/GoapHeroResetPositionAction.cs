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
            return !HeroScript.IsAnimationTagPlaying("attack") &&
                HeroScript.NpcHeroAnimator.GetFloat("heroDashAttack") < .1f;
        }

        public override bool Move()
        {
            Vector2 currentHeroPosition = gameObject.transform.position;
            if (
                HeroScript.IsFrozenPosition() ||
                HeroScript.IsCoroutineMoving
                )
            {
                return false;
            }

            var distanceFromResetPosition = Vector2.Distance(currentHeroPosition, new Vector2(0, 0));

            var walkResetType = HeroScript._heroFlipX ? "heroWalkBackLoop" : "heroWalkLoop";

            if (
                Mathf.Floor(distanceFromResetPosition) > ResetPositionThreshold &&
                GoapHeroAction.NpcTargetAttributes.Count < 1 &&
                Mathf.Abs(currentHeroPosition.x) > (Hero.HeroStep + ResetPositionThreshold) &&
                GetActiveNpcAttributesComponentsInRangeByDirection(gameObject) < 1
                )
            {
                float step = MoveSpeed / 2 * Time.deltaTime;
                gameObject.transform.position =
                    Vector3.MoveTowards(gameObject.transform.position, new Vector3(0, 0), step);
                HeroScript.NpcHeroAnimator.SetBool(walkResetType, true);
            }
            else
            {
                HeroScript.NpcHeroAnimator.SetBool(walkResetType, false);
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
