using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapAgentSearchAndDestroy : GoapAgentEnemy
{
    private Enemy[] _enemies;
    private NpcHeroAttributesComponent _npcHeroAttributesComponent;

    public override HashSet<KeyValuePair<string, object>> createGoalState()
    {
        HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
        goal.Add(new KeyValuePair<string, object>("destroyNpc", true));
        goal.Add(new KeyValuePair<string, object>("enemyWin", true));
        return goal;
    }

    public override bool moveAgent(GoapAction nextAction)
    {
        _npcHeroAttributesComponent = nextAction.target.GetComponent<NpcHeroAttributesComponent>();
        if (_enemyScript.health > 0 && _npcHeroAttributesComponent.health > 0 && !_enemyScript.IsAnimationPlaying("hit"))
        {

            _enemyScript.FaceTarget();

            if (_npcAttributes.braveCount > 0)
            {
                _enemyScript.FaceTarget();
                GoapAgentSearchAndDestroyRun(nextAction);

                float distanceFromTarget = (gameObject.transform.position - nextAction.target.transform.position).magnitude;

                if (distanceFromTarget < 1f)
                {
                    _enemyScript.animator.SetBool("enemyRun", false);
                    nextAction.setInRange(true);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (!_enemyScript.IsInWalkRange() && !_enemyScript.IsAnimationPlaying("attack") &&
                !_enemyScript.IsAnimationPlaying("walk") && _enemyScript.canWalk)
            {
                GoapAgentSearchAndDestroyRun(nextAction);
                return false;
            }
            else
            {
                _enemyScript.animator.SetBool("enemyRun", false);
                if (_enemyScript.IsAnimationPlaying("idle") && _enemyScript.canWalk)
                {
                    _enemyScript.MoveEnemy();
                    return false;
                }
                else if (_enemyScript.IsInCombatRange())
                {
                    nextAction.setInRange(true);
                    return true;
                }
            }
            return false;
        }
        else if (_npcHeroAttributesComponent.health < 1)
        {
            return false;
        }
        else
        {
            return false;
        }
    }

    private void GoapAgentSearchAndDestroyRun(GoapAction nextAction)
    {
        float step = _moveSpeed * 2 * Time.deltaTime;
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, nextAction.target.transform.position, step);
        _enemyScript.animator.SetBool("enemyRun", true);
    }

    public int CheckActiveCount()
    {
        _enemies = (Enemy[])UnityEngine.GameObject.FindObjectsOfType(typeof(Enemy));
        _enemies.Select(e => e.gameObject.activeInHierarchy && !e.isDead);
        return _enemies.Length;
    }

}


