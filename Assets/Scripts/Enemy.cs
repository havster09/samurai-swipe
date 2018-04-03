using System.Collections;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapEnemyActions;
using Assets.Scripts.GoapHeroActions;
using UnityEngine;

namespace Assets.Scripts
{
    public class Enemy : MovingObject
    {
        public int PlayerDamage;
        public float RunSpeed;
        private GoapHeroAction _goapHeroActionScript;

        public bool HasWalkAbility;

        public Animator NpcAnimator;
        private GoapEnemyAction _goapEnemyAction;
        private bool _enemyFlipX;
        private GameObject _headFromPool;
        private SlashRenderer _slashRenderer;
        public NpcAttributesComponent NpcAttribute;
        public bool IsHit { get; set; }
        public bool IsTaunting { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsDead { get; set; }
        public bool CanWalk = true;
        public bool IsCelebrating;

        void Awake()
        {
            _goapHeroActionScript = GameObject.FindObjectOfType<GoapHeroAction>();
            _slashRenderer = GameObject.FindObjectOfType<SlashRenderer>();
            _goapEnemyAction = gameObject.GetComponent<GoapEnemyAction>();
            NpcAttribute = gameObject.GetComponent<NpcAttributesComponent>();
            NpcAnimator = GetComponent<Animator>();
            AttachAnimationClipEvents();
        }

    

        private void AttachAnimationClipEvents()
        {
            var attackOneEvent = new AnimationEvent();
            var attackOneClip = NpcAnimator.runtimeAnimatorController.animationClips[1];
            attackOneEvent.time = attackOneClip.length;
            attackOneEvent.stringParameter = "attackOneEvent end";
            attackOneEvent.functionName = "EnemyAttackOneEventHandler";
            attackOneClip.AddEvent(attackOneEvent);

            var attackTwoEvent = new AnimationEvent();
            var attackTwoClip = NpcAnimator.runtimeAnimatorController.animationClips[12];
            attackTwoEvent.time = attackTwoClip.length;
            attackTwoEvent.stringParameter = "attackTwoEvent end";
            attackTwoEvent.functionName = "EnemyAttackTwoEventHandler";
            attackTwoClip.AddEvent(attackTwoEvent);

            var tauntEvent = new AnimationEvent();
            var tauntEndFrameEvent = new AnimationEvent();
            var tauntClip = NpcAnimator.runtimeAnimatorController.animationClips[13];
            tauntEvent.time = tauntClip.length;
            tauntEndFrameEvent.time = tauntClip.length - .1f;
            tauntEvent.stringParameter = "tauntEvent end";
            tauntEvent.functionName = "TauntEventHandler";
            tauntEndFrameEvent.functionName = "TauntEventEndFrameHandler";
            tauntClip.AddEvent(tauntEvent);
            tauntClip.AddEvent(tauntEndFrameEvent);

            var winEvent = new AnimationEvent();
            var winClip = NpcAnimator.runtimeAnimatorController.animationClips[10];
            winEvent.time = winClip.length;
            winEvent.stringParameter = "winEvent end";
            winEvent.functionName = "WinEventHandler";
            winClip.AddEvent(winEvent);

            var hitEvent = new AnimationEvent();
            var hitClip = NpcAnimator.runtimeAnimatorController.animationClips[7];
            hitEvent.time = hitClip.length;
            hitEvent.functionName = "EnemyHitEventHandler";
            hitClip.AddEvent(hitEvent);

            var walkEvent = new AnimationEvent();
            var walkClip = NpcAnimator.runtimeAnimatorController.animationClips[2];
            walkEvent.time = walkClip.length;
            walkEvent.functionName = "EnemyWalkEventHandler";
            walkClip.AddEvent(walkEvent);

            var walkBackEvent = new AnimationEvent();
            var walkBackClip = NpcAnimator.runtimeAnimatorController.animationClips[3];
            walkBackEvent.time = walkBackClip.length;
            walkBackEvent.functionName = "EnemyWalkBackEventHandler";
            walkBackClip.AddEvent(walkBackEvent);

            var blockClip = NpcAnimator.runtimeAnimatorController.animationClips[16];

            var blockEventEnd = new AnimationEvent();
            blockEventEnd.time = blockClip.length;
            blockEventEnd.functionName = "EnemyBlockEndEventHandler";
            blockClip.AddEvent(blockEventEnd);
        }

        private void EnemyBlockEndEventHandler()
        {
            NpcAnimator.speed = .8f;
            WaitFor(() =>
            {
                if (NpcAnimator.GetBool("enemyBlock"))
                {
                    EnemyBlock(false);
                }
            }, 5f);
        }

        private void EnemyAttackOneEventHandler(string stringParameter)
        {
            IsAttacking = false;
        }

        private void EnemyAttackTwoEventHandler(string stringParameter)
        {
            IsAttacking = false;
        }

        private void TauntEventHandler(string stringParameter)
        {
            IsTaunting = false;
            NpcAnimator.SetFloat("enemyTauntSpeedMultiplier", 1f);
        }

        private void TauntEventEndFrameHandler()
        {
            NpcAnimator.SetFloat("enemyTauntSpeedMultiplier", .05f);
        }
        private void WinEventHandler(string stringParameter)
        {
            EnemyCelebrate();
        }

        private void EnemyCelebrate()
        {
            NpcAnimator.SetBool("enemyWinCelebrate", true);
            IsCelebrating = true;
        }

        private void EnemyHitEventHandler()
        {
            IsHit = false;
        }

        private void EnemyWalkEventHandler()
        {
            WaitFor(() => CanWalk = true, 3f);
        }

        private void EnemyWalkBackEventHandler()
        {
            WaitFor(() => CanWalk = true, 4f);
        }

        protected override void OnEnable()
        {
            _goapHeroActionScript.RemoveTargetFromList(NpcAttribute);
            base.OnEnable();
        }


        public void FaceTarget()
        {
            float targetDistance = _goapEnemyAction.target.transform.position.x - transform.position.x;

            if (targetDistance < 0 && _enemyFlipX == false)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                _enemyFlipX = true;
            }
            else if (targetDistance > 0 && _enemyFlipX)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                _enemyFlipX = false;
            }
        }

