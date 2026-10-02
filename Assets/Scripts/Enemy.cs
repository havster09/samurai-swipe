using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapEnemyActions;
using Assets.Scripts.GoapHeroActions;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Enemy : MovingObject
    {
        public Animator NpcAnimator;
        private GoapEnemyAction _goapEnemyAction;
        public bool EnemyFlipX;
        private GameObject _headFromPool;
        public NpcAttributesComponent NpcAttribute;
        public bool IsTaunting { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsDead { get; set; }
        public bool IsCanWalk = true;
        private bool _pendingDeath;
        public bool IsCelebrating;

        public Coroutine MoveEnemyCoroutine;

        private void Awake()
        {
            _goapEnemyAction = gameObject.GetComponent<GoapEnemyAction>();
            NpcAttribute = gameObject.GetComponent<NpcAttributesComponent>();
            NpcAnimator = GetComponent<Animator>();
            NpcRenderer = GetComponent<Renderer>();
            AttachAnimationClipEvents();
        }

        public void OnTransformFind(Transform child)
        {
            // Debug.Log(transform.localPosition.x);
            // Debug.Log("From Broadcast");
        }

        private static bool _enemyClipEventsAttached = false;

        private void AttachAnimationClipEvents()
        {
            // AnimationClip assets are shared across all enemy instances.
            // Adding events is a one-time operation per play session; subsequent
            // instances must skip to avoid accumulating duplicate event firings.
            if (_enemyClipEventsAttached) return;
            _enemyClipEventsAttached = true;

            var clips = NpcAnimator.runtimeAnimatorController.animationClips;

            // Attack clips — matched by name so reordering the animator won't break this.
            clips.Where(a => a.name.Contains("Attack"))
                .ToList()
                .ForEach(a =>
                {
                    var fn = char.ToUpper(a.name[0]) + a.name.Substring(1) + "EventHandler";
                    a.AddEvent(new AnimationEvent { time = a.length / 2, functionName = fn, stringParameter = "mid" });
                    a.AddEvent(new AnimationEvent { time = a.length,     functionName = fn, stringParameter = "end" });
                });

            AddClipEvent(clips, "taunt",    "TauntEventHandler",        c => c.length,       "tauntEvent end");
            AddClipEvent(clips, "taunt",    "TauntEventEndFrameHandler", c => c.length - .1f, "");
            AddClipEvent(clips, "win",      "WinEventHandler",           c => c.length,       "winEvent end");
            AddClipEvent(clips, "hit",      "EnemyHitEventHandler",      c => c.length,       "");
            AddClipEvent(clips, "walkBack", "EnemyWalkBackEventHandler", c => c.length,       "");
            AddClipEvent(clips, "walk",     "EnemyWalkEventHandler",     c => c.length,       "");
            AddClipEvent(clips, "block",    "EnemyBlockEndEventHandler", c => c.length,       "");
        }

        /// <summary>
        /// Finds the first clip whose name contains <paramref name="namePart"/> (case-insensitive)
        /// and adds a single AnimationEvent to it.
        /// </summary>
        private static void AddClipEvent(
            AnimationClip[] clips, string namePart,
            string functionName,
            System.Func<AnimationClip, float> timeFn,
            string stringParam)
        {
            var clip = System.Array.Find(clips,
                c => c.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0);
            if (clip == null)
            {
                Debug.LogWarning(string.Format("[Enemy] AttachEvents: clip '{0}' not found", namePart));
                return;
            }
            var ev = new AnimationEvent
            {
                time            = timeFn(clip),
                functionName    = functionName,
                stringParameter = stringParam
            };
            clip.AddEvent(ev);
        }

        private void EnemyBlockEndEventHandler()
        {
            NpcAnimator.speed = .8f;
        }

        private void EnemyAttackOneEventHandler(string stringParameter)
        {
            if (stringParameter == "end")
            {
                IsAttacking = false;
            }

            if (stringParameter == "mid")
            {
                if (Hero.Instance.IsVulnerable() && NpcAttribute.Health > 0)
                {
                    if (!Hero.Instance.IsCoroutineMoving)
                    {
                        if (!Hero.Instance.NpcHeroAnimator.GetBool("heroHit"))
                        {
                            Hero.Instance.HeroHit(true, gameObject);
                        }
                    }
                }
            }
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
            if (_pendingDeath)
                StartCoroutine(DelayedDie(1f));
        }

        private IEnumerator DelayedDie(float delay)
        {
            yield return new WaitForSeconds(delay);
            EnemyDie();
        }

        private void EnemyWalkEventHandler()
        {
            TimingUtilities.Instance.WaitFor(() => IsCanWalk = true, 3f);
        }

        private void EnemyWalkBackEventHandler()
        {
            TimingUtilities.Instance.WaitFor(() => IsCanWalk = true, 3f);
        }

        public void FaceTarget()
        {
            if (!_goapEnemyAction.target)
            {
                return;
            }

            float targetDistance = _goapEnemyAction.target.transform.position.x - transform.position.x;

            if (targetDistance < 0 && EnemyFlipX == false)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                EnemyFlipX = true;
            }
            else if (targetDistance > 0 && EnemyFlipX)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                EnemyFlipX = false;
            }
        }

        public void MoveEnemy()
        {

            if (IsFrozenPosition())
            {
                return;
            }

            float xDir = 0;
            IsCanWalk = false;

            bool walkBackwards = Random.Range(0, 5) < 2 &&
                Utilities.ReplaceClone(name) != "Ukyo";
            if (!walkBackwards)
            {
                xDir = _goapEnemyAction.target.transform.position.x > transform.position.x ? .5f : -.5f;
                NpcAnimator.SetBool("enemyWalk", true); 
            }
            else
            {
                xDir = _goapEnemyAction.target.transform.position.x > transform.position.x ? -.5f : .5f;
                NpcAnimator.SetTrigger("enemyWalkBack");
            }

            Vector2 end = new Vector2(transform.position.x, 0) + new Vector2(xDir, 0);
            if (!IsCoroutineMoving)
            {
                MoveEnemyCoroutine = StartCoroutine(PerformMovementTo(end, .8f, false, null, 1f, NpcAnimator, "enemyWalk"));
            }
        }

        public bool IsInWalkRange()
        {
            return Mathf.Abs(Vector3.Distance(_goapEnemyAction.target.transform.transform.position, transform.position)) < MaxWalkRange;
        }

        public bool IsInCombatRange()
        {
            return Mathf.Abs(Vector3.Distance(_goapEnemyAction.target.transform.transform.position, transform.position)) < MaxCombatRange;
        }

        public void StopEnemyVelocity()
        {
            NpcAnimator.SetBool("enemyRun", false);
            Rb2D.velocity = new Vector2(0f, Rb2D.velocity.y);
        }

        public void JumpAttack()
        {
            Rb2D.velocity = new Vector2(EnemyFlipX ? -2f : 2f, 6f);
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
                Rb2D.velocity = new Vector2(Rb2D.velocity.x, -8f);
                yield return new WaitForFixedUpdate();
            }
            Rb2D.velocity = new Vector2(0, 0);
            NpcAnimator.SetFloat("enemyAttackJumpVertical", 0);
            transform.position = new Vector2(transform.position.x, 0f);
        }

        public void Taunt()
        {
            NpcAnimator.SetTrigger("enemyTaunt");
        }

        public void CrossSword(bool state)
        {
            NpcAnimator.SetBool("enemyCrossSword", state);
            if (state)
            {
                SlashRenderer.Instance.RemoveSlash();
            }
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
            // Block enemy attacks during a global combat pause (e.g. post dash-attack window).
            // Enemies can still reposition freely — only the attack trigger is gated here.
            if (CombatPauseManager.Instance != null && CombatPauseManager.Instance.IsPaused)
            {
                Debug.Log(string.Format("[Pause] {0} attack blocked (pause active)", gameObject.name));
                return;
            }

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
            TimingUtilities.Instance.WaitFor(() => GetBloodEffect("Blood", "BloodEffectSplit"), .1f);
        }

        private void EnemySplitDrop()
        {
            NpcAnimator.SetBool("enemySplitDrop", true);
            GetBloodEffect("Blood", "BloodEffectDiagonal1");
            TimingUtilities.Instance.WaitFor(() => GetBloodEffect("Blood", "BloodEffectSplit"), .1f);
        }

        private void EnemyDecapitation()
        {
            NpcAnimator.SetBool("enemyDecapitationBody", true);
            Vector2 start = transform.position;
            float distance = EnemyFlipX ? .25f : -.25f;
            Vector2 end = start + new Vector2(distance, 0);
            StartCoroutine(PerformMovementGeneral(end));
            string headString = Utilities.ReplaceClone(name) + "Head";
            _headFromPool = ObjectPooler.Instance.GetPooledObject("BodyPart", headString);
            int randomDecapitationIndex = Random.Range(0, ObjectPooler.Instance.BloodDecapitationEffects.Length);
            GetBloodEffect("BloodDecapitation",
                ObjectPooler.Instance.BloodDecapitationEffects[randomDecapitationIndex]);
            if (_headFromPool)
            {
                _headFromPool.transform.position = transform.position;
                _headFromPool.transform.rotation = transform.rotation;
                _headFromPool.SetActive(true);
                TimingUtilities.Instance.WaitFor(() => GetBloodEffect("Blood"), .1f);
            }
        }

        protected IEnumerator DieStateHandler(float waitTime)
        {
            float elapsedWaitTime = 0f;
            while (elapsedWaitTime < waitTime)
            {
                yield return new WaitForSeconds(1f);
                elapsedWaitTime++;
            }
            StartCoroutine(WaitToRespawn(.5f));
            StartCoroutine(Utilities.FadeOut(SpriteRenderer, .5f));
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
                Debug.Log(string.Format("[GOAP] SLASH HIT  {0}  hp:{1}", gameObject.name, NpcAttribute.Health));
                if (NpcAttribute.Health > 0)
                {
                    GoapHeroAction.Instance.AddTargetToList(NpcAttribute);
                }
            }
        }

        public void EnemyHitSuccess(int damage)
        {
            SlashRenderer.Instance.RemoveSlash();
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
            SlashRenderer.Instance.RemoveSlash();
            NpcAttribute.DefendCount -= 1;
            MoveBack(_goapEnemyAction.target, .35f, 3, () => EnemyBlock(false));
            if (!NpcAnimator.GetBool("enemyBlock"))
            {
                EnemyBlock(true);
            }
            else
            {
                NpcAnimator.Play("enemyBlock", 1, .5f);
            }
        }

        public void EnemyHit(int damage)
        {
            StopEnemyVelocity();
            GetBloodEffect("Blood", "BloodEffect1");
            NpcAnimator.SetTrigger("enemyHit");
            IsHit = true;
            NpcAttribute.Health -= damage;

            if (NpcAttribute.Health < 1 && !IsDead && !_pendingDeath)
            {
                // Flag the kill — actual death fires after the hit animation ends.
                _pendingDeath = true;
            }
        }

        public override bool IsFrozenPosition()
        {
            return IsAttacking   ||
                   IsTaunting    ||
                   IsDead        ||
                   _pendingDeath ||
                   IsCelebrating ||
                   IsHit;
        }

        public bool IsAnimationTagPlaying(string animationTag)
        {
            if (NpcAnimator.GetCurrentAnimatorStateInfo(0).Equals(null) ||
                NpcAnimator.GetCurrentAnimatorStateInfo(0).IsTag(animationTag))
            {
                return true;
            }
            return false;
        }

        public AnimationClip GetAnimationClip(string animationName)
        {
            if (!NpcAnimator) return null;

            return NpcAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == animationName);
        }

        protected override void OnEnable()
        {
            Debug.Log(string.Format("[GOAP] ENEMY ENABLE  {0}  (recycled from pool)", gameObject.name));
            Broadcaster<Transform>.EnableListener("FindChild", OnTransformFind);
            base.OnEnable();
        }

        protected void OnDisable()
        {
            Debug.Log(string.Format("[GOAP] ENEMY DISABLE  {0}  isDead:{1}", gameObject.name, IsDead));
            Broadcaster<Transform>.DisableListener("FindChild", OnTransformFind);
            GoapHeroAction.Instance.RemoveTargetFromList(NpcAttribute);
            // If GOAP was locked onto this enemy as its current target, clear it so
            // the hero doesn't keep chasing a pooled (inactive) object.
            if (GoapHeroAction.Instance.TargetNpcAttribute == NpcAttribute)
                GoapHeroAction.Instance.TargetNpcAttribute = null;

            if (GoapHeroDashAttackAction.Hits != null)
            {
                GoapHeroDashAttackAction.Hits = GoapHeroDashAttackAction.Hits
                                .Where(hit => hit.collider.gameObject != gameObject)
                                .ToArray();
            }
            Reset();
        }

        private void Reset()
        {
            IsAttacking   = false;
            IsTaunting    = false;
            IsDead        = false;
            _pendingDeath = false;
            IsCelebrating = false;
            IsHit         = false;
            IsCanWalk     = true;
            IsCoroutineMoving = false;
        }
    }
}