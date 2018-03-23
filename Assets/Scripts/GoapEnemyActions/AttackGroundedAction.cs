using UnityEngine;

public class AttackGroundedAction : GoapEnemyAction
{
    private bool _npcHasAttackGroundedAction = false;

    public AttackGroundedAction()
    {
        addPrecondition("enemyAttackGounded", false);
        addPrecondition("destroyNpc", true);
        addEffect("enemyAttackGounded", true);
        DistanceToTargetThreshold = .1f;
    }

    void OnEnable()
    {
        reset();
        if (EnemyScript == null)
        {
            EnemyScript = GetComponent<Enemy>();
        }
    }

    public override void reset()
    {
        _npcHasAttackGroundedAction = false;
        TargetNpcHeroAttribute = null;
    }

    public override bool isDone()
    {
        return _npcHasAttackGroundedAction;
    }

    public override bool requiresInRange()
    {
        return true;
    }

    public override bool perform(GameObject agent)
    {
        EnemyScript.AttackGrounded();
        _npcHasAttackGroundedAction = true;
        return _npcHasAttackGroundedAction;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        return true;
    }
}