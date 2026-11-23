using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using Assets.Scripts.GoapHeroSubStates;
using UnityEngine;

namespace Assets.Scripts.FsmHeroStates
{
    class FsmHeroIdleState : IState
    {
        public void BeginEnter()
        {
            // Debug.Log("FsmHeroIdleState begin enter");
        }

        public void EndEnter()
        {
            // Debug.Log("FsmHeroIdleState end enter");
        }

        public IEnumerable Execute()
        {
            while (true)
            {
                if (Hero.Instance.IsAnimationTagPlaying("idle"))
                {
                    // Debug.Log("FsmHeroIdleState Execute");
                }
                yield return new WaitForFixedUpdate();
            }
        }

#pragma warning disable 67   // interface-required event, never raised internally
        public event EventHandler<StateBeginExitEventArgs> OnBeginExit;
#pragma warning restore 67
        public void EndExit()
        {
            // Debug.Log("FsmHeroIdleState EndExit");
        }
    }
}
