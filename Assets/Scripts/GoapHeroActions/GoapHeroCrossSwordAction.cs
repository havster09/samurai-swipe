using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroCrossSwordAction : GoapHeroAction
    {
        public GoapHeroCrossSwordAction()
        {
            addEffect("crossSword", true);
            DistanceToTargetThreshold = .6f;
        }

        public override void reset()
        {
            HassCrossedSword = false;
            TargetNpcAttribute = null;
        }

        public override bool isDone()
        {
            return HassCrossedSword;
        }

        public override bool requiresInRange()
        {
            return true;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return FindNpcTarget(agent);
        }

        public override bool perform(GameObject agent)
        {
            var enemyScript = target.GetComponent<Enemy>();
            HeroScript.CrossSword(true);
            enemyScript.CrossSword(true);
            enemyScript.MoveEnemyBack(.1f, 1);
            return HassCrossedSword;
        }
    }
}

