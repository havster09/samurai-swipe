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
            // Guard: all GoapHeroAction subclasses are components on the same hero
            // GameObject. Without this check every subclass Awake() would wipe the
            // shared list, losing any targets added by earlier-running siblings.
            if (NpcTargetAttributes == null)
                NpcTargetAttributes = new List<NpcAttributesComponent>();
        }

        public override bool Move()
        {
            // Yield movement control to player-driven walk or any blocking state
            if (HeroSwipeController.IsPlayerWalking)   return false;
            if (Hero.Instance.IsBlockingMovement())    return false;
            if (Hero.Instance.IsFrozenPosition())      return false;

            var distanceFromTarget = DistanceFromTarget();

            if (distanceFromTarget >= DistanceToTargetThreshold)
            {
                Hero.Instance.FaceTarget(target);
                var step = (MoveSpeed * 2) * Time.deltaTime;
                gameObject.transform.position =
                    Vector3.MoveTowards(gameObject.transform.position,
                        new Vector3(target.transform.position.x, 0), step);
                Hero.Instance.NpcHeroAnimator.SetBool("heroRun", true);
            }
            else
            {
                Hero.Instance.NpcHeroAnimator.SetBool("heroRun", false);
                setInRange(true);
                return true;
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

            // Debug.LogWarning("=====FindLastNpcDashTarget======");
            var furthest = NpcTargetAttributes
                .Where((n) => n.Health > 0)
                .OrderBy(n => n.transform.position.x);

            TargetNpcAttribute = Hero.Instance.HeroFlipX ?
                furthest.LastOrDefault() :
                furthest.FirstOrDefault();
            if (TargetNpcAttribute != null) target = TargetNpcAttribute.gameObject;
            return true;
        }

        public bool FindSingleTarget(GameObject agent)
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
                Debug.Log(string.Format("[GOAP] TARGET ADD  {0}  (list:{1})",
                    npcAttribute.gameObject.name, NpcTargetAttributes.Count));
            }
        }

        public void ClearAllTargetsFromList()
        {
            Debug.Log(string.Format("[GOAP] TARGET CLEAR  all {0} targets", NpcTargetAttributes.Count));
            NpcTargetAttributes.Clear();
            SlashRenderer.Instance.RemoveSlashCollider();
        }

        public void RemoveTargetFromList(NpcAttributesComponent npcAttribute)
        {
            Debug.Log(string.Format("[GOAP] TARGET REMOVE  {0}  (list:{1})",
                npcAttribute != null ? npcAttribute.gameObject.name : "null",
                NpcTargetAttributes.Count - 1));
            NpcTargetAttributes.Remove(npcAttribute);
        }

        protected bool InResetRange()
        {
            return Mathf.Abs(gameObject.transform.position.x) < Hero.ResetPositionThreshold;
        }
    }
}