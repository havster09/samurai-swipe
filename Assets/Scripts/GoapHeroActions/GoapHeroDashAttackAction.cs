using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroDashAttackAction : GoapHeroBaseAttackAction
    {
        public static RaycastHit2D[] Hits;
        private const float DashOffset = 1f;

        public GoapHeroDashAttackAction()
        {
            addEffect("destroyEnemyNpc", true);
            DistanceToTargetThreshold = 1f;
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
            return false;
            if (NpcTargetAttributes.Count < 1)
            {
                return false;
            }

            if (target != null || IsPerforming) return true;
            return FindLastNpcDashTarget(agent);
        }

        public override bool perform(GameObject agent)
        {
            if (
                Hero.Instance.NpcHeroAnimator.GetFloat("heroDashAttack") < .1f &&
                !Hero.Instance.NpcHeroAnimator.GetBool("heroBlock") &&
                !Hero.Instance.IsCoroutineMoving &&
                !IsPerforming
                )
            {
                IsPerforming = true;
                BoxCollider2D boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
                var dashEnd = target.transform.position + new Vector3(Hero.Instance.HeroFlipX ? -DashOffset : DashOffset, 0, 0);
                var dashEndPosition = Hero.Instance.HeroFlipX
                    ? Vector3.Max(dashEnd, new Vector3(-3.5f, 0, 0))
                    : Vector3.Min(dashEnd, new Vector3(3.5f, 0, 0));
                var dashRaycastEndPosition = dashEndPosition + new Vector3(Hero.Instance.HeroFlipX ? -DashOffset : DashOffset, 0, 0);
                var startPosition = new Vector2(transform.position.x - (Hero.Instance.HeroFlipX ? DashOffset : DashOffset), boxCollider2D.offset.y);
                Hits = Physics2D.RaycastAll(
                    startPosition,
                    Hero.Instance.HeroFlipX ? Vector2.left : Vector2.right,
                    Vector2.Distance(startPosition, dashRaycastEndPosition),
                    512);

                Debug.DrawLine(startPosition, new Vector3(0, boxCollider2D.offset.y, 0) + dashRaycastEndPosition, Color.green, 5f);
                
                Hero.Instance.Dash(dashEndPosition, 6f, Hits);
                // Debug.Log(string.Format("<color=green>Active Targets {0}</color>", NpcTargetAttributes.Count));
            }
            return NpcIsDestroyed;
        }

        public void ResetDashAttack()
        {
            NpcIsDestroyed = true;
            IsPerforming = false;
            setInRange(false);
        }
    }
}

