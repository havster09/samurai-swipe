﻿using Assets.Scripts.GoapHeroActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.GameObject;

namespace Assets.Scripts.GoapHeroSubStates
{
    class DashEndPoseState : IState
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
                    var totalNpc = GoapHeroAction.NpcTargetAttributes;
                    var totalNpcWithAlpha = totalNpc.Where((n) =>
                    {
                        var spriteRenderer = n.GetComponent<SpriteRenderer>();
                        return spriteRenderer.color.a >= 1f;
                    }).ToList();

                    if (totalNpcWithAlpha.Count < 1)
                    {
                        InvokeIdleTransition();
                        UnityEngine.Object.FindObjectOfType<GoapHeroDashAttackAction>().ResetDashAttack();
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }

        public event EventHandler<StateBeginExitEventArgs> OnBeginExit;
        public void EndExit()
        {

        }

        private void InvokeIdleTransition()
        {
            var nextState = new DashEndIdleState();
            var transition = new SimpleDelayTransition();
            var eventArgs = new StateBeginExitEventArgs(nextState, transition);
            OnBeginExit(this, eventArgs);
        }
    }
}
