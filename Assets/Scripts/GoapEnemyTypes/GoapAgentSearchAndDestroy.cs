using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapAgentSearchAndDestroy : GoapAgentEnemy
{
    private Enemy[] _enemies;

    public override HashSet<KeyValuePair<string, object>> createGoalState()
    {
        HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
        goal.Add(new KeyValuePair<string, object>("destroyNpc", true));
        goal.Add(new KeyValuePair<string, object>("enemyWin", true));
        return goal;
    }

    public int CheckActiveCount()
    {
        _enemies = (Enemy[])UnityEngine.GameObject.FindObjectsOfType(typeof(Enemy));
        _enemies.Select(e => e.gameObject.activeInHierarchy && !e.IsDead);
        return _enemies.Length;
    }

}


