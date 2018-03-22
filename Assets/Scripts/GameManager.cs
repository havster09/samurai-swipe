using UnityEngine;
using System.Collections;


using System.Collections.Generic;       

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 5f;                      
    public float turnDelay = 0.1f;      
    public int playerFoodPoints = 100;  
    public static GameManager instance = null;
    [HideInInspector] public bool playersTurn = true;
    
    private int level = 2;                                 
    private List<Enemy> enemies;                          
    private bool enemiesMoving;                           
    private float moveTimeMultiplier;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        enemies = new List<Enemy>();
    }

    void Start()
    {
        InitGame();
    }

    void OnLevelWasLoaded(int index)
    {
        level++;
        InitGame();
    }

    void InitGame()
    {

        enemies.Clear();
        moveTimeMultiplier = 1f;
        InitEnemies();
        AudioManager.SharedInstance.TestLog();
    }

    private static void InitEnemies()
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
        GameObject enemyFromPool = ObjectPooler.SharedInstance.GetPooledObject("Enemy");
        enemyFromPool.transform.position = new Vector3(respawnPositionX, 0, 0);
        enemyFromPool.SetActive(true);
    }

    void Update()
    {
        if (enemiesMoving)
        {
            return;
        }
        GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (activeEnemies.Length < 1)
        {
            Debug.Log("Init all enemies");
            ObjectPooler.SharedInstance.InitializeAllEnemies("Enemy");
        }
        // todo remove and use gaop for all enemy prefabs
        // StartCoroutine(EnemiesAi());
    }

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }


    public void GameOver()
    {
        enabled = false;
    }

    IEnumerator EnemiesAi()
    {
        //While enemiesMoving is true player is unable to move.
        enemiesMoving = true;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i]._npcAttributes)
            {
                enemies[i].FaceTarget();
            }
            
            if (!enemies[i].gameObject.activeInHierarchy)
            {
                yield return new WaitForSeconds(.1f);
            }
            else
            {
                if (!enemies[i].IsInWalkRange() && !enemies[i].IsAnimationPlaying("run") && !enemies[i].isAttacking)
            {
                enemies[i].RunEnemy();
                yield return new WaitForSeconds(enemies[i].moveTime * moveTimeMultiplier);
            }
            else
            {
                if (enemies[i].IsInCombatRange())
                {
                    enemies[i].StopEnemyVelocity();
                    enemies[i].Attack("enemyAttackOne");
                    yield return new WaitForSeconds(.2f);
                }
                else if (enemies[i].IsAnimationPlaying("idle"))
                {
                    if (enemies[i]._hasWalkAbility && !enemies[i].IsAnimationPlaying("run") && !enemies[i].isAttacking)
                    {
                        enemies[i].MoveEnemy();
                    }
                    yield return new WaitForSeconds(enemies[i].moveTime * moveTimeMultiplier);
                }


            }
            }
        }
        playersTurn = true;
        enemiesMoving = false;
    }
}