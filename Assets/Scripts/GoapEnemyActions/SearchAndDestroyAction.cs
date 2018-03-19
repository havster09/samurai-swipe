using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchAndDestroyAction : GoapAction
{
    private bool npcIsDestroyed;
    public Enemy enemyScript;
    private NpcHeroAttributesComponent targetNpcHeroAttribute;
    private NpcExperienceComponent npcExperience;

    public SearchAndDestroyAction()
    {
        addPrecondition("destroyNpc", false);
        addEffect("destroyNpc", true);
    }

    void OnEnable()
    {
        reset();
        if (enemyScript == null)
        {
            enemyScript = GetComponent<Enemy>();
        }
    }


    public override void reset()
    {
        npcIsDestroyed = false;
        targetNpcHeroAttribute = null;
    }

    public override bool isDone()
    {
        return npcIsDestroyed;
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

        targetNpcHeroAttribute = closest;
        target = targetNpcHeroAttribute.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (targetNpcHeroAttribute != null)
        {
            if (targetNpcHeroAttribute.health > 0 && !enemyScript.IsAnimationPlaying("attack"))
            {
                enemyScript.Attack();
                targetNpcHeroAttribute.health -= 1;
            }
            npcExperience = (NpcExperienceComponent)agent.GetComponent(typeof(NpcExperienceComponent));
            npcExperience.attackCount += 1;
            // npcExperience.killCount += 1;
            if (targetNpcHeroAttribute.health < 1)
            {
                npcIsDestroyed = true;
            }
        }
        return npcIsDestroyed;
    }
}