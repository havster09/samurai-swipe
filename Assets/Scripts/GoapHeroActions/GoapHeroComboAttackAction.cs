using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroComboAttackAction : GoapHeroAction
    {
        public GoapHeroComboAttackAction()
        {

            addEffect("destroyEnemyNpc", true);
            DistanceToTargetThreshold = 1f;
        }

        public override bool perform(GameObject agent)
        {
            Enemy enemyScript = target.GetComponent<Enemy>();
            if (
                !HeroScript.IsAnimationPlaying("attack") &&
                !HeroScript.IsAnimationPlaying("rest") &&
                !enemyScript.IsDead
                )
            {
                var heroAttacks = new string[]
                {
                    "heroDoubleSlashMid",
                    "heroDoubleSlashHigh",
                    "heroDoubleSlashLow",
                };
                HeroScript.Attack(heroAttacks[Random.Range(0, heroAttacks.Length)], TargetNpcAttribute);
                TargetNpcAttribute.Health -= 50;
                enemyScript.EnemyHitSuccess();
                NpcHeroAttributes.AttackCount += 1;
            }

            if (enemyScript.IsDead)
            {
                NpcHeroAttributes.KillCount += 1;
                NpcHeroAttributes.ComboCount += 1;
                NpcTargetAttributes.Remove(TargetNpcAttribute);
            }

            if (NpcTargetAttributes.Count < 1)
            {
                NpcIsDestroyed = true;
                NpcTargetAttributes.Clear();
            }
            Debug.Log(string.Format("<color=green>{0}</color>", NpcTargetAttributes.Count));
            return NpcIsDestroyed;
        }
    }
}

