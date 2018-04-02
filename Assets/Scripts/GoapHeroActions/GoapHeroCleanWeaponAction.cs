using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroCleanWeaponAction : GoapHeroAction
    {
        public GoapHeroCleanWeaponAction()
        {
            addPrecondition("destroyEnemyNpc", true);
            addEffect("wipeBlood", true);
            DistanceToTargetThreshold = 1f;
        }

        public override void reset()
        {
            NpcIsDestroyedReset = false;
            TargetNpcAttribute = null;
        }

        public override bool isDone()
        {
            return NpcIsDestroyedReset;
        }

        public override bool requiresInRange()
        {
            return false;
            // todo check if all enemy targets are dead
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return true;
        }
        public override bool perform(GameObject agent)
        {
            if (GetActiveNpcAttributesComponentsInRange(gameObject, 4f) < 1)
            {
                HeroScript.CleanWeapon();
                NpcIsDestroyedReset = true;
            }
            else
            {
                NpcIsDestroyedReset = true;
            }

            NpcHeroAttributes.ComboCount = 0;
            return NpcIsDestroyedReset;
        }
    }
}
