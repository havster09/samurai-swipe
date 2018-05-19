using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.GoapHeroSubStates
{
    class SimpleDelayTransition : IStateTransition
    {
        public IEnumerable Exit()
        {
            return TransitionDelay(.2f).Cast<object>();
        }

        public IEnumerable Enter()
        {
            return TransitionDelay(.2f).Cast<object>();
        }

        private IEnumerable TransitionDelay(
            float duration
        )
        {
            var startTime = Time.time;
            var endTime = startTime + duration;
            while (Time.time < endTime)
            {
                yield return null;
            }
        }
    }
}
