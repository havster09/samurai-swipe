using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMoveAnimation : MonoBehaviour
{
    public float xEndPosition;
    public float yEndPosition;
    public float holdFirstFrameInSeconds;

    protected Rigidbody2D rb2D;
    public float moveTime = 1f;

    private float inverseMoveTime;
    private SpriteRenderer spriteRenderer;
    private bool flipX;

    private Transform target;
    private float distanceFromTarget;

    void OnEnable()
	{
	    rb2D = GetComponent<Rigidbody2D>();
	    spriteRenderer = GetComponent<SpriteRenderer>();
	    inverseMoveTime = 1.1f / moveTime;
	    target = GameObject.FindGameObjectWithTag("Player").transform;
	    distanceFromTarget = target.position.x - transform.position.x;
	    Utilities.ResetSpriteAlpha(spriteRenderer);
        SimpleMoveAnimationEndEventHandler();
	}

    private void SimpleMoveAnimationEndEventHandler()
    {
        if (holdFirstFrameInSeconds > 0)
        {
            StartCoroutine(HoldFirstFrameInSeconds());
        }
        else
        {
            StartCoroutine(MoveToEndPosition());
        }
    }

    protected bool FacingRight()
    {
        return distanceFromTarget < 0;
    }

    protected IEnumerator MoveToEndPosition()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(FacingRight() ? xEndPosition : Mathf.Abs(xEndPosition), yEndPosition);
        float sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;
        StartCoroutine(Utilities.FadeOut(spriteRenderer, moveTime * 2));
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPostion = Vector2.MoveTowards(rb2D.position, endPosition, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPostion);
            sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }

    protected IEnumerator HoldFirstFrameInSeconds()
    {
        int secondCount = 0;
        while (holdFirstFrameInSeconds > secondCount)
        {
            yield return new WaitForSeconds(1f);
            secondCount++;
        }
        StartCoroutine(MoveToEndPosition());
        yield return null;
    }
}
