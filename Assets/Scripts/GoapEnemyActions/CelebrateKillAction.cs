using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CelebrateKillAction : GoapAction
{
    private bool npcHasCelebrated = false;
    public Enemy enemyScript;
    private NpcHeroAttributesComponent targetNpcHeroAttribute;
    private NpcExperienceComponent npcExperience;

    public CelebrateKillAction()
    {
        addPrecondition("enemyWin", false);
        addEffect("enemyWin", true);
    }

    void OnEnable()
    {
        reset();
        if (enemyScript == null)
        {
            enemyScript = GetComponent<Enemy>();
        }
    }


    public override void reset()
    {
        npcHasCelebrated = false;
        targetNpcHeroAttribute = null;
    }

    public override bool isDone()
    {
        return npcHasCelebrated;
    }

    public override bool requiresInRange()
    {
        return false;
    }

    public override bool perform(GameObject agent)
    {
        enemyScript.NpcCelebrate();
        npcHasCelebrated = true;
        return npcHasCelebrated;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        return true;
    }
}