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
            DistanceToTargetThreshold = 2f;
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

            if (target != null) return target;
            return FindLastNpcDashTarget(agent);
        }

        public override bool perform(GameObject agent)
        {
            if (
                HeroScript.NpcHeroAnimator.GetFloat("heroDashAttack") < .1f &&
                !HeroScript.NpcHeroAnimator.GetBool("heroBlock") &&
                !HeroScript.IsCoroutineMoving &&
                !IsPerforming
                )
            {
                IsPerforming = true;
                HeroScript.FaceTarget(target);
                BoxCollider2D boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
                var dashEnd = target.transform.position + new Vector3(HeroScript._heroFlipX ? -1f : DashOffset, 0, 0);
                var dashEndPosition = HeroScript._heroFlipX
                    ? Vector3.Max(dashEnd, new Vector3(-3.5f, 0, 0))
                    : Vector3.Min(dashEnd, new Vector3(3.5f, 0, 0));
                var dashRaycastEndPosition = dashEndPosition + new Vector3(HeroScript._heroFlipX ? -1f : 1f, 0, 0);
                var startPosition = new Vector2(transform.position.x, boxCollider2D.offset.y);
                RaycastHit2D[] hits = Physics2D.RaycastAll(startPosition, HeroScript._heroFlipX ? Vector2.left : Vector2.right, Vector2.Distance(startPosition, dashRaycastEndPosition));
                Debug.DrawLine(startPosition, new Vector3(0, boxCollider2D.offset.y, 0) + dashRaycastEndPosition, Color.green, 5f);
                if (hits.Length > 0)
                {
                    Debug.LogWarning(hits);
                }

                HeroScript.Dash(dashEndPosition, 6f, () =>
                {
                    HeroScript.WaitFor(() => DamageTargets(hits), .2f);
                });
                Debug.Log(string.Format("<color=green>Active Targets {0}</color>", NpcTargetAttributes.Count));
            }
            return NpcIsDestroyed;
        }

        private void DamageTargets(RaycastHit2D[] hits)
        {

            hits.ToList().ForEach(h =>
            {
                var slashTarget = h.collider.gameObject;
                var enemyScript = slashTarget.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.EnemyHitSuccess(100);
                }
                
            });

            NpcTargetAttributes.Clear();
            // todo trigger reset pos action if needed

            HeroScript.WaitFor(() =>
            {
                HeroScript.NpcHeroAnimator.Play("genjuroIdle");
                NpcIsDestroyed = true;
                IsPerforming = false;
            }, 3f);
        }
    }
}

