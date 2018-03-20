using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CelebrateKillAction : GoapAction
{
    private bool _npcHasCelebrated = false;
    public Enemy _enemyScript;
    private NpcHeroAttributesComponent _targetNpcHeroAttribute;
    private NpcAttributesComponent _npcAttributes;

    public CelebrateKillAction()
    {
        addPrecondition("enemyWin", false);
        addEffect("enemyWin", true);
    }

    void OnEnable()
    {
        reset();
        if (_enemyScript == null)
        {
            _enemyScript = GetComponent<Enemy>();
        }
    }


    public override void reset()
    {
        _npcHasCelebrated = false;
        _targetNpcHeroAttribute = null;
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
        _enemyScript.NpcCelebrate();
        _npcHasCelebrated = true;
        return _npcHasCelebrated;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        return true;
    }
}