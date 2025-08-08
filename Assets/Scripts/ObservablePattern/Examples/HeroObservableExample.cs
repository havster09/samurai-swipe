using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.FsmHeroStates;
using Assets.Scripts.GoapAttributeComponents;
using Assets.Scripts.GoapHeroActions;
using Assets.Scripts.GoapHeroSubStates;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ObservablePattern.Examples
{
    /// <summary>
    /// Example of how to refactor the Hero class to use the observable pattern
    /// This shows how to replace callback-based patterns with observables
    /// </summary>
    public class HeroObservableExample : MovingObject
    {
        public static HeroObservableExample Instance;
        public const float HeroStep = .5f;
        public const float ResetPositionThreshold = 8f;
        public GameObject CurrentTarget;
        public bool HeroFlipX;
        public Animator NpcHeroAnimator;
        public FsmHeroBaseStateMachineHandler FsmHeroBaseStateMachineHandlerScript;
        public DashEndStateMachineHandler DashEndStateMachineHandlerScript;

        public bool IsAttacking { get; set; }
        public bool IsInPoseState;
        public bool IsInResetState;

        private GoapAction GoapActionScript;

        // Observable events for hero actions
        public readonly ObservableEvent OnHeroBlockEnd = new ObservableEvent();
        public readonly ObservableEvent OnHeroHitEnd = new ObservableEvent();
        public readonly ObservableEvent<bool> OnHeroBlock = new ObservableEvent<bool>();
        public readonly ObservableEvent<bool> OnHeroHit = new ObservableEvent<bool>();
        public readonly ObservableEvent<int> OnEnemyHit = new ObservableEvent<int>();
        public readonly ObservableEvent<string> OnAttack = new ObservableEvent<string>();
        public readonly ObservableEvent<bool> OnBloodCover = new ObservableEvent<bool>();
        public readonly ObservableEvent<bool> OnTurnPose = new ObservableEvent<bool>();
        public readonly ObservableEvent OnWipeBlood = new ObservableEvent();
        public readonly ObservableEvent OnCleanWeapon = new ObservableEvent();
        public readonly ObservableEvent OnHeroResetPosition = new ObservableEvent();

        // Subscriptions to manage cleanup
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            NpcHeroAnimator = GetComponent<Animator>();
            NpcRenderer = GetComponent<Renderer>();
            FsmHeroBaseStateMachineHandlerScript =
                GameObject.FindObjectOfType<FsmHeroBaseStateMachineHandler>();
            DashEndStateMachineHandlerScript =
                GameObject.FindObjectOfType<DashEndStateMachineHandler>();
            GoapActionScript =
                GameObject.FindObjectOfType<GoapAction>();
            
            AttachAnimationClipEvents();
            SetupObservableSubscriptions();
        }

        protected override void Start()
        {
            StartStateMachines();
            StartSubStateMachines();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        private void OnDestroy()
        {
            // Clean up all subscriptions
            foreach (var subscription in _subscriptions)
            {
                if (subscription != null)
                    subscription.Dispose();
            }
            _subscriptions.Clear();

            // Dispose of all observable events
            if (OnHeroBlockEnd != null)
                OnHeroBlockEnd.Dispose();
            if (OnHeroHitEnd != null)
                OnHeroHitEnd.Dispose();
            if (OnHeroBlock != null)
                OnHeroBlock.Dispose();
            if (OnHeroHit != null)
                OnHeroHit.Dispose();
            if (OnEnemyHit != null)
                OnEnemyHit.Dispose();
            if (OnAttack != null)
                OnAttack.Dispose();
            if (OnBloodCover != null)
                OnBloodCover.Dispose();
            if (OnTurnPose != null)
                OnTurnPose.Dispose();
            if (OnWipeBlood != null)
                OnWipeBlood.Dispose();
            if (OnCleanWeapon != null)
                OnCleanWeapon.Dispose();
            if (OnHeroResetPosition != null)
                OnHeroResetPosition.Dispose();
        }

        private void AttachAnimationClipEvents()
        {
            var blockClip = NpcHeroAnimator.runtimeAnimatorController.animationClips[23];
            
            var blockEventEnd = new AnimationEvent();
            blockEventEnd.time = blockClip.length;
            blockEventEnd.functionName = "HeroBlockEndEventHandler";
            blockClip.AddEvent(blockEventEnd);

            var hitClip = NpcHeroAnimator.runtimeAnimatorController.animationClips[28];

            var hitEventEnd = new AnimationEvent();
            hitEventEnd.time = hitClip.length;
            hitEventEnd.functionName = "HeroHitEndEventHandler";
            hitClip.AddEvent(hitEventEnd);
        }

        private void SetupObservableSubscriptions()
        {
            // Subscribe to hero block end event
            _subscriptions.Add(OnHeroBlockEnd.Subscribe(() =>
            {
                // Use the new observable timing utilities
                ObservableTimingUtilities.Instance.WaitFor("heroBlockEnd", 0.2f, () =>
                {
                    NpcHeroAnimator.Play("heroIdle");
                });
            }));

            // Subscribe to hero hit end event
            _subscriptions.Add(OnHeroHitEnd.Subscribe(() =>
            {
                // Use observable timing utilities instead of direct callbacks
                ObservableTimingUtilities.Instance.WaitFor("heroHitEnd", 0.1f, () =>
                {
                    OnHeroHit.Trigger(false);
                });
            }));

            // Subscribe to enemy hit events
            _subscriptions.Add(OnEnemyHit.Subscribe(damage =>
            {
                GetBloodEffect("Blood", "BloodEffect1");
                IsHit = true;
            }));

            // Subscribe to attack events
            _subscriptions.Add(OnAttack.Subscribe(attackType =>
            {
                IsAttacking = true;
                NpcHeroAnimator.SetTrigger(attackType);
                CheckEnemiesInRangeOfAttack();
            }));

            // Subscribe to blood cover events
            _subscriptions.Add(OnBloodCover.Subscribe(state =>
            {
                NpcHeroAnimator.SetBool("heroBloodCover", state);
                NpcHeroAttributesComponent.Instance.Rage = 0;
            }));

            // Subscribe to turn pose events
            _subscriptions.Add(OnTurnPose.Subscribe(state =>
            {
                NpcHeroAnimator.SetBool("heroTurnPause", state);
            }));

            // Subscribe to wipe blood events
            _subscriptions.Add(OnWipeBlood.Subscribe(() =>
            {
                // Implementation for wiping blood
                Debug.Log("Wiping blood");
            }));

            // Subscribe to clean weapon events
            _subscriptions.Add(OnCleanWeapon.Subscribe(() =>
            {
                // Implementation for cleaning weapon
                Debug.Log("Cleaning weapon");
            }));

            // Subscribe to hero reset position events
            _subscriptions.Add(OnHeroResetPosition.Subscribe(() =>
            {
                // Implementation for resetting hero position
                Debug.Log("Resetting hero position");
            }));
        }

        private void StartStateMachines()
        {
            FsmHeroBaseStateMachineHandlerScript.StartFsmHeroIdleStateMachineHandler();
        }

        private void StartSubStateMachines()
        {
            DashEndStateMachineHandlerScript.StartDashEndStateMachineHandler();
        }

        private void HeroBlockEndEventHandler()
        {
            // Instead of direct callback, trigger the observable event
            OnHeroBlockEnd.Trigger();
        }

        private void HeroHitEndEventHandler()
        {
            // Instead of direct callback, trigger the observable event
            OnHeroHitEnd.Trigger();
        }

        public void HeroBlock(bool state, GameObject target = null)
        {
            if (target != null)
            {
                FaceTarget(target);
            }
            NpcHeroAnimator.SetBool("heroBlock", state);
            
            // Trigger the observable event
            OnHeroBlock.Trigger(state);
        }

        public void HeroHit(bool state, GameObject target = null)
        {
            if (target != null)
            {
                FaceTarget(target);
            }
            NpcHeroAnimator.SetBool("heroHit", state);
            
            // Trigger the observable event
            OnHeroHit.Trigger(state);
            
            if (state)
            {
                OnEnemyHit.Trigger(1);
            }
        }

        public void FaceTarget(GameObject target)
        {
            CurrentTarget = target;
            float targetDistance = target.transform.position.x - transform.position.x;

            if (targetDistance < 0 && HeroFlipX == false)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                HeroFlipX = true;
            }
            else if (targetDistance > 0 && HeroFlipX)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                HeroFlipX = false;
            }
        }

        public void CrossSword(bool state)
        {
            NpcHeroAnimator.SetBool("heroBlock", false);
            NpcHeroAnimator.SetBool("heroCrossSword", state);
        }

        public void Attack(string attackType)
        {
            // Trigger the observable event instead of direct implementation
            OnAttack.Trigger(attackType);
        }

        private void CheckEnemiesInRangeOfAttack()
        {
            var totalNpcInRangeOfAttack = GoapActionScript.GetActiveNpcAttributesComponentsInRangeByDirection(gameObject, 1.2f);
            totalNpcInRangeOfAttack.ToList()
                .ForEach((npc) =>
                {
                    var enemyScript = npc.GetComponent<Enemy>();
                    enemyScript.EnemyHitSuccess(50);
                });
        }

        public void BloodCover(bool state)
        {
            // Trigger the observable event
            OnBloodCover.Trigger(state);
        }

        public void TurnPose(bool state)
        {
            // Trigger the observable event
            OnTurnPose.Trigger(state);
        }

        public bool IsAnimationTagPlaying(string animationTag)
        {
            if (NpcHeroAnimator.GetCurrentAnimatorStateInfo(0).IsTag(animationTag))
            {
                return true;
            }
            return false;
        }

        public override bool IsFrozenPosition()
        {
            return NpcHeroAnimator.GetBool("heroBloodCover") ||
                   NpcHeroAnimator.GetBool("heroCleanWeapon") ||
                   NpcHeroAnimator.GetBool("heroTurnPause");
        }

        public void WipeBlood()
        {
            // Trigger the observable event
            OnWipeBlood.Trigger();
        }

        public void CleanWeapon()
        {
            // Trigger the observable event
            OnCleanWeapon.Trigger();
        }

        public void HeroResetPosition()
        {
            // Trigger the observable event
            OnHeroResetPosition.Trigger();
        }

        public bool IsVulnerable()
        {
            return !IsFrozenPosition();
        }

        public int GetAttackTypeAndDamage(GameObject target)
        {
            // Implementation for getting attack type and damage
            return 1;
        }

        public bool IsAttackable()
        {
            return !IsFrozenPosition();
        }

        public void Dash(Vector3 end, float speed, RaycastHit2D[] hits)
        {
            // Implementation for dash
            Debug.Log("Dashing");
        }

        public void HandleDashEndEvent(RaycastHit2D[] hits)
        {
            // Implementation for handling dash end
            Debug.Log("Dash ended");
        }

        public void DamageRayCastTargets(RaycastHit2D[] hits)
        {
            // Implementation for damaging raycast targets
            Debug.Log("Damaging raycast targets");
        }
    }
} 