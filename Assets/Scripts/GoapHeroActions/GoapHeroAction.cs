using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroAction : GoapAction
    {
        public static GoapHeroAction Instance;
        public static List<NpcAttributesComponent> NpcTargetAttributes;
        protected float MoveSpeed = 2;
        protected float DistanceToTargetThreshold = 1f;
        protected float InRangeToTargetThreshold = 5f;
        protected float PoseThreshold = 10f;
        protected bool NpcIsDestroyed;
        protected bool HasCrossedSword;
        protected bool NpcIsDestroyedReset;
        protected bool HasResetPosition;
        public NpcAttributesComponent TargetNpcAttribute;
        protected bool IsPerforming;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            NpcTargetAttributes = new List<NpcAttributesComponent>();
        }

        public override bool Move()
        {
            var distanceFromTarget = DistanceFromTarget();

            if (Hero.Instance.IsAnimationTagPlaying("attack"))
            {
                return false;
            }

            if (
                !Hero.Instance.IsFrozenPosition() &&
                !Hero.Instance.IsAnimationTagPlaying("attack") &&
                !IsPerforming ||
                distanceFromTarget <= InRangeToTargetThreshold
                )
            {

                if (distanceFromTarget >= DistanceToTargetThreshold)
                {
                    Hero.Instance.FaceTarget(target);
                    var step = (MoveSpeed * 2) * Time.deltaTime;
                    gameObject.transform.position =
                        Vector3.MoveTowards(gameObject.transform.position, new Vector3(target.transform.position.x, 0), step);
                    Hero.Instance.NpcHeroAnimator.SetBool("heroRun", true);
                }
                else
                {
                    Hero.Instance.NpcHeroAnimator.SetBool("heroRun", false);
                    setInRange(true);
                    return true;
                }
            }
            return false;
        }

        protected float DistanceFromTarget()
        {
            return Vector2.Distance(new Vector2(gameObject.transform.position.x, 0), new Vector2(target.transform.position.x, 0));
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
            return FindNpcTargets(agent);
        }

        public bool FindNpcTargets(GameObject agent)
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
            if (target != null)
            {
                return true;
            }

            Debug.LogWarning("=====FindLastNpcDashTarget======");
            var furthest = NpcTargetAttributes
                .Where((n) => n.Health > 0)
                .OrderBy(n => n.transform.position.x);

            TargetNpcAttribute = Hero.Instance.HeroFlipX ?
                furthest.LastOrDefault() :
                furthest.FirstOrDefault();
            if (TargetNpcAttribute != null) target = TargetNpcAttribute.gameObject;
            return true;
        }

        public bool FindFirstTarget(GameObject agent)
        {
            if (TargetNpcAttribute != null)
            {
                target = TargetNpcAttribute.gameObject;
                return true;
            }

            TargetNpcAttribute = NpcTargetAttributes
                .Where((n) => n.Health > 0)
                .OrderBy(n => Vector2.Distance(n.transform.position, Hero.Instance.transform.position))
                .FirstOrDefault();

            if (TargetNpcAttribute != null)
            {
                target = TargetNpcAttribute.gameObject;
                ClearAllTargetsFromList();
                return true;
            }
            return false;
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
            return Mathf.Abs(gameObject.transform.position.x) < Hero.ResetPositionThreshold;
        }
    }
}