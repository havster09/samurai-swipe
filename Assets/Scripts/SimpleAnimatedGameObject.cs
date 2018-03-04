using System.Collections;
using UnityEngine;

public class SimpleAnimatedGameObject : MonoBehaviour {
    private Animator animator;

    public float holdLastFrameInSeconds;

    protected void OnEnable()
    {
        animator = GetComponent<Animator>();
        AnimationClip simpleAnimatedGameObjectClip;
        AnimationEvent simpleAnimatedGameObjectEvent;
        simpleAnimatedGameObjectEvent = new AnimationEvent();
        simpleAnimatedGameObjectClip = animator.runtimeAnimatorController.animationClips[0];
        simpleAnimatedGameObjectEvent.time = simpleAnimatedGameObjectClip.length;
        simpleAnimatedGameObjectEvent.stringParameter = "end";
        simpleAnimatedGameObjectEvent.functionName = "SimpleAnimatedObjectEndEventHandler";
        simpleAnimatedGameObjectClip.AddEvent(simpleAnimatedGameObjectEvent);
    }

    private void SimpleAnimatedObjectEndEventHandler(string stringParameter)
    {
        Debug.Log(stringParameter);
        if (holdLastFrameInSeconds > 0)
        {
            StartCoroutine(HoldLastFrameInSeconds());
        }
        else
        {
            gameObject.SetActive(false);
        }
        
    }

    protected IEnumerator HoldLastFrameInSeconds()
    {
        int secondCount = 0;
        while (holdLastFrameInSeconds > secondCount)
        {
            yield return new WaitForSeconds(1f);
            secondCount++;
        }
        gameObject.SetActive(false);
        yield return null;
    }
}
