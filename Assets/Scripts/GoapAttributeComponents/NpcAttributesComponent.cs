using UnityEngine;

namespace Assets.Scripts.GoapAttributeComponents
{
    public class NpcAttributesComponent : MonoBehaviour
    {
        public int KillCount;
        public int AttackCount;
        public int Brave;

        private int _health;
        public int Health
        {
            get { return _health; }
            set
            {
                _health = value;
                if (_health < 100 && _health > 1)
                {
                    EnemyScript.EnemyHitSuccess();
                }
            }
        }

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
            _health = 100;
        }
    }
}