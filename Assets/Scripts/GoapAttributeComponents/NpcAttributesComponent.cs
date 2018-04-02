using UnityEngine;

namespace Assets.Scripts.GoapAttributeComponents
{
    public class NpcAttributesComponent : MonoBehaviour
    {
        public int KillCount;
        public int AttackCount;
        public int Brave;
        public int Health;

        public int Stamina;
        public Enemy EnemyScript { get; set; }
        
        public NpcAttributesComponent()
        {
            reset();
        }

        void Awake()
        {
            EnemyScript = gameObject.GetComponent<Enemy>();
        }

        void OnEnable()
        {
            reset();
        }

        private void reset()
        {
            KillCount = 0;
            AttackCount = 0;
            Stamina = 100;
            Brave = 0;
            Health = 100;
        }
    }
}