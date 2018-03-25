using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public float LevelStartDelay = 5f;                      
        public float TurnDelay = 0.1f;      
        public int PlayerFoodPoints = 100;  
        public static GameManager Instance = null;
        [HideInInspector] public bool PlayersTurn = true; 

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
            InitEnemies();
            AudioManager.SharedInstance.TestLog();
        }

        private static void InitEnemies()
        {
            for (int i = 0; i < 10; i++)
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
            GameObject enemyFromPool = ObjectPooler.SharedInstance.GetPooledObject("Enemy");
            enemyFromPool.transform.position = new Vector3(respawnPositionX, 0, 0);
            enemyFromPool.SetActive(true);
        }

        void Update()
        {
            GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (activeEnemies.Length < 1)
            {
                Debug.Log("Init all enemies");
                ObjectPooler.SharedInstance.InitializeAllEnemies("Enemy");
            }
            // todo remove and use gaop for all enemy prefabs
            // StartCoroutine(EnemiesAi());
        }

        public void GameOver()
        {
            enabled = false;
        }
    }
}