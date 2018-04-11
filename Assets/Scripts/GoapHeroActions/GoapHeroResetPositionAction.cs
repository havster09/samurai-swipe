using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroResetPositionAction : GoapHeroAction
    {
        public GoapHeroResetPositionAction()
        {
            addPrecondition("wipeBlood", true);
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

            if (Vector2.Distance(currentHeroPosition, new Vector2(0, 0)) > 1f)
            {
                HeroScript.ResetPosition();
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
