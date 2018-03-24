using UnityEngine;

namespace Assets.Scripts.GoapEnemyActions
{
    public class ChargeDestroyAction : GoapEnemyAction
    {
        public ChargeDestroyAction()
        {
            addPrecondition("hasBrave", true);
            addPrecondition("destroyNpc", false);
            addEffect("destroyNpc", true);
            DistanceToTargetThreshold = 1f;
        }

        public override void reset()
        {
            NpcIsDestroyed = false;
            TargetNpcHeroAttribute = null;
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
            if (TargetNpcHeroAttribute != null)
            {
                if (!EnemyScript.IsAnimationPlaying("attack"))
                {
                    EnemyScript.Attack("enemyAttackTwo");
                    TargetNpcHeroAttribute.health -= 30;
                }
                NpcAttributes.attackCount += 1;
                if (TargetNpcHeroAttribute.health < 1)
                {
                    NpcIsDestroyed = true;
                    NpcAttributes.killCount += 1;
                }
            }
            return NpcIsDestroyed;
        }
    }
}