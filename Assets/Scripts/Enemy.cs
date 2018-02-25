using UnityEngine;
using System.Collections;

public class Enemy : MovingObject
{
    public int playerDamage;

    private Animator animator;                          
    private Transform target;                         
    private bool skipMove;
    private bool flipX;
    public float RunSpeed = 10f;


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
        FaceTarget();
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

        bool walkBackwards = Random.Range(0, 2) < .5;
        
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
                xDir = target.position.x > transform.position.x ? 1f : -1f;
                animator.SetTrigger("enemyWalk");
            }
            else
            {
                xDir = target.position.x > transform.position.x ? -1f : 1f;
                animator.SetTrigger("enemyWalkBack");
            }
            
        }

        //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player

        // Debug.DrawLine(target.position, transform.position, Color.red);
        
        AttemptMove<Player>(xDir, yDir, target, transform);
        
    }

    public void RunEnemy()
    {
        Debug.Log("running");
        FaceTarget();
        animator.SetBool("enemyRun", true);
        transform.position = Vector3.MoveTowards(transform.position, target.position, RunSpeed * Time.deltaTime);
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
}