        protected override void AttemptMove<T>(float xDir, float yDir, Transform target, Transform movingObject)
        {
            base.AttemptMove<T>(xDir, yDir, target, movingObject);
        }

        public void MoveEnemy()
        {
            if (!NpcAnimator.GetBool("enemyRun"))
            {
                NpcAnimator.SetBool("enemyRun", false);
            }

            if (IsFrozenPosition())
            {
                return;
            }

            float xDir = 0;
            float yDir = 0;
            CanWalk = false;

            bool walkBackwards = Random.Range(0, 5) < 2 && Utilities.ReplaceClone(name) != "Ukyo";
            if (!walkBackwards)
            {
                xDir = _goapEnemyAction.target.transform.position.x > transform.position.x ? .5f : -.5f;
                NpcAnimator.SetTrigger("enemyWalk");
            }
            else
            {
                xDir = _goapEnemyAction.target.transform.position.x > transform.position.x ? -.5f : .5f;
                NpcAnimator.SetTrigger("enemyWalkBack");
                NpcAttribute.Brave += 1;
            }


            Debug.DrawLine(_goapEnemyAction.target.transform.position, transform.position, Color.red);
            AttemptMove<Enemy>(xDir, yDir, _goapEnemyAction.target.transform, transform);
        }

        public void MoveEnemyBack()
        {
            float distance = _goapEnemyAction.target.transform.position.x > transform.position.x ? -2f : 2f;
            Vector2 end = new Vector2(transform.position.x, 0) + new Vector2(distance, 0);
            StartCoroutine(BlockMovement(end, NpcAttribute));
        }

        public bool IsInWalkRange()
        {
            return Mathf.Abs(Vector3.Distance(_goapEnemyAction.target.transform.transform.position, transform.position)) < maxWalkRange;
        }

        public bool IsInCombatRange()
        {
            return Mathf.Abs(Vector3.Distance(_goapEnemyAction.target.transform.transform.position, transform.position)) < maxCombatRange;
        }

        public void StopEnemyVelocity()
        {
            NpcAnimator.SetBool("enemyRun", false);
            rb2D.velocity = new Vector2(0f, rb2D.velocity.y);
        }

