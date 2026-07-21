using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroSingleAttackAction : GoapHeroAction
    {
        public GoapHeroSingleAttackAction()
        {
            addEffect("destroyEnemyNpcSingle", true);
            DistanceToTargetThreshold = .6f;
        }

        public override void reset()
        {
            NpcIsDestroyed = false;
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
            // return false; // todo add logic for which attack is used in goap planner (use SlashRenderer and NpcHeroAttributes)
            return FindSingleTarget(agent) &&
                   Hero.Instance.NpcHeroAnimator.GetFloat("heroDashAttack") < .1f;
        }

        public override bool perform(GameObject agent)
        {
            var enemyScript = target.GetComponent<Enemy>();
            if (enemyScript.MoveEnemyCoroutine != null)
            {
                enemyScript.StopCoroutine(enemyScript.MoveEnemyCoroutine);
            }

            // Only fire the next attack when not already mid-animation
            if (
                !Hero.Instance.IsAnimationTagPlaying("attack") &&
                !Hero.Instance.IsAnimationTagPlaying("cross") &&
                !Hero.Instance.IsAnimationTagPlaying("rest") &&
                !enemyScript.IsDead
                )
            {
                var damage = Hero.Instance.GetAttackTypeAndDamage(target);

                if (TargetNpcAttribute.DefendCount < 1)
                {
                    enemyScript.EnemyHitSuccess(damage);
                }
                else
                {
                    enemyScript.EnemyHitFail();
                    NpcHeroAttributesComponent.Instance.Rage -= 1;
                }

                NpcHeroAttributesComponent.Instance.AttackCount += 1;
            }

            if (enemyScript.IsDead)
            {
                NpcHeroAttributesComponent.Instance.KillCount += 1;
                NpcHeroAttributesComponent.Instance.ComboCount += 1;
                NpcHeroAttributesComponent.Instance.Rage += 1;
                NpcHeroAttributesComponent.Instance.Brave += 1;
                NpcIsDestroyed = true;
                HeroSwipeController.ClearQueue();
                ResetSingleAttack();
            }

            // Return true = "still performing successfully" so GOAP does NOT abort the plan.
            // isDone() returning true (NpcIsDestroyed) signals completion to GOAP.
            return true;
        }

        public void ResetSingleAttack()
        {
            TargetNpcAttribute = null;
            NpcTargetAttributes.Clear();
        }
    }
}

