using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroChargeDestroyAction : GoapHeroAction
    {
        public GoapHeroChargeDestroyAction()
        {
            addPrecondition("destroyEnemyNpc", false);
            addEffect("destroyEnemyNpc", true);
            addEffect("destroyEnemyReset", false);
            DistanceToTargetThreshold = 1f;
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
            return true;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return FindNpcTarget(agent);
        }

        public override bool perform(GameObject agent)
        {
            Debug.Log("Hero Perform");
            Enemy enemyScript = target.GetComponent<Enemy>();
            if (
                !HeroScript.IsAnimationPlaying("attack") &&
                !HeroScript.IsAnimationPlaying("rest") &&
                !enemyScript.IsDead
                )
            {
                var heroAttacks = new string[]
                {
                    "heroAttackOne",
                    "heroAttackTwo",
                    "heroAttackThree",
                    "heroAttackFour",
                    "heroAttackSix",
                    "heroAttackSeven"
                };
                HeroScript.Attack(heroAttacks[Random.Range(0, heroAttacks.Length - 1)]);
                TargetNpcAttribute.Health -= 50;
                NpcHeroAttributes.AttackCount += 1;
            }

            if (enemyScript.IsDead)
            {
                NpcIsDestroyed = true;
                NpcHeroAttributes.KillCount += 1;
            }
            return NpcIsDestroyed;
        }
    }
}

