using UnityEngine;

public class TauntAction : GoapEnemyAction
{
    private bool _npcHasTaunted;
    public TauntAction()
    {
        addPrecondition("hasBrave", false);
        addEffect("hasBrave", true);
        DistanceToTargetThreshold = 3;
    }

    public override void reset()
    {
        _npcHasTaunted = false;
        TargetNpcHeroAttribute = null;
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
        if (TargetNpcHeroAttribute != null)
        {
            if (!EnemyScript.IsAnimationPlaying("taunt"))
            {
                EnemyScript.Taunt(true);
                EnemyScript.WaitFor(() =>
                {
                    NpcAttributes.brave += 1;
                    EnemyScript.Taunt(false);
                }, 2f);

                if (NpcAttributes.brave > 3)
                {
                    _npcHasTaunted = true;
                }
            }            
        }
        return _npcHasTaunted;
    }
}