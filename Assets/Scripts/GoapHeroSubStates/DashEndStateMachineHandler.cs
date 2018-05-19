using UnityEngine;

namespace Assets.Scripts.GoapHeroSubStates
{
    public class DashEndStateMachineHandler : MonoBehaviour
    {
        StateMachine _stateMachine;
        public Coroutine DashEndStateMachineHandlerCoroutine;
        void Awake()
        {
            var dashEndPoseState = new DashEndPoseState();
            _stateMachine = new StateMachine(dashEndPoseState);
        }

        public void StartDashEndStateMachineHandler()
        {
            DashEndStateMachineHandlerCoroutine = StartCoroutine(_stateMachine.Execute().GetEnumerator());
        }

        public void StopStartDashEndStateMachineHandler()
        {
            StopCoroutine(DashEndStateMachineHandlerCoroutine);
        }
    }
}