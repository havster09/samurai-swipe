using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchAndDestroyAction : GoapAction
{
    private bool npcIsDestroyed = false;
    public Enemy enemyScript;
    private NpcHeroAttributesComponent targetNpcHeroAttribute;

    public SearchAndDestroyAction()
    {

    }

    void OnEnable()
    {
        reset();
        addEffect("destroyNpc", true);
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
            if (npc.health > 0)
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
        }
        if (closest == null)
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
            if (targetNpcHeroAttribute.health > 0)
            {
                enemyScript.Attack();
                targetNpcHeroAttribute.health -= 1;

                BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
                //GameObject prefab = Resources.Load<GameObject>(backpack.toolType);
                //GameObject tool = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
                //backpack.tool = tool;
                //tool.transform.parent = transform; // attach the tool

                return false;
            }
        }
        return false;
    }
}