using Assets.Scripts.GoapHeroActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.GoapHeroSubStates
{
    class DashEndPoseState : IState
    {
        private GoapHeroAction _goapHeroActionScript;
        public void BeginEnter()
        {
            _goapHeroActionScript = Object.FindObjectOfType<GoapHeroAction>();
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
                    var totalNpc = Object.FindObjectsOfType<Enemy>();
                    var totalNpcWithAlpha = totalNpc.ToList().Where((n) =>
                    {
                        var spriteRenderer = n.GetComponent<SpriteRenderer>();
                        return spriteRenderer.color.a >= 1f;
                    }).ToList();

                    if (totalNpcWithAlpha.Count < 1)
                    {
                        InvokeTransition();
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        public event EventHandler<StateBeginExitEventArgs> OnBeginExit;
        public void EndExit()
        {
            
        }

        private void InvokeTransition()
        {
            var nextState = new DashEndIdleState();
            var transition = new SimpleDelayTransition();
            var eventArgs = new StateBeginExitEventArgs(nextState, transition);
            OnBeginExit(this, eventArgs);
        }
    }
}
