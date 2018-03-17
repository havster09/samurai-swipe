using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapAgentSearchAndDestroy : GoapAgentEnemy
{
    

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

            if (!enemyScript.IsInWalkRange() && !enemyScript.IsAnimationPlaying("attack") && !enemyScript.IsAnimationPlaying("walk"))
            {

                enemyScript.WaitFor(() => GoapAgentSearchAndDestroyRun(nextAction), .5f);
                
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

            if (gameObject.transform.position.Equals(nextAction.target.transform.position))
            {
                nextAction.setInRange(true);
                enemyScript.animator.SetBool("enemyRun", false);
                return true;
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
}


