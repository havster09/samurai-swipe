using UnityEngine;

namespace Assets.Scripts.GoapEnemyActions
{
    public class TauntAction : GoapEnemyAction
    {
        private bool _npcHasTaunted;
        public TauntAction()
        {
            addPrecondition("hasBrave", false);
            addEffect("hasBrave", true);
            DistanceToTargetThreshold = 3;
        }

        public override void reset()
        {
            _npcHasTaunted = false;
            TargetNpcHeroAttribute = null;
        }

        public override bool isDone()
        {
            return _npcHasTaunted;
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
                if (EnemyScript.IsTaunting == false)
                {
                    EnemyScript.Taunt();
                    EnemyScript.IsTaunting = true;
                    NpcAttributes.Brave += 1;
                    if (NpcAttributes.Brave > 0)
                    {
                        _npcHasTaunted = true;
                    }
                }            
            }
            return _npcHasTaunted;
        }
    }
}