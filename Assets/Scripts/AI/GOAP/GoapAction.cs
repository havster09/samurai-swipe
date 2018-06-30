
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GoapAttributeComponents;
using UnityEngine;

public abstract class GoapAction : MonoBehaviour {


	private HashSet<KeyValuePair<string,object>> preconditions;
	private HashSet<KeyValuePair<string,object>> effects;

	protected bool InRange;
    protected float TotalMovementDistance;

	/* The cost of performing the action. 
	 * Figure out a weight that suits the action. 
	 * Changing it will affect what actions are chosen during planning.*/
	public float cost = 1f;

	/**
	 * An action often has to perform on an object. This is that object. Can be null. */
	public GameObject target;

    public GoapAction() {
		preconditions = new HashSet<KeyValuePair<string, object>> ();
		effects = new HashSet<KeyValuePair<string, object>> ();
	}

	public virtual void DoReset() {
		InRange = false;
		target = null;
		reset ();
	}

    public abstract bool Move();

	/**
	 * Reset any variables that need to be reset before planning happens again.
	 */
	public abstract void reset();

	/**
	 * Is the action done?
	 */
	public abstract bool isDone();

	/**
	 * Procedurally check if this action can run. Not all actions
	 * will need this, but some might.
	 */
	public abstract bool checkProceduralPrecondition(GameObject agent);

	/**
	 * Run the action.
	 * Returns True if the action performed successfully or false
	 * if something happened and it can no longer perform. In this case
	 * the action queue should clear out and the goal cannot be reached.
	 */
	public abstract bool perform(GameObject agent);

	/**
	 * Does this action need to be within range of a target game object?
	 * If not then the moveTo state will not need to run for this action.
	 */
	public abstract bool requiresInRange ();
	

	/**
	 * Are we in range of the target?
	 * The MoveTo state will set this and it gets reset each time this action is performed.
	 */
	public bool isInRange () {
		return InRange;
	}
	
	public void setInRange(bool inRange) {
		this.InRange = inRange;
	}


	public void addPrecondition(string key, object value) {
		preconditions.Add (new KeyValuePair<string, object>(key, value) );
	}


	public void removePrecondition(string key) {
		KeyValuePair<string, object> remove = default(KeyValuePair<string,object>);
		foreach (KeyValuePair<string, object> kvp in preconditions) {
			if (kvp.Key.Equals (key)) 
				remove = kvp;
		}
		if ( !default(KeyValuePair<string,object>).Equals(remove) )
			preconditions.Remove (remove);
	}


	public void addEffect(string key, object value) {
		effects.Add (new KeyValuePair<string, object>(key, value) );
	}


	public void removeEffect(string key) {
		KeyValuePair<string, object> remove = default(KeyValuePair<string,object>);
		foreach (KeyValuePair<string, object> kvp in effects) {
			if (kvp.Key.Equals (key)) 
				remove = kvp;
		}
		if ( !default(KeyValuePair<string,object>).Equals(remove) )
			effects.Remove (remove);
	}

	
	public HashSet<KeyValuePair<string, object>> Preconditions {
		get {
			return preconditions;
		}
	}

	public HashSet<KeyValuePair<string, object>> Effects {
		get {
			return effects;
		}
	}

    public NpcAttributesComponent[] GetActiveNpcAttributesComponents()
    {
        var totalNpc = FindObjectsOfType<NpcAttributesComponent>()
            .Where(npc => npc.Health > 0 && npc.isActiveAndEnabled)
            .ToArray();

        return totalNpc;
    }

    public NpcAttributesComponent[] GetActiveNpcAttributesComponentsInRange(GameObject from, float threshold)
    {
        var totalNpc = FindObjectsOfType<NpcAttributesComponent>()
            .Where(npc => npc.Health > 0 &&
                          npc.isActiveAndEnabled &&
                          Vector2.Distance(from.transform.position, npc.transform.position) < threshold)
            .ToArray();

        return totalNpc;
    }

    public NpcAttributesComponent[] GetActiveNpcAttributesComponentsInRangeByDirection(GameObject from, float threshold = 5f)
    {
        var directionRight = from.transform.position.x > 0;
        var totalNpc = FindObjectsOfType<NpcAttributesComponent>()
            .Where(npc => npc.Health > 0 &&
                          npc.isActiveAndEnabled &&
                          Vector2.Distance(npc.transform.position, from.transform.position) < threshold &&
                              (
                                npc.transform.position.x > from.transform.position.x && directionRight || 
                                npc.transform.position.x < from.transform.position.x && !directionRight
                              )
                          )
            .ToArray();

        return totalNpc;
    }
}