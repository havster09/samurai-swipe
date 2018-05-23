using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.GoapHeroSubStates
{
    class DashEndIdleState : IState
    {
        private Animator _npcHeroAnimator;
        
        public void BeginEnter()
        {
            
            _npcHeroAnimator = Hero.Instance.GetComponent<Animator>();
            _npcHeroAnimator.Play("heroIdle");
        }

        public void EndEnter()
        {
            
        }

        public IEnumerable Execute()
        {
            while (true)
            {
                InvokeTransition();
                Debug.Log("===Executing DashEndIdleState====");
                yield return new WaitForSeconds(2f);
            }
        }

        public event EventHandler<StateBeginExitEventArgs> OnBeginExit;
        public void EndExit()
        {
            
        }

        private void InvokeTransition()
        {
            var nextState = new DashEndPoseState();
            var transition = new SimpleDelayTransition();
            var eventArgs = new StateBeginExitEventArgs(nextState, transition);
            OnBeginExit(this, eventArgs);
        }
    }
}
