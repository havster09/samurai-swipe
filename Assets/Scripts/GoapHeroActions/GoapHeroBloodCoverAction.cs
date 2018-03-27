using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroBloodCoverAction : GoapHeroAction
    {
        public GoapHeroBloodCoverAction()
        {
            addPrecondition("destroyEnemyNpc", true);
            addEffect("bloodCover", false);
            addEffect("bloodCover", true);
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
            // todo check for number of active enemies in range too
            if (
                NpcTargetAttributes.Count == 0 &&
                NpcHeroAttributes.ComboCount > 4 &&
                HeroScript.NpcHeroAnimator.GetBool("heroBloodCover") == false)
            {
                HeroScript.BloodCover(true);
                float coverBloodCountPauseDuration = NpcHeroAttributes.ComboCount;
                HeroScript.WaitFor(() =>
                {
                    HeroScript.BloodCover(false);
                    NpcIsDestroyedReset = true;
                }, coverBloodCountPauseDuration);
            }
            else
            {
                NpcIsDestroyedReset = true;
                NpcTargetAttributes.Remove(TargetNpcAttribute);
            }
            NpcHeroAttributes.ComboCount = 0;
            return NpcIsDestroyedReset;
        }
    }
}
