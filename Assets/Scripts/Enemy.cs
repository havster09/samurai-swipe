﻿using UnityEngine;
using System.Collections;

public class Enemy : MovingObject
{
    public int playerDamage;
    public float runSpeed;

    public int health = 100;

    public bool hasWalkAbility;

    private Animator animator;
    private Transform target;
    private bool skipMove;
    private bool enemyFlipX;
    private bool checkingIsInCombatRangeWhileRunning;
    private GameObject headFromPool;

    public bool isAttacking { get; private set; }
    public bool isDead { get; set; }

    private void EnemyAttackOneEventHandler(string stringParameter)
    {
        Debug.Log(stringParameter);
        isAttacking = false;
        canMoveInSmoothMovement = true;
    }

    private void EnemyHitEventHandler()
    {
        canMoveInSmoothMovement = true;
    }

    protected override void OnEnable()
    {
        GameManager.instance.AddEnemyToList(this);
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

        bool walkBackwards = Random.Range(0, 5) < 2;

        //If the difference in positions is approximately zero (Epsilon) do the following:
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
        {
            //If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
            yDir = target.position.y > transform.position.y ? 1f : -1f;
        }
        //If the difference in positions is not approximately zero (Epsilon) do the following:
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

        // Debug.DrawLine(target.position, transform.position, Color.red);

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
        Debug.Log("check in combat range init");
        checkingIsInCombatRangeWhileRunning = true;
        int checkCount = 0;
        while(checkingIsInCombatRangeWhileRunning)
        {
            checkCount++;
            if (IsInCombatRange())
            {
                StopEnemyVelocity();
                Debug.Log("check in combat range total " + checkCount);
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
        // StartCoroutine("CheckAttackFrame");
    }

    protected IEnumerator CheckAttackFrame()
    {
        float frameNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        while(frameNormalizedTime < 1.0)
        {
            Debug.Log(frameNormalizedTime + "attack process");
            yield return new WaitForSeconds(.1f);
        }
        Debug.Log(frameNormalizedTime + "attack complete");
        yield return null;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public bool IsWalking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("walk");
    }
    public bool IsRunning()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("run");
    }
    public bool IsIdle()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).Equals(null) || animator.GetCurrentAnimatorStateInfo(0).IsTag("idle"))
        {
            return true;
        }
        return false;
    }

    public void EnemyDie()
    {
        canMoveInSmoothMovement = false;
        StopEnemyVelocity();

        // animator.SetBool("enemyDieSplit", true);
        enemyDecapitation();

        health = 0;
        isDead = true;
        StartCoroutine(WaitToRespawn(5));
    }

    private void enemyDecapitation()
    {
        canMoveInSmoothMovement = false;
        animator.SetBool("enemyDecapitationBody", true);
        Vector2 start = transform.position;
        float distance = enemyFlipX ? .25f : -.25f;
        Vector2 end = start + new Vector2(distance, 0);
        StartCoroutine(SmoothMovement(end));
        string headString = name.Replace("(Clone)", "") + "Head";
        headFromPool = ObjectPooler.SharedInstance.GetPooledObject(headString);
        GetBloodEffect("BloodDecapitation");
        if (headFromPool)
        {
            headFromPool.transform.position = transform.position;
            headFromPool.transform.rotation = transform.rotation;
            headFromPool.SetActive(true);
        }
    }

    protected IEnumerator WaitToRespawn(int respawnTime)
    {
        int elapsedDeathTime = 0;
        while (elapsedDeathTime < respawnTime)
        {
            yield return new WaitForSeconds(1f);
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

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            health -= 50;
            if (health > 0)
            {
                EnemyHit();
            }
            else if(!isDead)
            {
                EnemyDie();
            }
        }
    }

   private void EnemyHit()
    {
        canMoveInSmoothMovement = false;
        StopEnemyVelocity();
        GetBloodEffect("Blood");
        animator.SetTrigger("enemyHit");
    }

    private void GetBloodEffect(string tag)
    {
        GameObject bloodFromPool = ObjectPooler.SharedInstance.GetPooledObject(tag);
        if (bloodFromPool)
        {
            bloodFromPool.transform.position = transform.position;
            bloodFromPool.transform.rotation = transform.rotation;
            bloodFromPool.SetActive(true);
        }
    }
}