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

            var to = .6f;
            var speed = 5;

            float distance = target.transform.position.x > transform.position.x ? to : -to;
            Vector2 end = new Vector2(transform.position.x, 0) + new Vector2(distance, 0);
            StartCoroutine(enemyScript.MovementTo(end, speed, TargetNpcAttribute, () =>
            {
                
            }));

            return HassCrossedSword;
        }
    }
}

