using UnityEngine;

namespace Assets.Scripts.GoapEnemyActions
{
    public class AttackJumpAction : GoapEnemyAction
    {
        public AttackJumpAction()
        {
            addPrecondition("hasBrave", true);
            addPrecondition("hasStamina", true);
            addPrecondition("destroyNpc", false);
            addEffect("destroyNpc", true);
            DistanceToTargetThreshold = 1.5f;
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
            if (TargetNpcHeroAttribute != null && NpcAttributes.Stamina > 0)
            {
                if (!EnemyScript.IsAnimationPlaying("attack"))
                {
                    EnemyScript.JumpAttack();
                    NpcAttributes.Stamina -= 100;
                    TargetNpcHeroAttribute.Health -= 1;
                }
                NpcAttributes.AttackCount += 1;
                if (TargetNpcHeroAttribute.Health < 1)
                {
                    NpcIsDestroyed = true;
                    NpcAttributes.KillCount += 1;
                }
            }
            return NpcIsDestroyed;
        }
    }
}