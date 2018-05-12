using System;
using System.Collections;
using Assets.Scripts.GoapAttributeComponents;
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
        public Renderer NpcRenderer;

        public Coroutine MoveBackCoroutine;

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

        public void MoveBack(GameObject target, float to, float speed, Action action)
        {
            float distance = target.transform.position.x > transform.position.x ? -to : to;
            Vector2 end = new Vector2(transform.position.x, 0) + new Vector2(distance, 0);
            MoveBackCoroutine = StartCoroutine(PerformMovementTo(end, speed, true, action));
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

        public IEnumerator PerformMovementTo(Vector3 end, float speed, bool force = false, Action callback = null, float callbackDuration = 1f, Animator npcAnimator = null, string anmimationType = null)
        {
            if (IsFrozenPosition() && !force)
            {
                yield break;
            }

            var npcAttribute = gameObject.GetComponent<NpcAttributesComponent>();

            bool isAlive = !(npcAttribute != null && npcAttribute.Health < 1);

            while (Vector2.Distance(transform.position, end) > .01f && isAlive)
            {
                var step = speed * Time.deltaTime;
                var newPostion = Vector3.MoveTowards(Rb2D.position, end, step);
                Rb2D.MovePosition(newPostion);
                IsCoroutineMoving = true;
                yield return new WaitForFixedUpdate();
            }

            if (anmimationType != null && npcAnimator != null)
            {
                npcAnimator.ResetTrigger(anmimationType);
            }
            if (npcAnimator != null) npcAnimator.StopPlayback();
            if (callback != null) WaitFor(callback, callbackDuration);

            IsCoroutineMoving = false;
        }

        public abstract bool IsFrozenPosition();

        public void WaitFor(Action action, float duration, bool cancel = true)
        {
            StartCoroutine(WaitForCheck(action, duration, cancel));
        }

        protected IEnumerator WaitForCheck(Action callback, float duration, bool cancel)
        {
            float start = Time.time;
            while (Time.time <= start + duration && cancel)
            {
                yield return new WaitForSeconds(0.1f);
            }
            callback();
        }
    }
}