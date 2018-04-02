using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroAction : GoapAction
    {
        protected List<NpcAttributesComponent> NpcTargetAttributes;
        protected float MoveSpeed = 2;
        protected float DistanceToTargetThreshold = 1;
        protected float InRangeToTargetThreshold = 5f;
        protected bool NpcIsDestroyed;
        protected bool NpcIsDestroyedReset;
        protected Hero HeroScript;
        protected NpcAttributesComponent TargetNpcAttribute;
        protected NpcHeroAttributesComponent NpcHeroAttributes;

        protected bool IsPerforming { get; set; }

        public GoapHeroAction() { }

        void Awake()
        {
            HeroScript = GetComponent<Hero>();
            NpcHeroAttributes = GetComponent<NpcHeroAttributesComponent>();
            NpcTargetAttributes = new List<NpcAttributesComponent>();
        }

        public override bool Move()
        {
            if (!HeroScript.IsFrozenPosition() && !HeroScript.IsAnimationPlaying("attack"))
            {
                HeroScript.FaceTarget(target);
                float distanceFromTarget = Vector2.Distance(new Vector2(gameObject.transform.position.x, 0), new Vector2(target.transform.position.x, 0));
                if (distanceFromTarget >= DistanceToTargetThreshold && distanceFromTarget <= InRangeToTargetThreshold)
                {
                    float step = (MoveSpeed * 2) * Time.deltaTime;
                    gameObject.transform.position = 
                        Vector3.MoveTowards(gameObject.transform.position, new Vector3(target.transform.position.x, 0), step);
                    HeroScript.NpcHeroAnimator.SetBool("heroRun", true);
                }
                else
                {
                    HeroScript.NpcHeroAnimator.SetBool("heroRun", false);
                    setInRange(true);
                    return true;
                }
            }
            return false;
        }

        public override void reset()
        {
            NpcIsDestroyed = false;
            TargetNpcAttribute = null;
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
            NpcAttributesComponent closest = null;
            float closestDist = 5f;

            foreach (NpcAttributesComponent npc in NpcTargetAttributes)
            {
                float dist = (npc.gameObject.transform.position - agent.transform.position).magnitude;
                if (dist < closestDist && npc.Health > 0)
                {
                    closest = npc;
                    closestDist = dist;
                }
            }

            if (closest == null) 
            {
                return false;
            }

            TargetNpcAttribute = closest;
            target = TargetNpcAttribute.gameObject;

            return closest != null;
        }

        public override bool perform(GameObject agent)
        {
            if (TargetNpcAttribute != null)
            {
                NpcIsDestroyed = true;
            }
            return NpcIsDestroyed;
        }

        public void AddTargetToList(NpcAttributesComponent npcAttribute)
        {
            if (!NpcTargetAttributes.Contains(npcAttribute))
            {
                NpcTargetAttributes.Add(npcAttribute);
            }
        }

        public void ClearAllTargetsFromList()
        {
            NpcTargetAttributes.Clear();
        }

        public void RemoveTargetFromList(NpcAttributesComponent npcAttribute)
        {
            NpcTargetAttributes.Remove(npcAttribute);
        }
    }
}