using Assets.Scripts.GoapHeroActions;
using System;
using System.Collections;
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
            var threshold = 5f;
            while (true)
            {
                if (Hero.Instance.IsAnimationTagPlaying("dashEnd"))
                {
                    var totalNpc =
                        _goapHeroActionScript.GetActiveNpcAttributesComponentsInRange(Hero.Instance.gameObject, threshold);
                    if (totalNpc < 1)
                    {
                        InvokeTransition();
                    }
                }
                yield return new WaitForSeconds(2f);
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
