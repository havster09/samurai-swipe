using UnityEngine;
using System.Collections;

public class Enemy : MovingObject
{
    public int playerDamage;
    public float runSpeed;

    public bool hasWalkAbility;

    private Animator animator;
    private Transform target;
    private bool skipMove;
    private bool flipX;
    private bool checkingIsInCombatRangeWhileRunning;

    //Start overrides the virtual Start function of the base class.
    protected override void Start()
    {
        GameManager.instance.AddEnemyToList(this);

        //Get and store a reference to the attached Animator component.
        animator = GetComponent<Animator>();

        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
    }


    public void FaceTarget()
    {
        float enemyDistance = target.position.x - transform.position.x;

        if (enemyDistance < 0 && flipX == false)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            flipX = true;
        }
        else if (enemyDistance > 0 && flipX == true)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            flipX = false;
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
            Debug.Log("is Attacking");
            xDir = 0;
            yDir = 0;
        }   

        //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player

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

    public void RunStopEnemy()
    {
        animator.SetBool("enemyRun", false);
        rb2D.velocity = new Vector2(0f, rb2D.velocity.y);
    }

    public void RunEnemy()
    {
        animator.SetBool("enemyRun", true);
        runSpeed = target.position.x > transform.position.x ? 2f : -2f;
        rb2D.velocity = new Vector2(runSpeed, rb2D.velocity.y);
        if (!checkingIsInCombatRangeWhileRunning)
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
            Debug.Log("check in combat range check" + checkCount);
            if (IsInCombatRange())
            {
                RunStopEnemy();
                Debug.Log("check in combat range total " + checkCount);
                checkingIsInCombatRangeWhileRunning = false;
                yield return null;
            }
            yield return new WaitForSeconds(1f);
        }        
    }

    protected override void OnCantMove<T>(T component)
    {
        //Declare hitPlayer and set it to equal the encountered component.
        // Player hitPlayer = component as Player;

        //Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
        // hitPlayer.LoseFood(playerDamage);
        animator.SetTrigger("enemyAttackOne");
    }

    public void Attack()
    {
        animator.SetTrigger("enemyAttackOne");
    }

    public bool IsAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("attack");
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
        return animator.GetCurrentAnimatorStateInfo(0).Equals(null) || animator.GetCurrentAnimatorStateInfo(0).IsTag("idle");
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            Debug.Log("is colliding with player");
            RunStopEnemy();
            gameObject.SetActive(false);
            GameManager.RecycleEnemy();
        }
    }
}