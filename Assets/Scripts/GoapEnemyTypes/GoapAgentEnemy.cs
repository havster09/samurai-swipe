using System.Collections.Generic;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

namespace Assets.Scripts.GoapEnemyTypes
{
    public abstract class GoapAgentEnemy : MonoBehaviour, IGoap
    {
        public NpcAttributesComponent NpcAttributes;
        public float MoveSpeed = 2;

        public Enemy EnemyScript;
        public GoapAgent GoapAgentScript;


        void Awake()
        {
            NpcAttributes = gameObject.GetComponent<NpcAttributesComponent>();
            EnemyScript = GetComponent<Enemy>();
            GoapAgentScript = GetComponent<GoapAgent>();
        }

        /**
	 * Key-Value data that will feed the GOAP actions and system while planning.
	 */
        public HashSet<KeyValuePair<string, object>> getWorldState()
        {
            HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();

            worldData.Add(new KeyValuePair<string, object>("hasStamina", (NpcAttributes.Stamina > 0)));
            worldData.Add(new KeyValuePair<string, object>("hasKills", (NpcAttributes.KillCount > 0)));
            worldData.Add(new KeyValuePair<string, object>("hasBrave", (NpcAttributes.Brave > 3)));
            worldData.Add(new KeyValuePair<string, object>("enemyAttackGrounded", false));
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
            GoapAgentScript.createIdleState();
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
            GoapAgentScript.createIdleState();
        }

        public virtual bool moveAgent(GoapAction nextAction)
        {
            float step = MoveSpeed * Time.deltaTime;
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, nextAction.target.transform.position, step);

            if (gameObject.transform.position.Equals(nextAction.target.transform.position))
            {
                nextAction.setInRange(true);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

