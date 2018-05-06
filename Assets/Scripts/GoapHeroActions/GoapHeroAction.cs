﻿using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroAction : GoapAction
    {
        public static List<NpcAttributesComponent> NpcTargetAttributes;
        protected float MoveSpeed = 2;
        protected const float ResetPositionThreshold = 1f;
        protected float DistanceToTargetThreshold = 1f;
        protected float InRangeToTargetThreshold = 5f;
        protected float PoseThreshold = 10f;
        protected bool NpcIsDestroyed;
        protected bool HasCrossedSword;
        protected bool NpcIsDestroyedReset;
        protected bool HasResetPosition;
        protected Hero HeroScript;
        protected NpcAttributesComponent TargetNpcAttribute;
        protected NpcHeroAttributesComponent NpcHeroAttributes;
        protected SlashRenderer SlashRendererScript;

        protected static bool IsPerforming;

        public GoapHeroAction() { }

        void Awake()
        {
            HeroScript = GetComponent<Hero>();
            NpcHeroAttributes = GetComponent<NpcHeroAttributesComponent>();
            NpcTargetAttributes = new List<NpcAttributesComponent>();
            SlashRendererScript = FindObjectOfType<SlashRenderer>();
        }

        public override bool Move()
        {
            if (!HeroScript.IsFrozenPosition() && !HeroScript.IsAnimationTagPlaying("attack") && !IsPerforming)
            {
                float distanceFromTarget = Vector2.Distance(new Vector2(gameObject.transform.position.x, 0), new Vector2(target.transform.position.x, 0));
                if (distanceFromTarget >= DistanceToTargetThreshold && distanceFromTarget <= InRangeToTargetThreshold)
                {
                    HeroScript.FaceTarget(target);
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

        public bool FindNpcTarget(GameObject agent)
        {
            NpcAttributesComponent closest = null;
            float closestDist = 5f;

            foreach (var npc in NpcTargetAttributes)
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

            return true;
        }

        public bool FindLastNpcDashTarget(GameObject agent)
        {
            NpcAttributesComponent furthest = null;

            furthest = NpcTargetAttributes.OrderBy(n => n.transform.position.x).LastOrDefault();

            if (furthest == null)
            {
                return false;
            }

            TargetNpcAttribute = furthest;
            target = TargetNpcAttribute.gameObject;
            return true;
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

        protected bool InResetRange()
        {
            return Mathf.Abs(gameObject.transform.position.x) < ResetPositionThreshold;
        }
    }
}