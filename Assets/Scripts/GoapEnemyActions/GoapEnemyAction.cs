using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapEnemyAction : GoapAction
{
    protected float MoveSpeed = 1;
    protected float DistanceToTargetThreshold = 1;
    protected bool _npcIsDestroyed;
    protected Enemy EnemyScript;
    protected NpcHeroAttributesComponent TargetNpcHeroAttribute;
    protected NpcAttributesComponent NpcAttributes;
    protected bool IsPerforming;

    public GoapEnemyAction()
    {
        
    }

    void Awake()
    {
        EnemyScript = GetComponent<Enemy>();
        NpcAttributes = GetComponent<NpcAttributesComponent>();
    }

    void OnEnable()
    {
        reset();
    }

    public override bool Move()
    {
        if (IsPerforming)
        {
            return false;
        }

        EnemyScript.FaceTarget();
        float distanceFromTarget = Vector2.Distance(gameObject.transform.position, target.transform.position);

        if (distanceFromTarget >= DistanceToTargetThreshold)
        {
            float step = (MoveSpeed * 2) * Time.deltaTime;
            gameObject.transform.position =
                Vector3.MoveTowards(gameObject.transform.position, target.transform.position, step);
            EnemyScript._animator.SetBool("enemyRun", true);

        }
        else
        {
            EnemyScript._animator.SetBool("enemyRun", false);
            setInRange(true);
            return true;
        }
        return false;
    }


    public override void reset()
    {
        _npcIsDestroyed = false;
        TargetNpcHeroAttribute = null;
    }

    public override bool isDone()
    {
        return _npcIsDestroyed;
    }

    public override bool requiresInRange()
    {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        return FindNpcTarget(agent);
    }

    public virtual bool FindNpcTarget(GameObject agent)
    {
        NpcHeroAttributesComponent[] npcHeroAttributes = (NpcHeroAttributesComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(NpcHeroAttributesComponent));
        NpcHeroAttributesComponent closest = null;
        float closestDist = 0;

        foreach (NpcHeroAttributesComponent npc in npcHeroAttributes)
        {
            if (closest == null)
            {
                closest = npc;
                closestDist = (npc.gameObject.transform.position - agent.transform.position).magnitude;
            }
            else
            {
                float dist = (npc.gameObject.transform.position - agent.transform.position).magnitude;
                if (dist < closestDist)
                {
                    closest = npc;
                    closestDist = dist;
                }
            }
        }

        if (closest == null)
        {
            return false;
        }
        else if (closest.health < 1)
        {
            return false;
        }

        TargetNpcHeroAttribute = closest;
        target = TargetNpcHeroAttribute.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        IsPerforming = true;
        if (TargetNpcHeroAttribute != null)
        {
            _npcIsDestroyed = true;
            IsPerforming = false;
        }
        return _npcIsDestroyed;
    }    
}