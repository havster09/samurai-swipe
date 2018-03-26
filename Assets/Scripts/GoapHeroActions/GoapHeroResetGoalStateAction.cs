using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroResetGoalStateAction : GoapHeroAction
    {
        public GoapHeroResetGoalStateAction()
        {
            addPrecondition("destroyEnemyReset", false);
            addEffect("destroyEnemyReset", true);
            addEffect("destroyEnemy", false);
            DistanceToTargetThreshold = 1f;
        }

        public override void reset()
        {
            NpcIsDestroyedReset = false;
            TargetNpcAttribute = null;
        }

        public override bool isDone()
        {
            return NpcIsDestroyedReset;
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
            if (HeroScript.NpcHeroAnimator.GetBool("heroBloodCover") == false)
            {
                HeroScript.NpcHeroAnimator.SetBool("heroBloodCover", true);
                HeroScript.WaitFor(() =>
                {
                    HeroScript.NpcHeroAnimator.SetBool("heroBloodCover", false);
                    NpcIsDestroyedReset = true;
                }, 2f);
            }
            return NpcIsDestroyedReset;
        }
    }
}
