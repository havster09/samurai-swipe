﻿using UnityEngine;

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
            if (
                GetActiveNpcAttributesComponentsInRange(gameObject, PoseThreshold) < 1 &&
                !HeroScript.NpcHeroAnimator.GetBool("heroBloodCover") &&
                InResetRange()
                )
            {
                HeroScript.BloodCover(true);
                NpcIsDestroyedReset = true;
                float coverBloodCountPauseDuration = 5f;
                HeroScript.WaitFor(() =>
                {
                    HeroScript.BloodCover(false);
                }, coverBloodCountPauseDuration);
            }
            else
            {
                NpcIsDestroyedReset = true;
            }
            NpcHeroAttributes.ComboCount = 0;
            return NpcIsDestroyedReset;
        }
    }
}
