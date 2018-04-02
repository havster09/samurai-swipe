using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroBaseAttackAction : GoapHeroAction
    {
        public GoapHeroBaseAttackAction()
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
            return FindNpcTarget(agent);
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
                var heroAttacks = new List<string>();
                if (NpcHeroAttributes.Rage > 2)
                {

                    heroAttacks.AddRange(new List<string>
                    {
                        "heroDoubleSlashMid",
                        "heroDoubleSlashHigh",
                        "heroDoubleSlashLow",
                    });
                }
                else
                {
                    heroAttacks.AddRange(new List<string>
                    {
                        "heroAttackOne",
                        "heroAttackTwo",
                        "heroAttackThree",
                        "heroAttackFour",
                        "heroAttackSix",
                        "heroAttackSeven"
                    });
                }
                
                HeroScript.Attack(heroAttacks[Random.Range(0, heroAttacks.Count)], TargetNpcAttribute);
                TargetNpcAttribute.Health -= 100;
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
            Debug.Log(string.Format("<color=green>Active Targets {0}</color>", NpcTargetAttributes.Count));
            return NpcIsDestroyed;
        }
    }
}

