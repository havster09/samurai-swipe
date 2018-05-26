﻿using System;
using System.Collections.Generic;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroBaseAttackAction : GoapHeroAction
    {
        public GoapHeroBaseAttackAction()
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
            return FindNpcTarget(agent) &&
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
                var heroAttacks = new List<string>();
                var damage = 0;

                if (target.transform.position.y > 0)
                {
                    heroAttacks.AddRange(new List<string>
                    {
                        "heroAttackFour", // slash high
                        "heroAttackSix", // crouch slash high
                    });
                    damage = 100;
                }
                else if (NpcHeroAttributesComponent.Instance.Rage > 10)
                {

                    heroAttacks.AddRange(new List<string>
                    {
                        "heroDoubleSlashMid",
                        "heroDoubleSlashHigh",
                        "heroDoubleSlashLow",
                    });
                    damage = 50;
                }
                else
                {
                    heroAttacks.AddRange(new List<string>
                    {
                        "heroAttackOne", // slash down
                        "heroAttackSeven" // step strong slash
                    });
                    damage = 100;
                }

                Hero.Instance.Attack(heroAttacks[Random.Range(0, heroAttacks.Count)]);

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
            }

            if (NpcTargetAttributes.Count < 1)
            {
                NpcIsDestroyed = true;
            }
            // Debug.Log(string.Format("<color=green>Active Targets {0}</color>", NpcTargetAttributes.Count));
            return NpcIsDestroyed;
        }
    }
}

