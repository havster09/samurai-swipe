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
            goal = new HashSet<KeyValuePair<string, object>>();
        }

        public HashSet<KeyValuePair<string, object>> getWorldState()
        {
            WorldData = new HashSet<KeyValuePair<string, object>>
            {
                // todo create different state makers for behaviour
                // new KeyValuePair<string, object>("destroyEnemyNpc", false),
                new KeyValuePair<string, object>("destroyEnemyNpcSingle", false),
                new KeyValuePair<string, object>("destroyEnemyNpcDash", false),
                new KeyValuePair<string, object>("crossSword", false),
                /*new KeyValuePair<string, object>("bloodCover", false),
                new KeyValuePair<string, object>("wipeBlood", false),
                new KeyValuePair<string, object>("hasRage", false),                
                new KeyValuePair<string, object>("resetPosition", false)*/
            };
            return WorldData;
        }

        public HashSet<KeyValuePair<string, object>> createGoalState()
        {
            goal.Clear();

            bool hasLivingTarget = GoapHeroAction.NpcTargetAttributes != null &&
                GoapHeroAction.NpcTargetAttributes.Exists(n => n != null && n.Health > 0);

            if (!hasLivingTarget)
            {
                // Empty goal signals GoapAgent to stay idle without planning.
                // As soon as a slash adds a target, this returns a real goal
                // and GOAP picks it up on the very next frame.
                return goal;
            }

            if (NpcHeroAttributesComponent.Instance.Brave > 10)
                goal.Add(new KeyValuePair<string, object>("destroyEnemyNpcDash", true));
            else
                goal.Add(new KeyValuePair<string, object>("destroyEnemyNpcSingle", true));

            return goal;
        }

        public HashSet<KeyValuePair<string, object>> createPoseState()
        {
            goal.Clear();
            goal.Add(new KeyValuePair<string, object>("destroyEnemyNpcSingle", true));
            goal.Add(new KeyValuePair<string, object>("bloodCover", true));
            goal.Add(new KeyValuePair<string, object>("wipeBlood", true));
            goal.Add(new KeyValuePair<string, object>("resetPosition", true));
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
            Debug.Log("[GOAP] PLAN FAILED  " + GoapAgent.prettyPrint(failedGoal));

            var slashCollider = GameObject.FindGameObjectWithTag("SlashCollider");
            if (
                !Hero.Instance.IsAnimationTagPlaying("dash") &&
                !Hero.Instance.IsAnimationTagPlaying("dashEnd") &&
                slashCollider == null &&
                Mathf.Abs(gameObject.transform.position.x) > (Hero.HeroStep + Hero.ResetPositionThreshold)
                )
            {
                GoapAgentScript.createIdleStateFromGoal(createResetState());
            }
            else
            {
                GoapAgentScript.createIdleStateFromGoal(createGoalState());
            }
        }

        public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
        {
            Debug.Log("[GOAP] PLAN FOUND  " + GoapAgent.prettyPrint(actions));
        }

        public void actionsFinished()
        {
            Debug.Log("[GOAP] ACTIONS FINISHED  replanning");
            GoapAgentScript.createIdleState();
        }

        public void planAborted(GoapAction aborter)
        {
            Debug.Log("[GOAP] PLAN ABORTED  by:" + GoapAgent.prettyPrint(aborter));
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
