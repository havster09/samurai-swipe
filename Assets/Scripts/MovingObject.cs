using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class MovingObject : MonoBehaviour
    {
        public bool IsHit { get; set; }
        public float MoveTime = 0.1f;           
        public LayerMask BlockingLayer;         
        public float MaxCombatRange;
        public float MaxWalkRange;
        public SpriteRenderer SpriteRenderer;

        private BoxCollider2D _boxCollider;      
        public Rigidbody2D Rb2D;             
        private float _inverseMoveTime;
        public bool IsCoroutineMoving { get; set; }

        protected virtual void Start()
        {

        }

        protected virtual void OnEnable()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
            Rb2D = GetComponent<Rigidbody2D>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            _inverseMoveTime = 1.1f / MoveTime;
            Utilities.ResetSpriteAlpha(SpriteRenderer);
        }

        protected virtual void OnDisable()
        {
        
        }

        protected IEnumerator PerformMovementGeneral(Vector3 end)
        {
            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            while (sqrRemainingDistance > float.Epsilon && !IsFrozenPosition())
            {
                Vector3 newPostion = Vector3.MoveTowards(Rb2D.position, end, _inverseMoveTime * Time.deltaTime);
                Rb2D.MovePosition(newPostion);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                yield return null;
            }
        }

        public IEnumerator PerformMovementTo(Vector3 end, float speed, bool force = false, Action callback = null, float callbackDuration = 1f)
        {
            if (IsFrozenPosition() && !force)
            {
                yield break;
            }

            while (Vector2.Distance(transform.position, end) > .01f)
            {
                var step = speed * Time.deltaTime;
                Vector3 newPostion = Vector3.MoveTowards(Rb2D.position, end, step);
                Rb2D.MovePosition(newPostion);
                IsCoroutineMoving = true;
                yield return new WaitForFixedUpdate();
            }
            if (callback != null)
            {
                WaitFor(callback, callbackDuration);
            }
            IsCoroutineMoving = false;
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
    }
}