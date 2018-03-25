﻿using System.Collections.Generic;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

namespace Assets.Scripts.GoapHeroTypes
{
    public class GoapAgentHero : MonoBehaviour, IGoap {
        public float MoveSpeed = 2;
        public GoapAgent GoapAgentScript;

        public Hero HeroScript { get; set; }
        public NpcHeroAttributesComponent NpcHeroAttributes { get; set; }

        void Awake()
        {
            NpcHeroAttributes = gameObject.GetComponent<NpcHeroAttributesComponent>();
            HeroScript = GetComponent<Hero>();
            GoapAgentScript = GetComponent<GoapAgent>();
        }

        public HashSet<KeyValuePair<string, object>> getWorldState()
        {
            HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();
            worldData.Add(new KeyValuePair<string, object>("destroyEnemyNpc", false));
            worldData.Add(new KeyValuePair<string, object>("destroyEnemyReset", false));
            return worldData;
        }

        public HashSet<KeyValuePair<string, object>> createGoalState()
        {
            HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
            goal.Add(new KeyValuePair<string, object>("destroyEnemyNpc", true));
            goal.Add(new KeyValuePair<string, object>("destroyEnemyReset", true));
            return goal;
        }

        public void planFailed(HashSet<KeyValuePair<string, object>> failedGoal)
        {
            Debug.Log("Hero Unhandled plan Failed");
            GoapAgentScript.createIdleState();
        }

        public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
        {
            Debug.Log("<color=green>Hero Plan found</color> " + GoapAgent.prettyPrint(actions));
        }

        public void actionsFinished()
        {
            Debug.Log("<color=blue>Hero Actions completed</color>");
        }

        public void planAborted(GoapAction aborter)
        {
            Debug.Log("<color=red>Hero Plan Aborted</color> " + GoapAgent.prettyPrint(aborter));
            GoapAgentScript.createIdleState();
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