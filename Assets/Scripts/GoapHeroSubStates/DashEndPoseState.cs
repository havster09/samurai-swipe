using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

namespace Assets.Scripts.GoapHeroSubStates
{
    class DashEndPoseState : MonoBehaviour, IState
    {
        private Hero _heroScript;
        private Animator _npcHeroAnimator;
        public void BeginEnter()
        {
            _heroScript = FindObjectOfType<Hero>();
            _npcHeroAnimator = _heroScript.GetComponent<Animator>();
            Debug.LogWarning("====BeginEnter DashEndPoseState====");
        }

        public void EndEnter()
        {
            Debug.LogWarning("====EndEnter DashEndPoseState====");
        }

        public IEnumerable Execute()
        {
            var threshold = 5f;
            var heroGameObject = FindObjectOfType<Hero>();
            var directionRight = heroGameObject.transform.position.x < 0;
            while (true)
            {
                if (_heroScript.IsAnimationTagPlaying("dashEnd"))
                {
                    var totalNpc = FindObjectsOfType<NpcAttributesComponent>()
                        .Where(npc => npc.Health > 0 &&
                                      npc.isActiveAndEnabled &&
                                      Vector2.Distance(npc.transform.position, heroGameObject.transform.position) < threshold &&
                                      (
                                          npc.transform.position.x > heroGameObject.transform.position.x && directionRight ||
                                          npc.transform.position.x < heroGameObject.transform.position.x && !directionRight
                                      )
                        )
                        .ToArray();
                    Debug.LogWarning("====Execute DashEndPoseState====");
                    if (totalNpc.Length < 1)
                    {
                        var nextState = new DashEndIdleState();
                        var transition = new SimpleDelayTransition();
                        var eventArgs = new StateBeginExitEventArgs(nextState, transition);
                        OnBeginExit(this, eventArgs);
                    }
                }
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
