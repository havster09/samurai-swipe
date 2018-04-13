using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroResetPositionAction : GoapHeroAction
    {
        private const float ResetPositionThreshold = 1.5f;
        

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

        public override bool Move()
        {
            Vector2 currentHeroPosition = gameObject.transform.position;
            if (HeroScript.IsFrozenPosition())
            {
                return false;
            }

            // todo create new action to force hero back into screen

            if (
                !NpcHeroRenderer.isVisible ||
                Mathf.Round(Vector2.Distance(currentHeroPosition, new Vector2(0, 0))) > ResetPositionThreshold &&
                GetActiveNpcAttributesComponentsInRangeByDirection(gameObject) < 1
                )
            {
                HeroScript.HeroResetPosition();
            }
            else
            {
                setInRange(true);
                return true;
            }
            return false;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return true;
        }
        public override bool perform(GameObject agent)
        {
            HasResetPosition = true;
            return HasResetPosition;
        }
    }
}
