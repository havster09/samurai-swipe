using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroBloodCoverAction : GoapHeroAction
    {
        public GoapHeroBloodCoverAction()
        {
            addPrecondition("destroyEnemyNpcSingle", true);
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
            if (
                GetActiveNpcAttributesComponentsInRange(gameObject, PoseThreshold).Length < 10 &&
                !Hero.Instance.NpcHeroAnimator.GetBool("heroBloodCover") &&
                InResetRange() // todo review this check
                )
            {
                Hero.Instance.IsInPoseState = true;
                Hero.Instance.BloodCover(true);
                NpcIsDestroyedReset = true;
                float coverBloodCountPauseDuration = 5f;
                TimingUtilities.Instance.WaitFor(() =>
                {
                    Hero.Instance.BloodCover(false);
                }, coverBloodCountPauseDuration);
            }
            else
            {
                Hero.Instance.IsInPoseState = false;
                NpcIsDestroyedReset = true;
            }
            NpcHeroAttributesComponent.Instance.ComboCount = 0;
            return NpcIsDestroyedReset;
        }
    }
}
