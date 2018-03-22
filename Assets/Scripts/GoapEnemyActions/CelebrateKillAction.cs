using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CelebrateKillAction : GoapEnemyAction
{
    private bool _npcHasCelebrated = false;

    public CelebrateKillAction()
    {
        addPrecondition("enemyWin", false);
        addPrecondition("destroyNpc", true);
        addEffect("enemyWin", true);
    }

    void OnEnable()
    {
        reset();
        if (EnemyScript == null)
        {
            EnemyScript = GetComponent<Enemy>();
        }
    }


    public override bool Move()
    {
        return true;
    }

    public override void reset()
    {
        _npcHasCelebrated = false;
        TargetNpcHeroAttribute = null;
    }

    public override bool isDone()
    {
        return _npcHasCelebrated;
    }

    public override bool requiresInRange()
    {
        return false;
    }

    public override bool perform(GameObject agent)
    {
        EnemyScript.NpcCelebrate();
        _npcHasCelebrated = true;
        return _npcHasCelebrated;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        return true;
    }
}