using System.Linq;
using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroDashAttackAction : GoapHeroBaseAttackAction
    {
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
                var dashEnd = target.transform.position + new Vector3(Hero.Instance.HeroFlipX ? -1f : DashOffset, 0, 0);
                var dashEndPosition = Hero.Instance.HeroFlipX
                    ? Vector3.Max(dashEnd, new Vector3(-3.5f, 0, 0))
                    : Vector3.Min(dashEnd, new Vector3(3.5f, 0, 0));
                var dashRaycastEndPosition = dashEndPosition + new Vector3(Hero.Instance.HeroFlipX ? -1f : 1f, 0, 0);
                var startPosition = new Vector2(transform.position.x, boxCollider2D.offset.y);
                RaycastHit2D[] hits = Physics2D.RaycastAll(startPosition, Hero.Instance.HeroFlipX ? Vector2.left : Vector2.right, Vector2.Distance(startPosition, dashRaycastEndPosition));
                // todo move contents of hits to NpcTargetAttributes and use it instead to remove targets via Enemy script
                Debug.DrawLine(startPosition, new Vector3(0, boxCollider2D.offset.y, 0) + dashRaycastEndPosition, Color.green, 5f);
                
                Hero.Instance.Dash(dashEndPosition, 6f, hits);
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

