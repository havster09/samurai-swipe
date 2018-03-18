using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapAgentCelebrateKill : GoapAgentEnemy
{
    private Enemy[] enemies;

    public override HashSet<KeyValuePair<string, object>> createGoalState()
    {
        HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
        goal.Add(new KeyValuePair<string, object>("enemyWin", true));
        return goal;
    }
}



