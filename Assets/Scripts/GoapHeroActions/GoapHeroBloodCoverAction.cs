using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroBloodCoverAction : GoapHeroAction
    {
        public GoapHeroBloodCoverAction()
        {
            addPrecondition("destroyEnemyNpc", true);
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
            Debug.Log(GetActiveNpcAttributesComponentsInRange(gameObject, 2f));
            if (
                NpcTargetAttributes.Count < 1 &&
                GetActiveNpcAttributesComponentsInRange(gameObject, 2f) < 1 &&
                NpcHeroAttributes.ComboCount > 0 &&
                !HeroScript.NpcHeroAnimator.GetBool("heroBloodCover"))
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
