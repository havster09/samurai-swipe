using UnityEngine;

namespace Assets.Scripts.GoapHeroActions
{
    public class GoapHeroWipeBloodAction : GoapHeroAction
    {
        public GoapHeroWipeBloodAction()
        {
            addPrecondition("destroyEnemyNpc", true);
            addPrecondition("bloodCover", true);
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
                HeroScript.WipeBlood();
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
