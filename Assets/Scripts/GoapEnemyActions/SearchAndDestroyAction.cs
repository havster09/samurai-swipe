using UnityEngine;

namespace Assets.Scripts.GoapEnemyActions
{
    public class SearchAndDestroyAction : GoapEnemyAction
    {
        public SearchAndDestroyAction()
        {
            addPrecondition("hasBrave", false);
            addPrecondition("destroyNpc", false);
            addEffect("destroyNpc", true);
        }

        public override bool Move()
        {
            if (EnemyScript.IsFrozenPosition())
            {
                return false;
            }

            EnemyScript.FaceTarget();
            float distanceFromTarget = Vector2.Distance(gameObject.transform.position, target.transform.position);
            if (distanceFromTarget > 2 &&
                !EnemyScript.IsAnimationTagPlaying("attack") &&
                !EnemyScript.IsAnimationTagPlaying("walk") && EnemyScript.IsCanWalk)
            {
                GoapAgentSearchAndDestroyRun();
                return false;
            }
            else
            {
                EnemyScript.NpcAnimator.SetBool("enemyRun", false);
                if (distanceFromTarget < 1)
                {
                    setInRange(true);
                    return true;
                }
                else if (EnemyScript.IsAnimationTagPlaying("idle") && EnemyScript.IsCanWalk)
                {
                    EnemyScript.MoveEnemy();
                    return false;
                }
            }
            return false;
        }

        protected void GoapAgentSearchAndDestroyRun()
        {
            float step = (MoveSpeed * 2) * Time.deltaTime;
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, target.transform.position, step);
            EnemyScript.NpcAnimator.SetBool("enemyRun", true);
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
                if (TargetNpcHeroAttribute.Health > 0 && !EnemyScript.IsAnimationTagPlaying("attack"))
                {
                    EnemyScript.Attack("enemyAttackOne");
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