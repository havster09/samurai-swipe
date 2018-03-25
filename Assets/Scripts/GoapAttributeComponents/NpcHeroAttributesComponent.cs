using UnityEngine;

namespace Assets.Scripts.GoapAttributeComponents
{
    public class NpcHeroAttributesComponent : MonoBehaviour
    {
        public int Health;
        public int Brave;
        public int AttackCount;
        public int KillCount;

        public NpcHeroAttributesComponent()
        {
            Health = 100;
            Brave = 100;
            AttackCount = 0;
            KillCount = 0;
        }
    }
}