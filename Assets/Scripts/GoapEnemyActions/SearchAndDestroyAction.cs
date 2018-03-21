﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchAndDestroyAction : GoapAction
{
    private float _moveSpeed = 1;
    private bool _npcIsDestroyed;
    public Enemy _enemyScript;
    private NpcHeroAttributesComponent _targetNpcHeroAttribute;
    private NpcAttributesComponent _npcAttributes;

    public SearchAndDestroyAction()
    {
        addPrecondition("hasBrave", false);
        addPrecondition("destroyNpc", false);
        addEffect("destroyNpc", true);
    }

    void Awake()
    {
        reset();
        _enemyScript = GetComponent<Enemy>();
        _npcAttributes = GetComponent<NpcAttributesComponent>();
    }


    public override bool Move()
    {
        _enemyScript.FaceTarget();
        float distanceFromTarget = Vector2.Distance(gameObject.transform.position, target.transform.position);
        if (distanceFromTarget > 2 &&
            !_enemyScript.IsAnimationPlaying("attack") &&
            !_enemyScript.IsAnimationPlaying("walk") && _enemyScript.canWalk)
        {
            GoapAgentSearchAndDestroyRun();
            return false;
        }
        else
        {
            _enemyScript._animator.SetBool("enemyRun", false);
            if (distanceFromTarget < 1)
            {
                setInRange(true);
                return true;
            }
            else if (_enemyScript.IsAnimationPlaying("idle") && _enemyScript.canWalk)
            {
                _enemyScript.MoveEnemy();
                return false;
            }
        }
        return false;
    }

    protected void GoapAgentSearchAndDestroyRun()
    {
        float step = (_moveSpeed * 2) * Time.deltaTime;
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, target.transform.position, step);
        _enemyScript._animator.SetBool("enemyRun", true);
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
            if (_targetNpcHeroAttribute.health > 0 && !_enemyScript.IsAnimationPlaying("attack"))
            {
                _enemyScript.Attack("enemyAttackOne");
                _targetNpcHeroAttribute.health -= 1;
            }            
            _npcAttributes.attackCount += 1;
            if (_targetNpcHeroAttribute.health < 1)
            {
                _npcIsDestroyed = true;
                _npcAttributes.killCount += 1;
            }
        }
        return _npcIsDestroyed;
    }
}