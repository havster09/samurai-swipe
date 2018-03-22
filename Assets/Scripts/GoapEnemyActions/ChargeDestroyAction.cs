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
        TargetNpcHeroAttribute = null;
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
        if (TargetNpcHeroAttribute != null)
        {
            if (!EnemyScript.IsAnimationPlaying("attack"))
            {
                IsPerforming = true;
                EnemyScript.Attack("enemyAttackTwo");
                EnemyScript.WaitFor(() => IsPerforming = false, 1f);
                TargetNpcHeroAttribute.health -= 10;
            }
            NpcAttributes.attackCount += 1;
            if (TargetNpcHeroAttribute.health < 1)
            {
                _npcIsDestroyed = true;
                NpcAttributes.killCount += 1;
            }
        }
        return _npcIsDestroyed;
    }
}