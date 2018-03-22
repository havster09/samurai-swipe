using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChargeDestroyAction : GoapEnemyAction
{
    public ChargeDestroyAction()
    {
        addPrecondition("hasBrave", true);
        addPrecondition("destroyNpc", false);
        addEffect("destroyNpc", true);
    }

    public override void reset()
    {
        _npcIsDestroyed = false;
        _targetNpcHeroAttribute = null;
    }

    public override bool isDone()
    {
        return _npcIsDestroyed;
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
            if (!_enemyScript.IsAnimationPlaying("attack"))
            {
                _enemyScript.Attack("enemyAttackTwo");
                _targetNpcHeroAttribute.health -= 1;
            }
            _npcAttributes.attackCount += 1;
            if (_targetNpcHeroAttribute.health < 1)
            {
                _npcIsDestroyed = true;
                _npcAttributes.killCount += 1;
            }
        }
        return _npcIsDestroyed;
    }
}