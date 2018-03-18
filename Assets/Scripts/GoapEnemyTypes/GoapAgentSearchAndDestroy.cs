using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapAgentSearchAndDestroy : GoapAgentEnemy
{
    private Enemy[] enemies;

    public override HashSet<KeyValuePair<string, object>> createGoalState()
    {
        HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
        goal.Add(new KeyValuePair<string, object>("destroyNpc", true));
        return goal;
    }

    public override bool moveAgent(GoapAction nextAction)
    {
        if (enemyScript.health > 0 && !enemyScript.IsAnimationPlaying("hit"))
        {
            enemyScript.FaceTarget();

            if (!enemyScript.IsInWalkRange() && !enemyScript.IsAnimationPlaying("attack") &&
                !enemyScript.IsAnimationPlaying("walk") && enemyScript.canWalk)
            {
                GoapAgentSearchAndDestroyRun(nextAction);
            }
            else
            {
                enemyScript.animator.SetBool("enemyRun", false);
                if (enemyScript.IsAnimationPlaying("idle") && enemyScript.canWalk)
                {
                    enemyScript.MoveEnemy();
                }
                else if (enemyScript.IsInCombatRange())
                {
                    nextAction.setInRange(true);
                    return true;
                }
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    private void GoapAgentSearchAndDestroyRun(GoapAction nextAction)
    {
        float step = moveSpeed * Time.deltaTime;
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, nextAction.target.transform.position, step);
        enemyScript.animator.SetBool("enemyRun", true);
    }

    public int CheckActiveCount()
    {
        enemies = (Enemy[])UnityEngine.GameObject.FindObjectsOfType(typeof(Enemy));
        enemies.Select(e => e.gameObject.activeInHierarchy && !e.isDead);
        return enemies.Length;
    }

}


