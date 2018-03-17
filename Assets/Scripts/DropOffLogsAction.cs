
using System;
using UnityEngine;

public class DropOffLogsAction: GoapAction
{
	private bool droppedOffLogs = false;
	private NpcAttributesComponent targetNpcAttributes; // where we drop off the logs
	
	public DropOffLogsAction () {
		addPrecondition ("hasLogs", true); // can't drop off logs if we don't already have some
		addEffect ("hasLogs", false); // we now have no logs
		addEffect ("collectLogs", true); // we collected logs
	}
	
	
	public override void reset ()
	{
		droppedOffLogs = false;
		targetNpcAttributes = null;
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
		NpcAttributesComponent[] npcAttributeses = (NpcAttributesComponent[]) UnityEngine.GameObject.FindObjectsOfType ( typeof(NpcAttributesComponent) );
		NpcAttributesComponent closest = null;
		float closestDist = 0;
		
		foreach (NpcAttributesComponent supply in npcAttributeses) {
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

		targetNpcAttributes = closest;
		target = targetNpcAttributes.gameObject;
		
		return closest != null;
	}
	
	public override bool perform (GameObject agent)
	{
		BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
		targetNpcAttributes.numLogs += backpack.numLogs;
		droppedOffLogs = true;
		backpack.numLogs = 0;
		
		return true;
	}
}
