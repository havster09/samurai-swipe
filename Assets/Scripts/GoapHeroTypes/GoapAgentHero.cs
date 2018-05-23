using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using UnityEngine;

namespace Assets.Scripts.GoapHeroTypes
{
    public class GoapAgentHero : MonoBehaviour, IGoap {
        public static GoapAgentHero Instance;
        public float MoveSpeed = 2;
        public GoapAgent GoapAgentScript;
        public GoapHeroAction GoapHeroActionScript;

        protected HashSet<KeyValuePair<string, object>> WorldData;
        public HashSet<KeyValuePair<string, object>> goal;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            GoapAgentScript = GetComponent<GoapAgent>();
            GoapHeroActionScript = GetComponent<GoapHeroAction>();
            goal = new HashSet<KeyValuePair<string, object>>();
        }

        public HashSet<KeyValuePair<string, object>> getWorldState()
        {
            WorldData = new HashSet<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("destroyEnemyNpc", false),
                new KeyValuePair<string, object>("crossSword", false),
                /*new KeyValuePair<string, object>("bloodCover", false),
                new KeyValuePair<string, object>("wipeBlood", false),
                new KeyValuePair<string, object>("hasRage", false),*/                
                new KeyValuePair<string, object>("resetPosition", false)
            };
            return WorldData;
        }

        public HashSet<KeyValuePair<string, object>> createGoalState()
        {
            goal.Clear();
            goal.Add(new KeyValuePair<string, object>("destroyEnemyNpc", true));
            goal.Add(new KeyValuePair<string, object>("bloodCover", true));
            goal.Add(new KeyValuePair<string, object>("wipeBlood", true));
            return goal;
        }

        public HashSet<KeyValuePair<string, object>> createResetState()
        {
            goal.Clear();
            goal.Add(new KeyValuePair<string, object>("resetPosition", true));
            return goal;
        }

        public void planFailed(HashSet<KeyValuePair<string, object>> failedGoal)
        {
            Debug.Log("<color=red>Hero Unhandled plan Failed</color> " + GoapAgent.prettyPrint(failedGoal));
            if (
                !Hero.Instance.IsAnimationTagPlaying("dash") &&
                !Hero.Instance.IsAnimationTagPlaying("dashEnd") &&
                Mathf.Abs(gameObject.transform.position.x) > (Hero.HeroStep + 1f)
                )
            {
                GoapAgentScript.createResetState();
            }
            else
            {
                GoapAgentScript.createIdleState();
            }
        }

        public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
        {
            Debug.Log("<color=green>Hero Plan found</color> " + GoapAgent.prettyPrint(actions));
        }

        public void actionsFinished()
        {
            Debug.Log("<color=blue>Hero Actions completed</color>");
            GoapAgentScript.createIdleState();
        }

        public void planAborted(GoapAction aborter)
        {
            Debug.Log("<color=red>Hero Plan Aborted</color> " + GoapAgent.prettyPrint(aborter));
        }

        public bool moveAgent(GoapAction nextAction)
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
