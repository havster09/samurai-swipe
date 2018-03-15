using System;
using UnityEngine;

public class KillTargetAction : GoapAction
{
    private bool killedTarget = false;
    private GameObject target;

    private float startTime = 0;
    public float workDuration = 2; // seconds

    public KillTargetAction()
    {
        //addPrecondition("hasTool", true); // we need a tool to do this
        //addPrecondition("hasFirewood", false); // if we have firewood we don't want more
        addEffect("hasKilledTarget", true);
    }


    public override void reset()
    {
        killedTarget = false;
        // targetChoppingBlock = null;
        startTime = 0;
    }

    public override bool isDone()
    {
        return killedTarget;
    }

    public override bool requiresInRange()
    {
        return true; 
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // find the nearest chopping block that we can chop our wood at
        GameObject[] blocks = (GameObject[])UnityEngine.GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float closestDist = 0;

        foreach (GameObject block in blocks)
        {
            if (closest == null)
            {
                // first one, so choose it for now
                closest = block;
                closestDist = (block.transform.position - agent.transform.position).magnitude;
            }
            else
            {
                // is this one closer than the last?
                float dist = (block.transform.position - agent.transform.position).magnitude;
                if (dist < closestDist)
                {
                    // we found a closer one, use it
                    closest = block;
                    closestDist = dist;
                }
            }
        }
        if (closest == null)
            return false;

        target = closest;

        return closest != null;
    }

    public override bool perform(GameObject agent)
    {
        if (startTime == 0)
            startTime = Time.time;

        if (Time.time - startTime > workDuration)
        {
            // finished chopping
            //BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            //backpack.numFirewood += 5;
            //killedTarget = true;
            //ToolComponent tool = backpack.tool.GetComponent(typeof(ToolComponent)) as ToolComponent;
            //tool.use(0.34f);
            //if (tool.destroyed())
            //{
            //    Destroy(backpack.tool);
            //    backpack.tool = null;
            //}
            killedTarget = true;
        }
        return true;
    }

}
