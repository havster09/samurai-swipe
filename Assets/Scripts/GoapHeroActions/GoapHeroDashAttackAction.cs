using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroDashAttackAction : GoapHeroBaseAttackAction
    {
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
                !HeroScript.IsCoroutineMoving &&
                !IsPerforming
                )
            {
                IsPerforming = true;
                HeroScript.FaceTarget(target);
                var dashEnd = target.transform.position + new Vector3(HeroScript._heroFlipX ? -1f : 1f, 0, 0);
                var dashEndPosition = HeroScript._heroFlipX
                    ? Vector3.Max(dashEnd, new Vector3(-3.5f, 0, 0))
                    : Vector3.Min(dashEnd, new Vector3(3.5f, 0, 0));
                HeroScript.Dash(dashEndPosition, 6f, () =>
                {
                    HeroScript.WaitFor(DamageTargets, .2f);
                });
                Debug.Log(string.Format("<color=green>Active Targets {0}</color>", NpcTargetAttributes.Count));
            }
            return NpcIsDestroyed;
        }

        private void DamageTargets()
        {

            NpcTargetAttributes.ForEach(n =>
            {
                var slashTarget = n.gameObject;
                var enemyScript = slashTarget.GetComponent<Enemy>();
                var damage = 100;
                enemyScript.EnemyHitSuccess(damage);
            });
            NpcTargetAttributes.Clear();
            var slashRenderer = FindObjectOfType<SlashRenderer>();
            HeroScript.WaitFor(() =>
            {
                HeroScript.NpcHeroAnimator.Play("genjuroIdle");
                NpcIsDestroyed = true;
                IsPerforming = false;
            }, 3f);
        }
    }
}