        public void JumpAttack()
        {
            rb2D.velocity = new Vector2(_enemyFlipX ? -2f : 2f, 6f);
            NpcAnimator.SetFloat("enemyAttackJumpVertical", 1f);
            StartCoroutine("EnemyAttackJumpVertical");
        }

        protected IEnumerator EnemyAttackJumpVertical()
        {
            while (transform.position.y < 2f)
            {
                yield return new WaitForFixedUpdate();
            }
            StartCoroutine("EnemyAttackJumpVerticalDown");
        }

        protected IEnumerator EnemyAttackJumpVerticalDown()
        {
            while (transform.position.y > 0)
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, -8f);
                yield return new WaitForFixedUpdate();
            }
            rb2D.velocity = new Vector2(0, 0);
            NpcAnimator.SetFloat("enemyAttackJumpVertical", 0);
            transform.position = new  Vector2(transform.position.x, 0f);
        }

        protected override void OnCantMove<T>(T component)
        {
            NpcAnimator.SetTrigger("enemyAttackOne");
        }

        public void Taunt()
        {
            NpcAnimator.SetTrigger("enemyTaunt");
        }

        public void CrossSword(bool state)
        {
            FaceTarget();            
            NpcAnimator.SetBool("enemyCrossSword", state);
        }

        public void EnemyBlock(bool state)
        {
            FaceTarget();
            if (!state)
            {
                NpcAnimator.speed = 1;
            }
            NpcAnimator.SetBool("enemyBlock", state);
        }

        public void AttackGrounded()
        {
            NpcAnimator.SetTrigger("enemyAttackGrounded");
        }

        public void Attack(string attackType)
        {
            IsAttacking = true;
            NpcAnimator.SetTrigger(attackType);
        }

        public void EnemyDie()
        {
            StopEnemyVelocity();
            EnemySpray();
            NpcAttribute.Health = 0;
            IsDead = true;
        }

        private void EnemySpray()
        {
            NpcAnimator.SetFloat("enemyHitSpeedMultiplier", 0f);
            NpcAnimator.Play("enemyHit", 0, 1f);
            GetBloodEffect("Blood", "BloodEffectSpray");
            StartCoroutine(SprayBlood(3));
        }

        protected IEnumerator SprayBlood(int sprayTime)
        {
            int elapsedSprayTime = 0;
            while (elapsedSprayTime < sprayTime)
            {
                yield return new WaitForSeconds(1f);
                elapsedSprayTime++;
            }
            NpcAnimator.SetFloat("enemyHitSpeedMultiplier", 1f);
            int randomDeath = Random.Range(0, 5);
            if (randomDeath == 0)
            {
                // NpcAnimator.SetBool("enemyDrop", true);
                EnemyDecapitation();
            }
            else if (randomDeath == 1)
            {
                EnemySplitDrop();
                string enemyName = Utilities.ReplaceClone(name);
                GetBloodEffect("BodyParts", enemyName + "TorsoSplit");
            }
            else
            {
                EnemyDieSplit();
            }
            StartCoroutine(DieStateHandler(5f));
            yield return null;
        }

        private void EnemyDieSplit()
        {
            NpcAnimator.SetBool("enemyDieSplit", true);
            GetBloodEffect("Blood", "BloodEffectDiagonal1");
            WaitFor(() => GetBloodEffect("Blood", "BloodEffectSplit"), .1f);
        }

        private void EnemySplitDrop()
        {
            NpcAnimator.SetBool("enemySplitDrop", true);
            GetBloodEffect("Blood", "BloodEffectDiagonal1");
            WaitFor(() => GetBloodEffect("Blood", "BloodEffectSplit"), .1f);
        }

        private void EnemyDecapitation()
        {
            NpcAnimator.SetBool("enemyDecapitationBody", true);
            Vector2 start = transform.position;
            float distance = _enemyFlipX ? .25f : -.25f;
            Vector2 end = start + new Vector2(distance, 0);
            StartCoroutine(SmoothMovement(end));
            string headString = Utilities.ReplaceClone(name) + "Head";
            _headFromPool = ObjectPooler.SharedInstance.GetPooledObject("BodyPart", headString);
            int randomDecapitationIndex = Random.Range(0, ObjectPooler.SharedInstance.bloodDecapitationEffects.Length);
            GetBloodEffect("BloodDecapitation",
                ObjectPooler.SharedInstance.bloodDecapitationEffects[randomDecapitationIndex]);
            if (_headFromPool)
            {
                _headFromPool.transform.position = transform.position;
                _headFromPool.transform.rotation = transform.rotation;
                _headFromPool.SetActive(true);
                WaitFor(() => GetBloodEffect("Blood"), .1f);
            }
        }

        protected IEnumerator DieStateHandler(float waitTime)
        {
            float elapsedWaitTime = 0f;
            while (elapsedWaitTime < waitTime)
            {
                // todo spray blood objects here
                yield return new WaitForSeconds(1f);
                elapsedWaitTime++;
            }
            StartCoroutine(WaitToRespawn(.5f));
            StartCoroutine(Utilities.FadeOut(spriteRenderer, .5f));
            yield return null;
        }

        protected IEnumerator WaitToRespawn(float respawnTime)
        {
            int elapsedDeathTime = 0;
            while (elapsedDeathTime < respawnTime)
            {
                yield return new WaitForSeconds(3f);
                elapsedDeathTime++;
            }
            RespawnEnemy();
            yield return null;
        }

        private void RespawnEnemy()
        {
            gameObject.SetActive(false);
            IsDead = false;
            GameManager.RespawnEnemyFromPool();
        }

        public void NpcCelebrate()
        {
            NpcAnimator.SetTrigger("enemyWin");
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.tag == "SlashCollider")
            {
                if (NpcAttribute.Health > 0)
                {
                    _goapHeroActionScript.AddTargetToList(NpcAttribute);
                }
            }
        }

        public void EnemyHitSuccess(int damage)
        {
            _slashRenderer.RemoveSlash();

            if (NpcAnimator.GetBool("enemyBlock"))
            {
                EnemyBlock(false);
            }

            if (NpcAttribute.Health > 0)
            {
                EnemyHit(damage);
            }
        }

        public void EnemyHitFail()
        {
            _slashRenderer.RemoveSlash();
            NpcAttribute.DefendCount -= 1;
            MoveEnemyBack();
            if (!NpcAnimator.GetBool("enemyBlock"))
            {
                EnemyBlock(true);
            }
            else
            {
                NpcAnimator.Play("enemyBlock", -1, .5f);
            }
        }

        public void EnemyHit(int damage)
        {
            StopEnemyVelocity();
            GetBloodEffect("Blood", "BloodEffect1");
            NpcAnimator.SetTrigger("enemyHit");
            IsHit = true;
            NpcAttribute.Health -= damage;

            if (NpcAttribute.Health < 1 && !IsDead)
            {
                EnemyDie();
            }
        }

        private void GetBloodEffect(string tag, string name = null)
        {
            GameObject bloodFromPool = ObjectPooler.SharedInstance.GetPooledObject(tag, name);
            if (bloodFromPool)
            {
                bloodFromPool.transform.position = transform.position;
                bloodFromPool.transform.rotation = transform.rotation;
                bloodFromPool.SetActive(true);
            }
        }

        public override bool IsFrozenPosition()
        {
            if (
                IsAttacking.Equals(true) ||
                IsTaunting.Equals(true) ||
                IsDead.Equals(true) ||
                IsCelebrating.Equals(true) ||
                IsHit.Equals(true))
            {
                return true;
            }
            return false;
        }

        public bool IsAnimationPlaying(string animationTag)
        {
            if (NpcAnimator.GetCurrentAnimatorStateInfo(0).Equals(null) ||
                NpcAnimator.GetCurrentAnimatorStateInfo(0).IsTag(animationTag))
            {
                return true;
            }
            return false;
        }

        public AnimationClip GetAnimationClip(string name)
        {
            if (!NpcAnimator) return null;

            foreach (AnimationClip clip in NpcAnimator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == name)
                {
                    return clip;
                }
            }
            return null; // no clip by that name
        }

        void OnDisable()
        {
            IsAttacking = false;
            IsTaunting = false;
            IsDead = false;
            IsCelebrating = false;
            IsHit = false;
        }
    }
}