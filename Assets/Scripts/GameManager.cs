﻿using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public Object Hero;

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

        void Start()
        {
            InitGame();
        }

        void OnLevelWasLoaded(int index)
        {
            InitGame();
        }

        void InitGame()
        {
            InitHero();
            InitEnemies();
            AudioManager.Instance.TestLog();
        }

        private void InitHero()
        {
            Hero = Instantiate(Resources.Load("GenjuroHero"));
        }

        private void InitEnemies()
        {
            for (int i = 0; i < 1; i++)
            {
                RespawnEnemyFromPool();
            }
        }

        public static float GetPlayerCurrentPosition()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player.transform.position.x;
        }

        public static float GetRespawnCurrentPosition(string side)
        {
            GameObject respawnPosition = GameObject.Find("respawnPosition" + side);
            if (respawnPosition)
            {
                return respawnPosition.transform.position.x;
            }
            else
            {
                return 10f;
            }
        
        }

        public static void RespawnEnemyFromPool()
        {
            float respawnPositionX = GetRespawnCurrentPosition(Random.Range(0f, 10f) > 5 ? "Left" : "Right");
            GameObject enemyFromPool = ObjectPooler.Instance.GetPooledObject("Enemy");
            if (enemyFromPool)
            {
                enemyFromPool.transform.position = new Vector3(respawnPositionX, 0, 0);
                enemyFromPool.SetActive(true);
            }
        }

        void Update()
        {
            GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (activeEnemies.Length < 1)
            {
                Debug.Log("Init all enemies");
                ObjectPooler.Instance.InitializeAllEnemies("Enemy");
            }
        }

        public void GameOver()
        {
            enabled = false;
        }
    }
}