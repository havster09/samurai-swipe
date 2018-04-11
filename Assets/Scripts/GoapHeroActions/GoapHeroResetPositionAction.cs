using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroResetPositionAction : GoapHeroAction
    {
        public GoapHeroResetPositionAction()
        {
            addPrecondition("destroyEnemyNpc", true);
            addEffect("resetPosition", true);
        }

        public override void reset()
        {
            HasResetPosition = false;
            TargetNpcAttribute = null;
        }

        public override bool isDone()
        {
            return HasResetPosition;
        }

        public override bool requiresInRange()
        {
            return false;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return true;
        }
        public override bool perform(GameObject agent)
        {
            Vector2 currentHeroPosition = agent.transform.position;
            if (Mathf.Abs(currentHeroPosition.x) > 0)
            {
                HeroScript.ResetPosition();
            }
            return HasResetPosition;
        }
    }
}
