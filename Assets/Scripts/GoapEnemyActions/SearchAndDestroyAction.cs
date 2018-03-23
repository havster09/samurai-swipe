using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        if (EnemyScript._isAttacking)
        {
            return false;
        }

        EnemyScript.FaceTarget();
        float distanceFromTarget = Vector2.Distance(gameObject.transform.position, target.transform.position);
        if (distanceFromTarget > 2 &&
            !EnemyScript.IsAnimationPlaying("attack") &&
            !EnemyScript.IsAnimationPlaying("walk") && EnemyScript.CanWalk)
        {
            GoapAgentSearchAndDestroyRun();
            return false;
        }
        else
        {
            EnemyScript._animator.SetBool("enemyRun", false);
            if (distanceFromTarget < 1)
            {
                setInRange(true);
                return true;
            }
            else if (EnemyScript.IsAnimationPlaying("idle") && EnemyScript.CanWalk)
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
        EnemyScript._animator.SetBool("enemyRun", true);
    }

    public override void reset()
    {
        _npcIsDestroyed = false;
        TargetNpcHeroAttribute = null;
    }

    public override bool isDone()
    {
        return _npcIsDestroyed;
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
            if (TargetNpcHeroAttribute.health > 0 && !EnemyScript.IsAnimationPlaying("attack"))
            {
                EnemyScript.Attack("enemyAttackOne");
            }            
            NpcAttributes.attackCount += 1;
            if (TargetNpcHeroAttribute.health < 1)
            {
                _npcIsDestroyed = true;
                NpcAttributes.killCount += 1;
            }
        }
        return _npcIsDestroyed;
    }
}