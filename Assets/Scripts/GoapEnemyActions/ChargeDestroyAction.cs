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
                if (!EnemyScript.IsAnimationTagPlaying("attack"))
                {
                    EnemyScript.Attack("enemyAttackTwo");
                    TargetNpcHeroAttribute.Health -= 1;
                    NpcAttributes.AttackCount += 1;
                    if (TargetNpcHeroAttribute.Health < 1)
                    {
                        NpcIsDestroyed = true;
                        NpcAttributes.KillCount += 1;
                    }
                }
            }
            return NpcIsDestroyed;
        }
    }
}