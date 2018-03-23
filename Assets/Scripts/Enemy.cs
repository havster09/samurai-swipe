using UnityEngine;
using System.Collections;

public class Enemy : MovingObject
{
    public int _playerDamage;
    public float _runSpeed;

    public bool _hasWalkAbility;

    public Animator _animator;
    private Transform _target;
    private bool _enemyFlipX;
    private bool _checkingIsInCombatRangeWhileRunning;
    private GameObject headFromPool;
    private NpcHeroAttributesComponent _npcHeroAttributesComponent;
    private SlashRenderer _slashRenderer;
    public NpcAttributesComponent _npcAttributes;


    public bool _isAttacking { get; set; }
    public bool IsDead { get; set; }
    public bool CanWalk = true;

    void Awake()
    {
        _npcAttributes = GetComponent<NpcAttributesComponent>();
        _npcHeroAttributesComponent = GetComponent<NpcHeroAttributesComponent>();
        _slashRenderer = GameObject.FindObjectOfType<SlashRenderer>();
        _npcAttributes = gameObject.GetComponent<NpcAttributesComponent>();
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _animator = GetComponent<Animator>();
        AttachAnimationClipEvents();
    }

    private void AttachAnimationClipEvents()
    {
        AnimationClip attackOneClip;
        AnimationEvent attackOneEvent;
        attackOneEvent = new AnimationEvent();
        attackOneClip = _animator.runtimeAnimatorController.animationClips[1];
        attackOneEvent.time = attackOneClip.length;
        attackOneEvent.stringParameter = "attackOneEvent end";
        attackOneEvent.functionName = "EnemyAttackOneEventHandler";
        attackOneClip.AddEvent(attackOneEvent);

        AnimationClip attackTwoClip;
        AnimationEvent attackTwoEvent;
        attackTwoEvent = new AnimationEvent();
        attackTwoClip = _animator.runtimeAnimatorController.animationClips[12];
        attackTwoEvent.time = attackTwoClip.length;
        attackTwoEvent.stringParameter = "attackTwoEvent end";
        attackTwoEvent.functionName = "EnemyAttackTwoEventHandler";
        attackTwoClip.AddEvent(attackTwoEvent);

        AnimationClip hitClip;
        AnimationEvent hitEvent;
        hitEvent = new AnimationEvent();
        hitClip = _animator.runtimeAnimatorController.animationClips[7];
        hitEvent.time = hitClip.length;
        hitEvent.functionName = "EnemyHitEventHandler";
        hitClip.AddEvent(hitEvent);

        AnimationClip walkClip;
        AnimationEvent walkEvent;
        walkEvent = new AnimationEvent();
        walkClip = _animator.runtimeAnimatorController.animationClips[2];
        walkEvent.time = walkClip.length;
        walkEvent.functionName = "EnemyWalkEventHandler";
        walkClip.AddEvent(walkEvent);

        AnimationClip walkBackClip;
        AnimationEvent walkBackEvent;
        walkBackEvent = new AnimationEvent();
        walkBackClip = _animator.runtimeAnimatorController.animationClips[3];
        walkBackEvent.time = walkBackClip.length;
        walkBackEvent.functionName = "EnemyWalkBackEventHandler";
        walkBackClip.AddEvent(walkBackEvent);
    }

    private void EnemyAttackOneEventHandler(string stringParameter)
    {
        _isAttacking = false;
        canMoveInSmoothMovement = true;
        Debug.Log(stringParameter);
    }

    private void EnemyAttackTwoEventHandler(string stringParameter)
    {
        _isAttacking = false;
        canMoveInSmoothMovement = true;
        Debug.Log(stringParameter);
    }

    private void EnemyHitEventHandler()
    {
        canMoveInSmoothMovement = true;
        _slashRenderer.RemoveSlash();
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
        if (Utilities.ReplaceClone(name) != "Jubei")
        {
            // GameManager.instance.AddEnemyToList(this);
        }
        base.OnEnable();
    }


    public void FaceTarget()
    {
        float enemyDistance = _target.position.x - transform.position.x;

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
        _animator.SetBool("enemyRun", false);
        if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("attack"))
        {
            Debug.Log("is Attacking");
            return;
        }

        float xDir = 0;
        float yDir = 0;
        CanWalk = false;

        bool walkBackwards = Random.Range(0, 5) < 2 && Utilities.ReplaceClone(name) != "Ukyo";
        if (Mathf.Abs(_target.position.x - transform.position.x) < float.Epsilon)
        {
            yDir = _target.position.y > transform.position.y ? 1f : -1f;
        }
        else
        {
            if (!walkBackwards)
            {
                xDir = _target.position.x > transform.position.x ? .5f : -.5f;
                _animator.SetTrigger("enemyWalk");
            }
            else
            {
                xDir = _target.position.x > transform.position.x ? -.5f : .5f;
                _animator.SetTrigger("enemyWalkBack");
                _npcAttributes.brave += 1;
            }
        }


