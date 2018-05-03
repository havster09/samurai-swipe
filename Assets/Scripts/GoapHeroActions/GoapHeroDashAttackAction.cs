using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroDashAttackAction : GoapHeroBaseAttackAction
    {
        public GoapHeroDashAttackAction()
        {
            addEffect("destroyEnemyNpc", true);
            DistanceToTargetThreshold = 3f;
        }

        public override void reset()
        {
            NpcIsDestroyed = false;
            TargetNpcAttribute = null;
        }

        public override bool isDone()
        {
            return NpcIsDestroyed;
        }

        public override bool requiresInRange()
        {
            return false;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            if (target == null)
            {
                return FindLastNpcDashTarget(agent) && !HeroScript.IsAnimationTagPlaying("attack");
            }
            return target;
        }

        public override bool perform(GameObject agent)
        {
            if (HeroScript.NpcHeroAnimator.GetFloat("heroDashAttack") < .1f && !HeroScript.IsCoroutineMoving)
            {
                HeroScript.Dash(target.transform.position, 6f);
                NpcIsDestroyed = true;

                Debug.Log(string.Format("<color=green>Active Targets {0}</color>", NpcTargetAttributes.Count));
            }

            return NpcIsDestroyed;
        }
    }
}

