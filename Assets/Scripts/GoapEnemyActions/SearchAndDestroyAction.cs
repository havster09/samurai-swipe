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
        _enemyScript.FaceTarget();
        float distanceFromTarget = Vector2.Distance(gameObject.transform.position, target.transform.position);
        if (distanceFromTarget > 2 &&
            !_enemyScript.IsAnimationPlaying("attack") &&
            !_enemyScript.IsAnimationPlaying("walk") && _enemyScript.canWalk)
        {
            GoapAgentSearchAndDestroyRun();
            return false;
        }
        else
        {
            _enemyScript._animator.SetBool("enemyRun", false);
            if (distanceFromTarget < 1)
            {
                setInRange(true);
                return true;
            }
            else if (_enemyScript.IsAnimationPlaying("idle") && _enemyScript.canWalk)
            {
                _enemyScript.MoveEnemy();
                return false;
            }
        }
        return false;
    }

    protected void GoapAgentSearchAndDestroyRun()
    {
        float step = (_moveSpeed * 2) * Time.deltaTime;
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, target.transform.position, step);
        _enemyScript._animator.SetBool("enemyRun", true);
    }

    public override void reset()
    {
        _npcIsDestroyed = false;
        _targetNpcHeroAttribute = null;
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
        if (_targetNpcHeroAttribute != null)
        {
            if (_targetNpcHeroAttribute.health > 0 && !_enemyScript.IsAnimationPlaying("attack"))
            {
                _enemyScript.Attack("enemyAttackOne");
                _targetNpcHeroAttribute.health -= 1;
            }            
            _npcAttributes.attackCount += 1;
            if (_targetNpcHeroAttribute.health < 1)
            {
                _npcIsDestroyed = true;
                _npcAttributes.killCount += 1;
            }
        }
        return _npcIsDestroyed;
    }
}