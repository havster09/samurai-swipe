using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroResetPositionAction : GoapHeroAction
    {
        private bool InitialPause { get; set; }


        public GoapHeroResetPositionAction()
        {
            addEffect("resetPosition", true);
        }

        public override void reset()
        {
            HasResetPosition = false;
            InitialPause = false;
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
            if (HeroScript.IsFrozenPosition() || !InitialPause)
            {
                HeroScript.WaitFor(() => InitialPause = true, 1f);
                return false;
            }

            var distanceFromResetPosition = Vector2.Distance(currentHeroPosition, new Vector2(0, 0));

            if (
                Mathf.Floor(distanceFromResetPosition) > ResetPositionThreshold &&
                GoapHeroAction.NpcTargetAttributes.Count < 1 &&
                Mathf.Abs(currentHeroPosition.x) > (Hero.HeroStep + ResetPositionThreshold) &&
                GetActiveNpcAttributesComponentsInRangeByDirection(gameObject) < 1
                )
            {
                HeroScript.HeroResetPosition();
            }
            else
            {
                setInRange(true);
                // todo add reset step method on hero
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
