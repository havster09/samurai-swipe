using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChargeDestroyAction : GoapAction
{
    private bool _npcIsDestroyed;
    public Enemy _enemyScript;
    private NpcHeroAttributesComponent _targetNpcHeroAttribute;
    private NpcAttributesComponent _npcAttributes;

    public ChargeDestroyAction()
    {
        addPrecondition("hasBrave", true);
        addPrecondition("destroyNpc", false);
        addEffect("destroyNpc", true);
    }

    void Start()
    {
        if (_enemyScript == null)
        {
            _enemyScript = GetComponent<Enemy>();
        }
        if (_npcAttributes == null)
        {
            _npcAttributes = GetComponent<NpcAttributesComponent>();
        }
    }

    void OnEnable()
    {
        reset();
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
                _enemyScript.Attack("enemyAttackTwo");
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