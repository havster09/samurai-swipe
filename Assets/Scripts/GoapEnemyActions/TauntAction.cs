using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TauntAction : GoapEnemyAction
{
    private bool _npcHasTaunted;
    public TauntAction()
    {
        addEffect("getBrave", true);
        _distanceToTargetThreshold = 2;
    }

    public override void reset()
    {
        _npcHasTaunted = false;
        _targetNpcHeroAttribute = null;
    }

    public override bool isDone()
    {
        return _npcHasTaunted;
    }

    public override bool requiresInRange()
    {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        return FindNpcTarget(agent);
    }

    public override bool perform(GameObject agent)
    {
        if (_targetNpcHeroAttribute != null)
        {
            if (!_enemyScript.IsAnimationPlaying("taunt"))
            {
                _enemyScript.Taunt();
                _npcHasTaunted = true;
            }            
        }
        return _npcHasTaunted;
    }
}