using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroCrossSwordAction : GoapHeroAction
    {
        public Enemy EnemyScript { get; set; }

        public GoapHeroCrossSwordAction()
        {
            addEffect("crossSword", true);
            DistanceToTargetThreshold = .6f;
            TotalMovementDistance = 0;
        }

        public override void DoReset()
        {
            if (!IsPerforming)
            {
                target = null;
                TargetNpcAttribute = null;
                if (EnemyScript != null && EnemyScript.IsDead)
                {
                    TotalMovementDistance = 0;
                }
            }
            InRange = false;
            reset();
        }

        public override void reset()
        {
            HasCrossedSword = false;
        }

        public override bool isDone()
        {
            return HasCrossedSword;
        }

        public override bool requiresInRange()
        {
            if (IsPerforming)
            {
                return false;
            }
            return true;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            if (IsPerforming && target != null) return true;
            return FindNpcTarget(agent);
        }

        public override bool perform(GameObject agent)
        {
            if (
                TargetNpcAttribute.CrossSwordMaxMovementDistance < 1 ||
                GetActiveNpcAttributesComponentsInRange(gameObject, PoseThreshold) > 2
                )
            {
                HasCrossedSword = true;
                return HasCrossedSword;
            }

            EnemyScript = target.GetComponent<Enemy>();
            if (EnemyScript.MoveEnemyCoroutine != null)
            {
                EnemyScript.StopCoroutine(EnemyScript.MoveEnemyCoroutine);
            }

            if (
                !HeroScript.NpcHeroAnimator.GetBool("heroCrossSword") &&
                Mathf.Abs(TotalMovementDistance) < TargetNpcAttribute.CrossSwordMaxMovementDistance
                )
            {
                HeroScript.CrossSword(true);
                EnemyScript.CrossSword(true);
                IsPerforming = true;
                var targetCrossSwordPosition = EnemyScript.EnemyFlipX ? DistanceToTargetThreshold : -DistanceToTargetThreshold;
                target.gameObject.transform.position = new Vector2(
                    gameObject.transform.position.x + targetCrossSwordPosition,
                    0
                    );
            }

            if (Mathf.Abs(TotalMovementDistance) > TargetNpcAttribute.CrossSwordMaxMovementDistance)
            {
                HeroScript.CrossSword(false);
                EnemyScript.CrossSword(false);
                IsPerforming = false;
                HasCrossedSword = true;
                HeroScript.Rb2D.velocity = new Vector2(0, 0);
                EnemyScript.Rb2D.velocity = new Vector2(0, 0);
                HasCrossedSword = true;
                return HasCrossedSword;
            }
            else
            {
                if (IsPerforming)
                {
                    SetCrossSwordMovement(EnemyScript, HeroScript);
                }
            }
            return HasCrossedSword;
        }

        private void SetCrossSwordMovement(MovingObject enemyScript, MovingObject heroScript)
        {
            TotalMovementDistance++;
            var velocity = Random.Range(-2, 2);
            heroScript.Rb2D.velocity = new Vector2(velocity, 0);
            enemyScript.Rb2D.velocity = new Vector2(velocity, 0);
        }
    }
}

