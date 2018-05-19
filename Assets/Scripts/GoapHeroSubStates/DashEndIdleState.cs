using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

namespace Assets.Scripts.GoapHeroSubStates
{
    class DashEndIdleState : MonoBehaviour, IState
    {
        private Hero _heroScript;
        private Animator _npcHeroAnimator;
        
        public void BeginEnter()
        {
            _heroScript = FindObjectOfType<Hero>();
            _npcHeroAnimator = _heroScript.GetComponent<Animator>();
            _npcHeroAnimator.Play("heroIdle");
            Debug.LogWarning("====BeginEnter DashEndPoseState====");
        }

        public void EndEnter()
        {
            Debug.LogWarning("====EndEnter DashEndPoseState====");
        }

        public IEnumerable Execute()
        {
            while (true)
            {
                Debug.LogWarning("====Execute DashEndPoseState====");
                // todo refactor to reset position
                var nextState = new DashEndPoseState();
                var transition = new SimpleDelayTransition();
                var eventArgs = new StateBeginExitEventArgs(nextState, transition);
                OnBeginExit(this, eventArgs);

                yield return new WaitForSeconds(2f);
            }
        }

        public event EventHandler<StateBeginExitEventArgs> OnBeginExit;
        public void EndExit()
        {
            Debug.LogWarning("====EndExit DashEndPoseState====");
        }
    }
}
