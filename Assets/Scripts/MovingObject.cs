using System.Collections;
using UnityEngine;
using System;
using Assets.Scripts.GoapAttributeComponents;

public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = 0.1f;           
    public LayerMask blockingLayer;         
    public float maxCombatRange;
    public float maxWalkRange;
    public SpriteRenderer spriteRenderer;

    private BoxCollider2D boxCollider;      
    protected Rigidbody2D rb2D;             
    private float inverseMoveTime;    

    protected virtual void Start()
    {

    }

    protected virtual void OnEnable()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inverseMoveTime = 1.1f / moveTime;
        Utilities.ResetSpriteAlpha(spriteRenderer);
    }

    protected virtual void OnDisable()
    {
        
    }


    protected bool Move(float xDir, float yDir, out RaycastHit2D hit, Transform target, Transform movingObject)
    {
        Vector2 start = transform.position;

        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;

        hit = Physics2D.Linecast(start, end, blockingLayer); // todo reuse for projectile attack

        boxCollider.enabled = true;

        if (hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }
        return false;
    }


    protected IEnumerator SmoothMovement(Vector3 end)
    {
        if (IsFrozenPosition())
        {
            yield break;
        }
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon && !IsFrozenPosition())
        {
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPostion);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }

    public IEnumerator MovementTo(Vector3 end, float speed, NpcAttributesComponent npcAttribute, Action callback = null, float callbackDuration = 1f)
    {
        while (Vector2.Distance(transform.position, end) > .01f || npcAttribute.Health < 1)
        {
            var step = speed * Time.deltaTime;
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, step);
            rb2D.MovePosition(newPostion);
            yield return new WaitForFixedUpdate();
        }
        if (callback != null)
        {
            WaitFor(callback, callbackDuration);
        }
    }

    protected virtual void AttemptMove<T>(float xDir, float yDir, Transform target, Transform movingObject)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit, target, movingObject);

        if (!canMove)
        {
            OnCantMove(target);
        }
    }

    public abstract bool IsFrozenPosition(); 

    public void WaitFor(Action action, float duration)
    {
        StartCoroutine(WaitForCheck(action, duration));
    }

    protected IEnumerator WaitForCheck(Action callback, float duration)
    {
        float start = Time.time;
        while (Time.time <= start + duration)
        {
            yield return new WaitForSeconds(0.1f);
        }
        callback();
    }

    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}