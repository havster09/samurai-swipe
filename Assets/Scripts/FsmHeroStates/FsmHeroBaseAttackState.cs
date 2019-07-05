using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using Assets.Scripts.GoapHeroSubStates;
using UnityEngine;

namespace Assets.Scripts.FsmHeroStates
{
    class FsmHeroBaseAttackState : IState
    {
        public void BeginEnter()
        {

        }

        public void EndEnter()
        {

        }

        public IEnumerable Execute()
        {
            while (true)
            {
                if (Hero.Instance.IsAnimationTagPlaying("dashEnd"))
                {
                    Hero.Instance.IsInPoseState = true;
                    var slashCollider = GameObject.FindGameObjectWithTag("SlashCollider");
                    if (slashCollider != null)
                    {
                        ResetDashAttack();
                    }

                    var totalNpcAttributeHits = GoapHeroDashAttackAction.Hits
                        .Select(h => h.collider.GetComponent<NpcAttributesComponent>())
                        .ToList();

                    var totalNpcWithAlpha = totalNpcAttributeHits.Where((n) =>
                    {
                        var spriteRenderer = n.GetComponent<SpriteRenderer>();
                        return spriteRenderer.color.a >= 1f;
                    }).ToList();

                    if (totalNpcWithAlpha.Count < 1)
                    {
                        ResetDashAttack();
                        GoapHeroAction.Instance.ClearAllTargetsFromList();
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }

        private void ResetDashAttack()
        {
            InvokeIdleTransition();
            UnityEngine.Object.FindObjectOfType<GoapHeroDashAttackAction>().ResetDashAttack();
        }

        public event EventHandler<StateBeginExitEventArgs> OnBeginExit;
        public void EndExit()
        {

        }

        private void InvokeIdleTransition()
        {
            var nextState = new DashEndIdleState();
            var transition = new FSMHeroBaseAttack();
            var eventArgs = new StateBeginExitEventArgs(nextState, transition);
            OnBeginExit(this, eventArgs);
        }
    }
}
