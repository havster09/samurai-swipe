using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroDashAttackAction : GoapHeroBaseAttackAction
    {
        public GoapHeroDashAttackAction()
        {
            addEffect("destroyEnemyNpc", true);
            DistanceToTargetThreshold = .6f;
        }

        public override void reset()
        {
            if (Random.Range(0, 10) < 5)
            {
                addPrecondition("crossSword", true);
            }

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
            return FindNpcTarget(agent); // todo replace and get multiple targets
        }

        public override bool perform(GameObject agent)
        {
            if (NpcTargetAttributes.Count > 2)
            {
                HeroScript.NpcHeroAnimator.SetFloat("heroDashAttack", 1);
            }
            var enemyScript = target.GetComponent<Enemy>();

            if (enemyScript.MoveEnemyCoroutine != null)
            {
                enemyScript.StopCoroutine(enemyScript.MoveEnemyCoroutine);
            }

            if (
                !HeroScript.IsAnimationTagPlaying("attack") &&
                !HeroScript.IsAnimationTagPlaying("cross") &&
                !HeroScript.IsAnimationTagPlaying("rest") &&
                !enemyScript.IsDead
                )
            {
                var heroAttacks = new List<string>();
                var damage = 0;
            }

            if (enemyScript.IsDead)
            {
                NpcHeroAttributes.KillCount += 1;
                NpcHeroAttributes.ComboCount += 1;
                NpcTargetAttributes.Remove(TargetNpcAttribute);
                NpcHeroAttributes.Rage += 5;
            }

            if (NpcTargetAttributes.Count < 1)
            {
                NpcIsDestroyed = true;
                NpcTargetAttributes.Clear();
            }
            Debug.Log(string.Format("<color=green>Active Targets {0}</color>", NpcTargetAttributes.Count));
            return NpcIsDestroyed;
        }
    }
}

