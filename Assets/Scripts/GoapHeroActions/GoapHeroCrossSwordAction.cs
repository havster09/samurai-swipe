using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroCrossSwordAction : GoapHeroAction
    {
        public GoapHeroCrossSwordAction()
        {
            addPrecondition("crossSword", false);
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
            return FindNpcTarget(agent);
        }

        public override bool perform(GameObject agent)
        {
            var forward = Random.Range(0, 10) < 3;
            IsPerforming = true;
            var enemyScript = target.GetComponent<Enemy>();

            if (!HeroScript.NpcHeroAnimator.GetBool("heroCrossSword") && Mathf.Abs(TotalMovementDistance) < 10)
            {
                HeroScript.CrossSword(true);
                enemyScript.CrossSword(true);

                var targetCrossSwordPosition = enemyScript.EnemyFlipX ? DistanceToTargetThreshold : -DistanceToTargetThreshold;
                target.gameObject.transform.position = new Vector2(
                    gameObject.transform.position.x + targetCrossSwordPosition,
                    0
                    );
            }

            if (Mathf.Abs(TotalMovementDistance) > 10)
            {
                HeroScript.CrossSword(false);
                enemyScript.CrossSword(false);
                IsPerforming = false;
                HasCrossedSword = true;
            }
            else
            {
                SetCrossSwordMovement(enemyScript, HeroScript, forward);
            }
            return HasCrossedSword;
        }

        private void SetCrossSwordMovement(Enemy enemyScript, Hero heroScript, bool forward)
        {
            Vector2 end;
            if (forward)
            {
                TotalMovementDistance--;
                end = new Vector2(transform.position.x, 0) - new Vector2(TotalMovementDistance, 0);
            }
            else
            {
                TotalMovementDistance++;
                end = new Vector2(transform.position.x, 0) + new Vector2(TotalMovementDistance, 0);
            }
            var speed = Random.Range(0, 2);
            StartCoroutine(enemyScript.PerformMovementTo(end, speed, TargetNpcAttribute));
            StartCoroutine(heroScript.PerformMovementTo(end, speed, TargetNpcAttribute));
        }
    }
}

