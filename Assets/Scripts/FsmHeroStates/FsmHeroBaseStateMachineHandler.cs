using UnityEngine;

namespace Assets.Scripts.FsmHeroStates
{
    public class FsmHeroBaseStateMachineHandler : MonoBehaviour
    {
        StateMachine _stateMachine;
        public Coroutine FsmHeroIdleStateMachineHandlerCoroutine;
        void Awake()
        {
            var fsmHeroIdlePoseState = new FsmHeroIdleState();
            _stateMachine = new StateMachine(fsmHeroIdlePoseState);
        }

        public void StartFsmHeroIdleStateMachineHandler()
        {
            FsmHeroIdleStateMachineHandlerCoroutine = StartCoroutine(_stateMachine.Execute().GetEnumerator());
        }

        public void StopStartFsmHeroIdleStateMachineHandler()
        {
            StopCoroutine(FsmHeroIdleStateMachineHandlerCoroutine);
        }
    }
}