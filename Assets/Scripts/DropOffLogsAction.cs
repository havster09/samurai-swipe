
using System;
using UnityEngine;

public class DropOffLogsAction: GoapAction
{
	private bool droppedOffLogs = false;
	private NpcHeroAttributesComponent targetNpcHeroAttributes; // where we drop off the logs
	
	public DropOffLogsAction () {
		addPrecondition ("hasLogs", true); // can't drop off logs if we don't already have some
		addEffect ("hasLogs", false); // we now have no logs
		addEffect ("collectLogs", true); // we collected logs
	}
	
	
	public override void reset ()
	{
		droppedOffLogs = false;
		targetNpcHeroAttributes = null;
	}
	
	public override bool isDone ()
	{
		return droppedOffLogs;
	}
	
	public override bool requiresInRange ()
	{
		return true; // yes we need to be near a supply pile so we can drop off the logs
	}
	
	public override bool checkProceduralPrecondition (GameObject agent)
	{
		// find the nearest supply pile
		NpcHeroAttributesComponent[] npcHeroAttributeses = (NpcHeroAttributesComponent[]) UnityEngine.GameObject.FindObjectsOfType ( typeof(NpcHeroAttributesComponent) );
		NpcHeroAttributesComponent closest = null;
		float closestDist = 0;
		
		foreach (NpcHeroAttributesComponent supply in npcHeroAttributeses) {
			if (closest == null) {
				// first one, so choose it for now
				closest = supply;
				closestDist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
			} else {
				// is this one closer than the last?
				float dist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
				if (dist < closestDist) {
					// we found a closer one, use it
					closest = supply;
					closestDist = dist;
				}
			}
		}
		if (closest == null)
			return false;

		targetNpcHeroAttributes = closest;
		target = targetNpcHeroAttributes.gameObject;
		
		return closest != null;
	}
	
	public override bool perform (GameObject agent)
	{
		BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
		// targetNpcHeroAttributes.numLogs += backpack.numLogs;
		droppedOffLogs = true;
		backpack.numLogs = 0;
		
		return true;
	}
}
