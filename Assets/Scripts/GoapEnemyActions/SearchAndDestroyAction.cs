using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchAndDestroyAction : GoapAction
{
    private bool hasTool = false;
    public Enemy enemyScript;
    private NpcAttributesComponent targetNpcAttributes; 

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
        hasTool = false;
        targetNpcAttributes = null;
    }

    public override bool isDone()
    {
        return hasTool;
    }

    public override bool requiresInRange()
    {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        NpcAttributesComponent[] npcAttributeses = (NpcAttributesComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(NpcAttributesComponent));
        NpcAttributesComponent closest = null;
        float closestDist = 0;

        foreach (NpcAttributesComponent npc in npcAttributeses)
        {
            if (npc.numTools > 0)
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
            

        targetNpcAttributes = closest;
        target = targetNpcAttributes.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (targetNpcAttributes.health > 0)
            {
            enemyScript.Attack();
            targetNpcAttributes.health -= 1;
            
            BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            //GameObject prefab = Resources.Load<GameObject>(backpack.toolType);
            //GameObject tool = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
            //backpack.tool = tool;
            //tool.transform.parent = transform; // attach the tool

            return targetNpcAttributes.health < 1;
        }
        else
        {
            // cannot perform action npc is dead
            return false;
        }
    }

}