        if (IsAttacking())
        {
            canMoveInSmoothMovement = false;
        }

        Debug.DrawLine(_target.position, transform.position, Color.red);
        AttemptMove<Player>(xDir, yDir, _target, transform);
    }

    public bool IsInWalkRange()
    {
        return Mathf.Abs(Vector3.Distance(_target.transform.position, transform.position)) < maxWalkRange;
    }

    public bool IsInCombatRange()
    {
        return Mathf.Abs(Vector3.Distance(_target.transform.position, transform.position)) < maxCombatRange;
    }

    public void StopEnemyVelocity()
    {
        _animator.SetBool("enemyRun", false);
        rb2D.velocity = new Vector2(0f, rb2D.velocity.y);
    }

    public void RunEnemy()
    {
        FaceTarget();
        _animator.SetBool("enemyRun", true);
        _runSpeed = _target.position.x > transform.position.x ? 2f : -2f;
        rb2D.velocity = new Vector2(_runSpeed, rb2D.velocity.y);
        if (!_checkingIsInCombatRangeWhileRunning && gameObject.activeInHierarchy)
        {
            StartCoroutine("CheckIsInCombatRangeWhileRunning");
        }
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
        _animator.SetTrigger("enemyAttackOne");
    }

    public void Taunt(bool state)
    {
        _animator.SetBool("enemyTaunting", state);
    }

    public void Block(bool state)
    {
        _animator.SetBool("enemyBlock", state);
    }

    public void AttackGrounded()
    {
        _animator.SetTrigger("enemyAttackGrounded");
    }

    public void Attack(string attackType)
    {
        _isAttacking = true;
        canMoveInSmoothMovement = false;
        _animator.SetTrigger(attackType);
    }

    public void EnemyDie()
    {
        canMoveInSmoothMovement = false;
        StopEnemyVelocity();

        EnemySpray();

        _npcAttributes.health = 0;
        IsDead = true;
    }

    private void EnemySpray()
    {
        canMoveInSmoothMovement = false;
        _animator.SetFloat("enemyHitSpeedMultiplier", 0f);
        _animator.Play("enemyHit", 0, 1f);
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
        _animator.SetFloat("enemyHitSpeedMultiplier", 1f);
        int randomDeath = Random.Range(0, 5);
        if (randomDeath == 0)
        {
            // _animator.SetBool("enemyDrop", true);
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
        _animator.SetBool("enemyDieSplit", true);
        GetBloodEffect("Blood", "BloodEffectDiagonal1");
        WaitFor(() => GetBloodEffect("Blood", "BloodEffectSplit"), .1f);
    }

    private void EnemySplitDrop()
    {
        _animator.SetBool("enemySplitDrop", true);
        GetBloodEffect("Blood", "BloodEffectDiagonal1");
        WaitFor(() => GetBloodEffect("Blood", "BloodEffectSplit"), .1f);
    }

    private void EnemyDecapitation()
    {
        canMoveInSmoothMovement = false;
        _animator.SetBool("enemyDecapitationBody", true);
        Vector2 start = transform.position;
        float distance = _enemyFlipX ? .25f : -.25f;
        Vector2 end = start + new Vector2(distance, 0);
        StartCoroutine(SmoothMovement(end));
        string headString = Utilities.ReplaceClone(name) + "Head";
        headFromPool = ObjectPooler.SharedInstance.GetPooledObject("BodyPart", headString);
        int randomDecapitationIndex = Random.Range(0, ObjectPooler.SharedInstance.bloodDecapitationEffects.Length);
        GetBloodEffect("BloodDecapitation",
            ObjectPooler.SharedInstance.bloodDecapitationEffects[randomDecapitationIndex]);
        if (headFromPool)
        {
            headFromPool.transform.position = transform.position;
            headFromPool.transform.rotation = transform.rotation;
            headFromPool.SetActive(true);
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
        StartCoroutine(WaitToRespawn(3f));
        StartCoroutine(Utilities.FadeOut(spriteRenderer, 3f));
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
        _npcAttributes.health = 100;
        IsDead = false;
        canMoveInSmoothMovement = true;
        GameManager.RespawnEnemyFromPool();
    }

    public void NpcCelebrate()
    {
        _animator.SetTrigger("enemyWin");
        WaitFor(() => _animator.SetBool("enemyWinCelebrate", true), 5f);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "SlashCollider")
        {
            _npcAttributes.health -= 50;
            if (_npcAttributes.health > 0)
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
        canMoveInSmoothMovement = false;
        StopEnemyVelocity();
        GetBloodEffect("Blood", "BloodEffect1");
        _animator.SetTrigger("enemyHit");
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

    public bool IsAttacking()
    {
        return _isAttacking;
    }

    public bool IsAnimationPlaying(string animationTag)
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).Equals(null) ||
            _animator.GetCurrentAnimatorStateInfo(0).IsTag(animationTag))
        {
            return true;
        }
        return false;
    }
}