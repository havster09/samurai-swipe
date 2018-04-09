using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroCrossSwordAction : GoapHeroAction
    {
        private int _maxMovementDistance = 20;

        public GoapHeroCrossSwordAction()
        {
            addEffect("crossSword", true);
            DistanceToTargetThreshold = .6f;
            TotalMovementDistance = 0;
        }

        public override void reset()
        {
            HasCrossedSword = false;
            TargetNpcAttribute = null;
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
            if (!target)
            {
                return FindNpcTarget(agent);
            }
            return target;
        }

        public override bool perform(GameObject agent)
        {
            IsPerforming = true;
            var enemyScript = target.GetComponent<Enemy>();

            if (enemyScript.MoveEnemyCoroutine != null)
            {
                enemyScript.StopCoroutine(enemyScript.MoveEnemyCoroutine);
            }

            if (!HeroScript.NpcHeroAnimator.GetBool("heroCrossSword") && Mathf.Abs(TotalMovementDistance) < _maxMovementDistance)
            {
                HeroScript.CrossSword(true);
                enemyScript.CrossSword(true);

                var targetCrossSwordPosition = enemyScript.EnemyFlipX ? DistanceToTargetThreshold : -DistanceToTargetThreshold;
                target.gameObject.transform.position = new Vector2(
                    gameObject.transform.position.x + targetCrossSwordPosition,
                    0
                    );
            }

            if (Mathf.Abs(TotalMovementDistance) > _maxMovementDistance)
            {
                HeroScript.CrossSword(false);
                enemyScript.CrossSword(false);
                IsPerforming = false;
                HasCrossedSword = true;
                HeroScript.Rb2D.velocity = new Vector2(0, 0);
                enemyScript.Rb2D.velocity = new Vector2(0, 0);
                HasCrossedSword = true;
                return HasCrossedSword;
            }
            else
            {
                if (IsPerforming)
                {
                    SetCrossSwordMovement(enemyScript, HeroScript);
                }
            }
            return HasCrossedSword;
        }

        private void SetCrossSwordMovement(MovingObject enemyScript, MovingObject heroScript)
        {
            TotalMovementDistance++;
            var velocity = Random.Range(-5, 5);
            heroScript.Rb2D.velocity = new Vector2(velocity, 0);
            enemyScript.Rb2D.velocity = new Vector2(velocity, 0);
        }
    }
}

