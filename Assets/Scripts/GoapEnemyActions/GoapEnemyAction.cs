using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapEnemyAction : GoapAction
{
    protected float _moveSpeed = 1;
    protected float _distanceToTargetThreshold = 1;
    protected bool _npcIsDestroyed;
    protected Enemy _enemyScript;
    protected NpcHeroAttributesComponent _targetNpcHeroAttribute;
    protected NpcAttributesComponent _npcAttributes;

    public GoapEnemyAction()
    {
        addEffect("destroyNpc", true);
    }

    void Awake()
    {
        _enemyScript = GetComponent<Enemy>();
        _npcAttributes = GetComponent<NpcAttributesComponent>();
    }

    void OnEnable()
    {
        reset();
    }

    public override bool Move()
    {
        _enemyScript.FaceTarget();
        float distanceFromTarget = Vector2.Distance(gameObject.transform.position, target.transform.position);

        if (distanceFromTarget >= _distanceToTargetThreshold)
        {
            float step = (_moveSpeed * 2) * Time.deltaTime;
            gameObject.transform.position =
                Vector3.MoveTowards(gameObject.transform.position, target.transform.position, step);
            _enemyScript._animator.SetBool("enemyRun", true);

        }
        else
        {
            _enemyScript._animator.SetBool("enemyRun", false);
            setInRange(true);
            return true;
        }
        return false;
    }


    public override void reset()
    {
        _npcIsDestroyed = false;
        _targetNpcHeroAttribute = null;
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

        _targetNpcHeroAttribute = closest;
        target = _targetNpcHeroAttribute.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (_targetNpcHeroAttribute != null)
        {
            _npcIsDestroyed = true;
        }
        return _npcIsDestroyed;
    }    
}