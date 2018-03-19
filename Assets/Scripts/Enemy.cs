using UnityEngine;
using System.Collections;

public class Enemy : MovingObject
{
    public int playerDamage;
    public float runSpeed;

    public int health = 100;

    public bool hasWalkAbility;

    public Animator animator;
    private Transform target;
    private bool enemyFlipX;
    private bool checkingIsInCombatRangeWhileRunning;
    private GameObject headFromPool;
    private NpcHeroAttributesComponent npcHeroAttributesComponent;

    public bool isAttacking { get; private set; }
    public bool isDead { get; set; }
    public bool canWalk;

    private void EnemyAttackOneEventHandler(string stringParameter)
    {
        isAttacking = false;
        canMoveInSmoothMovement = true;
    }

    private void EnemyHitEventHandler()
    {
        canMoveInSmoothMovement = true;
    }

    private void EnemyWalkEventHandler()
    {
        WaitFor(() => canWalk = true, 3f);
    }

    private void EnemyWalkBackEventHandler()
    {
        WaitFor(() => canWalk = true, 4f);
    }

    protected override void OnEnable()
    {
        if (Utilities.ReplaceClone(name) != "Jubei")
        {
            GameManager.instance.AddEnemyToList(this);
        }

        if (npcHeroAttributesComponent == null)
        {
            npcHeroAttributesComponent = GetComponent<NpcHeroAttributesComponent>();
        }

        target = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();

        AnimationClip attackOneClip;
        AnimationEvent attackOneEvent;
        attackOneEvent = new AnimationEvent();
        attackOneClip = animator.runtimeAnimatorController.animationClips[1];
        attackOneEvent.time = attackOneClip.length;
        attackOneEvent.stringParameter = "end";
        attackOneEvent.functionName = "EnemyAttackOneEventHandler";
        attackOneClip.AddEvent(attackOneEvent);

        AnimationClip hitClip;
        AnimationEvent hitEvent;
        hitEvent = new AnimationEvent();
        hitClip = animator.runtimeAnimatorController.animationClips[7];
        hitEvent.time = hitClip.length;
        hitEvent.functionName = "EnemyHitEventHandler";
        hitClip.AddEvent(hitEvent);

        AnimationClip walkClip;
        AnimationEvent walkEvent;
        walkEvent = new AnimationEvent();
        walkClip = animator.runtimeAnimatorController.animationClips[2];
        walkEvent.time = walkClip.length;
        walkEvent.functionName = "EnemyWalkEventHandler";
        walkClip.AddEvent(walkEvent);

        AnimationClip walkBackClip;
        AnimationEvent walkBackEvent;
        walkBackEvent = new AnimationEvent();
        walkBackClip = animator.runtimeAnimatorController.animationClips[3];
        walkBackEvent.time = walkBackClip.length;
        walkBackEvent.functionName = "EnemyWalkBackEventHandler";
        walkBackClip.AddEvent(walkBackEvent);

        base.OnEnable();
    }


