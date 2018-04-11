using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMoveAnimation : MonoBehaviour
{
    public float XEndPosition;
    public float YEndPosition;
    public float holdFirstFrameInSeconds;

    protected Rigidbody2D Rb2D;
    public float MoveTime = 1f;

    private float _inverseMoveTime;
    private SpriteRenderer _spriteRenderer;
    private Transform _target;
    private float _distanceFromTarget;

    void OnEnable()
	{
	    Rb2D = GetComponent<Rigidbody2D>();
	    _spriteRenderer = GetComponent<SpriteRenderer>();
	    _inverseMoveTime = 1.1f / MoveTime;
	    _target = GameObject.FindGameObjectWithTag("Player").transform;
	    _distanceFromTarget = _target.position.x - transform.position.x;
	    Utilities.ResetSpriteAlpha(_spriteRenderer);
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
        return _distanceFromTarget < 0;
    }

    protected IEnumerator MoveToEndPosition()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(FacingRight() ? XEndPosition : Mathf.Abs(XEndPosition), YEndPosition);
        float sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;
        StartCoroutine(Utilities.FadeOut(_spriteRenderer, MoveTime * 2));
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPostion = Vector2.MoveTowards(Rb2D.position, endPosition, _inverseMoveTime * Time.deltaTime);
            Rb2D.MovePosition(newPostion);
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
