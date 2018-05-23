﻿using UnityEngine;

namespace Assets.Scripts.GoapAttributeComponents
{
    public class NpcHeroAttributesComponent : MonoBehaviour
    {
        public static NpcHeroAttributesComponent Instance;
        public int Health;
        public int Brave;
        public int Rage;
        public int AttackCount;
        public int ComboCount;
        public int KillCount;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        public NpcHeroAttributesComponent()
        {
            Health = 100;
            Brave = 100;
            Rage = 0;
            AttackCount = 0;
            ComboCount = 0;
            KillCount = 0;
        }
    }
}