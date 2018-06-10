using System;
using System.Collections.Generic;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroOneAttackAction : GoapHeroAction
    {
        public GoapHeroOneAttackAction()
        {
            addEffect("destroyEnemyNpc", true);
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
            return FindFirstTarget(agent) &&
                   Hero.Instance.NpcHeroAnimator.GetFloat("heroDashAttack") < .1f;
        }

        public override bool perform(GameObject agent)
        {
            var enemyScript = target.GetComponent<Enemy>();
            if (enemyScript.MoveEnemyCoroutine != null)
            {
                enemyScript.StopCoroutine(enemyScript.MoveEnemyCoroutine);
            }

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
                NpcHeroAttributesComponent.Instance.Rage += 5;
                NpcIsDestroyed = true;
                ResetOneAttack();
            }

            return NpcIsDestroyed;
        }

        public void ResetOneAttack()
        {
            TargetNpcAttribute = null;
        }
    }
}