    public void FaceTarget()
    {
        float enemyDistance = target.position.x - transform.position.x;

        if (enemyDistance < 0 && enemyFlipX == false)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            enemyFlipX = true;
        }
        else if (enemyDistance > 0 && enemyFlipX == true)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            enemyFlipX = false;
        }
    }

    protected override void AttemptMove<T>(float xDir, float yDir, Transform target, Transform movingObject)
    {
        base.AttemptMove<T>(xDir, yDir, target, movingObject);
    }

    public void MoveEnemy()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("attack"))
        {
            Debug.Log("is Attacking");
            return;
        }

        float xDir = 0;
        float yDir = 0;
        canWalk = false;

        bool walkBackwards = Random.Range(0, 5) < 2 && Utilities.ReplaceClone(name) != "Ukyo";
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
        {
            yDir = target.position.y > transform.position.y ? 1f : -1f;
        }
        else
        {
            if (!walkBackwards)
            {
                xDir = target.position.x > transform.position.x ? .5f : -.5f;
                animator.SetTrigger("enemyWalk");
            }
            else
            {
                xDir = target.position.x > transform.position.x ? -.5f : .5f;
                animator.SetTrigger("enemyWalkBack");
            }
        }


        if (IsAttacking())
        {
            canMoveInSmoothMovement = false;
        }

        Debug.DrawLine(target.position, transform.position, Color.red);
        AttemptMove<Player>(xDir, yDir, target, transform);
    }

    public bool IsInWalkRange()
    {
        return Mathf.Abs(Vector3.Distance(target.transform.position, transform.position)) < maxWalkRange;
    }

    public bool IsInCombatRange()
    {
        return Mathf.Abs(Vector3.Distance(target.transform.position, transform.position)) < maxCombatRange;
    }

    public void StopEnemyVelocity()
    {
        animator.SetBool("enemyRun", false);
        rb2D.velocity = new Vector2(0f, rb2D.velocity.y);
    }

    public void RunEnemy()
    {
        FaceTarget();
        animator.SetBool("enemyRun", true);
        runSpeed = target.position.x > transform.position.x ? 2f : -2f;
        rb2D.velocity = new Vector2(runSpeed, rb2D.velocity.y);
        if (!checkingIsInCombatRangeWhileRunning && gameObject.activeInHierarchy)
        {
            StartCoroutine("CheckIsInCombatRangeWhileRunning");
        }
    }

    protected IEnumerator CheckIsInCombatRangeWhileRunning()
    {
        checkingIsInCombatRangeWhileRunning = true;
        int checkCount = 0;
        while (checkingIsInCombatRangeWhileRunning)
        {
            checkCount++;
            if (IsInCombatRange())
            {
                StopEnemyVelocity();
                checkingIsInCombatRangeWhileRunning = false;
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        animator.SetTrigger("enemyAttackOne");
    }

    public void Attack()
    {
        isAttacking = true;
        canMoveInSmoothMovement = false;
        animator.SetTrigger("enemyAttackOne");
    }

    public void EnemyDie()
    {
        canMoveInSmoothMovement = false;
        StopEnemyVelocity();

        EnemySpray();

        health = 0;
        isDead = true;
    }

    private void EnemySpray()
    {
        canMoveInSmoothMovement = false;
        animator.SetFloat("enemyHitSpeedMultiplier", 0f);
        animator.Play("enemyHit", 0, 1f);
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
        animator.SetFloat("enemyHitSpeedMultiplier", 1f);
        int randomDeath = Random.Range(0, 5);
        if (randomDeath == 0)
        {
            // animator.SetBool("enemyDrop", true);
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
        animator.SetBool("enemyDieSplit", true);
        GetBloodEffect("Blood", "BloodEffectDiagonal1");
        WaitFor(() => GetBloodEffect("Blood", "BloodEffectSplit"), .1f);
    }

    private void EnemySplitDrop()
    {
        animator.SetBool("enemySplitDrop", true);
        GetBloodEffect("Blood", "BloodEffectDiagonal1");
        WaitFor(() => GetBloodEffect("Blood", "BloodEffectSplit"), .1f);
    }

    private void EnemyDecapitation()
    {
        canMoveInSmoothMovement = false;
        animator.SetBool("enemyDecapitationBody", true);
        Vector2 start = transform.position;
        float distance = enemyFlipX ? .25f : -.25f;
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
        health = 100;
        isDead = false;
        canMoveInSmoothMovement = true;
        GameManager.RespawnEnemyFromPool();
    }

    public void NpcCelebrate()
    {
        animator.SetTrigger("enemyWin");
        WaitFor(() => animator.SetBool("enemyWinCelebrate", true), 5f);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            health -= 50;
            if (health > 0)
            {
                EnemyHit();
            }
            else if (!isDead)
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
        animator.SetTrigger("enemyHit");
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
        return isAttacking;
    }

    public bool IsAnimationPlaying(string animationTag)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).Equals(null) ||
            animator.GetCurrentAnimatorStateInfo(0).IsTag(animationTag))
        {
            return true;
        }
        return false;
    }
}