using Assets.Scripts.GoapAttributeComponents;
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
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            return true;
        }
        public override bool perform(GameObject agent)
        {
            if (GetActiveNpcAttributesComponentsInRange(gameObject, PoseThreshold) < 1 && InResetRange())
            {
                Hero.Instance.CleanWeapon();
                NpcIsDestroyedReset = true;
            }
            else
            {
                NpcIsDestroyedReset = true;
            }

            NpcHeroAttributesComponent.Instance.ComboCount = 0;
            return NpcIsDestroyedReset;
        }
    }
}
