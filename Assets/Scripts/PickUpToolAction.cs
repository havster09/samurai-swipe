using System;
using UnityEngine;

public class PickUpToolAction : GoapAction
{
    private bool hasTool = false;
    private NpcHeroAttributesComponent targetNpcHeroAttributes; // where we get the tool from

    public PickUpToolAction()
    {
        // addPrecondition("hasTool", false); // don't get a tool if we already have one
        addEffect("collectLogs", true); // we now have a tool
    }


    public override void reset()
    {
        hasTool = false;
        targetNpcHeroAttributes = null;
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
        NpcHeroAttributesComponent[] npcHeroAttributeses = (NpcHeroAttributesComponent[])UnityEngine.GameObject.FindObjectsOfType(typeof(NpcHeroAttributesComponent));
        NpcHeroAttributesComponent closest = null;
        float closestDist = 0;

        foreach (NpcHeroAttributesComponent supply in npcHeroAttributeses)
        {
            if (supply.health > 0)
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

        targetNpcHeroAttributes = closest;
        target = targetNpcHeroAttributes.gameObject;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (targetNpcHeroAttributes.health > 0)
        {
            targetNpcHeroAttributes.health -= 1;
            hasTool = true;

            // create the tool and add it to the agent

            NpcExperienceComponent npcExperience = (NpcExperienceComponent)agent.GetComponent(typeof(NpcExperienceComponent));
            //GameObject prefab = Resources.Load<GameObject>(NpcExperience.toolType);
            //GameObject tool = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
            //NpcExperience.tool = tool;
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


