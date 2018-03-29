using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.GoapEnemyTypes;

public class GoapAgentSearchAndDestroy : GoapAgentEnemy
{
    public override HashSet<KeyValuePair<string, object>> createGoalState()
    {
        HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
        goal.Add(new KeyValuePair<string, object>("destroyNpc", true));
        goal.Add(new KeyValuePair<string, object>("enemyWin", true));
        return goal;
    }
}


