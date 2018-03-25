using UnityEngine;
using System.Collections;
using Assets.Scripts.GoapEnemyActions;

public class Enemy : MovingObject
{
    public int PlayerDamage;
    public float RunSpeed;

    public bool HasWalkAbility;

    public Animator NpcAnimator;
    private GoapEnemyAction _goapEnemyAction;
    private bool _enemyFlipX;
    private bool _checkingIsInCombatRangeWhileRunning;
    private GameObject _headFromPool;
    private SlashRenderer _slashRenderer;
    public NpcAttributesComponent NpcAttributes;
    public bool IsHit { get; set; }
    public bool IsTaunting { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsDead { get; set; }
    public bool CanWalk = true;
    public bool IsCelebrating;
    private bool _grounded = true;

    void Awake()
    {
        _slashRenderer = GameObject.FindObjectOfType<SlashRenderer>();
        _goapEnemyAction = gameObject.GetComponent<GoapEnemyAction>();
        NpcAttributes = gameObject.GetComponent<NpcAttributesComponent>();
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
        _slashRenderer.RemoveSlash();
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
        base.OnEnable();
    }


    public void FaceTarget()
    {
        float enemyDistance = _goapEnemyAction.target.transform.position.x - transform.position.x;

        if (enemyDistance < 0 && _enemyFlipX == false)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            _enemyFlipX = true;
        }
        else if (enemyDistance > 0 && _enemyFlipX == true)
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
        NpcAnimator.SetBool("enemyRun", false);
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
            NpcAttributes.brave += 1;
        }


        Debug.DrawLine(_goapEnemyAction.target.transform.position, transform.position, Color.red);
        AttemptMove<Player>(xDir, yDir, _goapEnemyAction.target.transform, transform);
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

    public void RunEnemy()
    {
        FaceTarget();
        NpcAnimator.SetBool("enemyRun", true);
        RunSpeed = _goapEnemyAction.target.transform.position.x > transform.position.x ? 2f : -2f;
        rb2D.velocity = new Vector2(RunSpeed, rb2D.velocity.y);
        if (!_checkingIsInCombatRangeWhileRunning && gameObject.activeInHierarchy)
        {
            StartCoroutine("CheckIsInCombatRangeWhileRunning");
        }
    }

    public void JumpAttack()
    {
        rb2D.velocity = new Vector2(_enemyFlipX ? -2f : 2f, 8f);
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
            rb2D.velocity = new Vector2(rb2D.velocity.x, -10f);
            yield return new WaitForFixedUpdate();
        }
        rb2D.velocity = new Vector2(0, 0);
        NpcAnimator.SetFloat("enemyAttackJumpVertical", 0);
        transform.position = new  Vector2(transform.position.x, 0f);
    }

    protected IEnumerator CheckIsInCombatRangeWhileRunning()
    {
        _checkingIsInCombatRangeWhileRunning = true;
        int checkCount = 0;
        while (_checkingIsInCombatRangeWhileRunning)
        {
            checkCount++;
            if (IsInCombatRange())
            {
                StopEnemyVelocity();
                _checkingIsInCombatRangeWhileRunning = false;
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        NpcAnimator.SetTrigger("enemyAttackOne");
    }

    public void Taunt()
    {
        NpcAnimator.SetTrigger("enemyTaunt");
    }

    public void Block(bool state)
    {
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
        NpcAttributes.health = 0;
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
        StartCoroutine(WaitToRespawn(1f));
        StartCoroutine(Utilities.FadeOut(spriteRenderer, 1f));
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
        NpcAttributes.health = 100;
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
            NpcAttributes.health -= 50;
            if (NpcAttributes.health > 0)
            {
                EnemyHit();
            }
            else if (!IsDead)
            {
                EnemyDie();
            }
        }
    }

    private void EnemyHit()
    {
        StopEnemyVelocity();
        GetBloodEffect("Blood", "BloodEffect1");
        NpcAnimator.SetTrigger("enemyHit");
        IsHit = true;
        WaitFor(EnemyHitEventHandler, 2f);
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

    void OnDisable()
    {
        IsAttacking = false;
        IsTaunting = false;
        IsDead = false;
        IsCelebrating = false;
        IsHit = false;
    }
}