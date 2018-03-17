using System;
using UnityEngine;

public class PickUpToolAction : GoapAction
{
    private bool hasTool = false;
    private NpcAttributesComponent targetNpcAttributes; // where we get the tool from

    public PickUpToolAction()
    {
        // addPrecondition("hasTool", false); // don't get a tool if we already have one
        addEffect("collectLogs", true); // we now have a tool
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
        return true; // yes we need to be near a supply pile so we can pick up the tool
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // find the nearest supply pile that has spare tools
        NpcAttributesComponent[] npcAttributeses = (NpcAttributesComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(NpcAttributesComponent));
        NpcAttributesComponent closest = null;
        float closestDist = 0;

        foreach (NpcAttributesComponent supply in npcAttributeses)
        {
            if (supply.numTools > 0)
            {
                if (closest == null)
                {
                    // first one, so choose it for now
                    closest = supply;
                    closestDist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
                }
                else
                {
                    // is this one closer than the last?
                    float dist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
                    if (dist < closestDist)
                    {
                        // we found a closer one, use it
                        closest = supply;
                        closestDist = dist;
                    }
                }
            }
        }
        if (closest == null)
            return false;

        targetNpcAttributes = closest;
        target = targetNpcAttributes.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (targetNpcAttributes.numTools > 0)
        {
            targetNpcAttributes.numTools -= 1;
            hasTool = true;

            // create the tool and add it to the agent

            BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            //GameObject prefab = Resources.Load<GameObject>(backpack.toolType);
            //GameObject tool = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
            //backpack.tool = tool;
            //tool.transform.parent = transform; // attach the tool

            return true;
        }
        else
        {
            // we got there but there was no tool available! Someone got there first. Cannot perform action
            return false;
        }
    }

}


