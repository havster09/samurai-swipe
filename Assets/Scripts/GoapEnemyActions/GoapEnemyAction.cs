﻿using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

namespace Assets.Scripts.GoapEnemyActions
{
    public class GoapEnemyAction : GoapAction
    {
        protected float MoveSpeed = 1;
        protected float DistanceToTargetThreshold = 1;
        protected bool NpcIsDestroyed;
        protected Enemy EnemyScript;
        protected NpcHeroAttributesComponent TargetNpcHeroAttribute;
        protected NpcAttributesComponent NpcAttributes;
        public Hero HeroScript;

        protected bool IsPerforming { get; set; }

        public GoapEnemyAction(){}

        void Awake()
        {
            EnemyScript = GetComponent<Enemy>();
            NpcAttributes = GetComponent<NpcAttributesComponent>();
            HeroScript = FindObjectOfType<Hero>();
        }

        void OnEnable()
        {
            reset();
        }

        public override bool Move()
        {
            if (!HeroScript.IsFrozenPosition())
            {
                if (!EnemyScript.IsFrozenPosition())
                {
                    EnemyScript.FaceTarget();
                    float distanceFromTarget =
                        Vector2.Distance(gameObject.transform.position, target.transform.position);

                    if (distanceFromTarget >= DistanceToTargetThreshold)
                    {
                        float step = (MoveSpeed * 2) * Time.deltaTime;
                        gameObject.transform.position =
                            Vector3.MoveTowards(gameObject.transform.position, target.transform.position, step);
                        EnemyScript.NpcAnimator.SetBool("enemyRun", true);
                    }
                    else
                    {
                        EnemyScript.NpcAnimator.SetBool("enemyRun", false);
                        setInRange(true);
                        return true;
                    }
                }
            }
            else
            {
                if(EnemyScript.NpcAnimator.GetBool("enemyCrossSword"))
                {
                    return false; //todo refactor movement to a shared method between hero and enemy
                }

                if (EnemyScript.NpcAnimator.GetBool("enemyRun"))
                {
                    EnemyScript.NpcAnimator.SetBool("enemyRun", false);
                }
                doReset();
            }
            return false;
        }

        public override void reset()
        {
            NpcIsDestroyed = false;
            TargetNpcHeroAttribute = null;
        }

        public override bool isDone()
        {
            return NpcIsDestroyed;
        }

        public override bool requiresInRange()
        {
            return true;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return FindNpcTarget(agent);
        }

        public virtual bool FindNpcTarget(GameObject agent)
        {
            NpcHeroAttributesComponent[] npcHeroAttributes = (NpcHeroAttributesComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(NpcHeroAttributesComponent));
            NpcHeroAttributesComponent closest = null;
            float closestDist = 0;

            foreach (NpcHeroAttributesComponent npc in npcHeroAttributes)
            {
                if (closest == null)
                {
                    closest = npc;
                    closestDist = (npc.gameObject.transform.position - agent.transform.position).magnitude;
                }
                else
                {
                    float dist = (npc.gameObject.transform.position - agent.transform.position).magnitude;
                    if (dist < closestDist)
                    {
                        closest = npc;
                        closestDist = dist;
                    }
                }
            }

            if (closest == null)
            {
                return false;
            }

            TargetNpcHeroAttribute = closest;
            target = TargetNpcHeroAttribute.gameObject;

            return closest != null;
        }

        public override bool perform(GameObject agent)
        {
            if (TargetNpcHeroAttribute != null)
            {
                NpcIsDestroyed = true;
            }
            return NpcIsDestroyed;
        }    
    }
}