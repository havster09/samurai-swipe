using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroResetPositionAction : GoapHeroAction
    {
        public GoapHeroResetPositionAction()
        {
            addPrecondition("destroyEnemyNpcSingle", true);
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
            Hero.Instance.IsInResetState = true;
            Vector2 currentHeroPosition = gameObject.transform.position;
            if (
                Hero.Instance.IsFrozenPosition() ||
                Hero.Instance.IsCoroutineMoving
                )
            {
                return false;
            }

            if (Mathf.Abs(currentHeroPosition.x) > (Hero.HeroStep + Hero.ResetPositionThreshold))
            {
                float step = MoveSpeed / 2 * Time.deltaTime;
                float moveDirX = -currentHeroPosition.x; // always moves toward x=0
                gameObject.transform.position =
                    Vector3.MoveTowards(gameObject.transform.position, new Vector3(0, 0), step);
                Hero.Instance.SetWalkAnimation(moveDirX);
            }
            else
            {
                Hero.Instance.StopWalkAnimation();
                setInRange(true);
                return true;
            }
            return false;
        }

        public override bool perform(GameObject agent)
        {
            Hero.Instance.IsInResetState = false;
            HasResetPosition = true;
            return HasResetPosition;
        }
    }
}
