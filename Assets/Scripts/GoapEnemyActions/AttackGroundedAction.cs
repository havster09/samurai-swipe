using UnityEngine;

namespace Assets.Scripts.GoapEnemyActions
{
    public class AttackGroundedAction : GoapEnemyAction
    {
        private bool _npcHasAttackGroundedAction;

        public AttackGroundedAction()
        {
            addPrecondition("destroyNpc", true);
            addEffect("enemyAttackGounded", true);
            DistanceToTargetThreshold = .3f;
        }

        public override void reset()
        {
            _npcHasAttackGroundedAction = false;
            TargetNpcHeroAttribute = null;
        }

        public override bool isDone()
        {
            return _npcHasAttackGroundedAction;
        }

        public override bool requiresInRange()
        {
            return true;
        }

        public override bool perform(GameObject agent)
        {
            EnemyScript.IsAttacking = true;
            EnemyScript.AttackGrounded();
            EnemyScript.WaitFor(() => EnemyScript.IsAttacking = false, 12f);
            _npcHasAttackGroundedAction = true;
            return _npcHasAttackGroundedAction;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return FindNpcTarget(agent);
        }
    }
}