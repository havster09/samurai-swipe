using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public abstract class GoapAgentEnemy : MonoBehaviour, IGoap
{
    public NpcAttributesComponent _npcAttributes;
    public float _moveSpeed = 2;

    public Enemy _enemyScript;
    public GoapAgent _goapAgentScript;


    void Awake()
    {
        _npcAttributes = gameObject.GetComponent<NpcAttributesComponent>();
        _enemyScript = GetComponent<Enemy>();
        _goapAgentScript = GetComponent<GoapAgent>();
    }

    /**
	 * Key-Value data that will feed the GOAP actions and system while planning.
	 */
    public HashSet<KeyValuePair<string, object>> getWorldState()
    {
        HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();

        worldData.Add(new KeyValuePair<string, object>("hasKills", (_npcAttributes.killCount > 2)));
        worldData.Add(new KeyValuePair<string, object>("hasBrave", (_npcAttributes.brave > 0)));
        worldData.Add(new KeyValuePair<string, object>("destroyNpc", false));
        worldData.Add(new KeyValuePair<string, object>("enemyWin", false));
        worldData.Add(new KeyValuePair<string, object>("getBrave", false));

        return worldData;
    }

    /**
	 * Implement in subclasses
	 */
    public abstract HashSet<KeyValuePair<string, object>> createGoalState();


    public virtual void planFailed(HashSet<KeyValuePair<string, object>> failedGoal)
    {
        Debug.Log("Unhandled plan Failed");
        _goapAgentScript.createIdleState();
    }

    public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
    {
        // Yay we found a plan for our goal
        Debug.Log("<color=green>Plan found</color> " + GoapAgent.prettyPrint(actions));
    }

    public void actionsFinished()
    {
        // Everything is done, we completed our actions for this gool. Hooray!
        Debug.Log("<color=blue>Actions completed</color>");
    }

    public void planAborted(GoapAction aborter)
    {
        // An action bailed out of the plan. State has been reset to plan again.
        // Take note of what happened and make sure if you run the same goal again
        // that it can succeed.
        Debug.Log("<color=red>Plan Aborted</color> " + GoapAgent.prettyPrint(aborter));
    }

    public virtual bool moveAgent(GoapAction nextAction)
    {
        // move towards the NextAction's target
        float step = _moveSpeed * Time.deltaTime;
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, nextAction.target.transform.position, step);

        if (gameObject.transform.position.Equals(nextAction.target.transform.position))
        {
            // we are at the target location, we are done
            nextAction.setInRange(true);
            return true;
        }
        else
        {
            return false;
        }
    }
}

