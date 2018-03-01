using UnityEngine;
using System.Collections;


using System.Collections.Generic;       //Allows us to use Lists. 

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2f;                      //Time to wait before starting level, in seconds.
    public float turnDelay = 0.1f;                          //Delay between each Player turn.
    public int playerFoodPoints = 100;                      //Starting value for Player food points.
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    [HideInInspector] public bool playersTurn = true;       //Boolean to check if it's players turn, hidden in inspector but public.


    // private BoardManager boardScript;                       //Store a reference to our BoardManager which will set up the level.
    private int level = 2;                                  //Current level number, expressed in game as "Day 1".
    private List<Enemy> enemies;                          //List of all Enemy units, used to issue them move commands.
    private bool enemiesMoving;                             //Boolean to check if enemies are moving.
    private float moveTimeMultiplier;




    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        //Assign enemies to a new List of Enemy objects.
        enemies = new List<Enemy>();

        //Get a component reference to the attached BoardManager script
        // boardScript = GetComponent<BoardManager>();
    }

    void Start()
    {
        InitGame();
    }

    //This is called each time a scene is loaded.
    void OnLevelWasLoaded(int index)
    {
        //Add one to our level number.
        level++;
        //Call InitGame to initialize our level.
        InitGame();
    }

    //Initializes the game for each level.
    void InitGame()
    {

        //Clear any Enemy objects in our List to prepare for next level.
        enemies.Clear();

        //Call the SetupScene function of the BoardManager script, pass it current level number.
        // boardScript.SetupScene(level);
        
        moveTimeMultiplier = 1 - (5 / 10);

        for (int i = 0; i < 10; i++)
        {
            GameObject enemyFromPool = ObjectPooler.SharedInstance.GetPooledObject("Enemy");
            Instantiate(enemyFromPool, new Vector3(1 + i, 0, 0), Quaternion.identity);
        }
    }

    private static float GetPlayerCurrentPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log(player.transform.position.x);
        return player.transform.position.x;
    }

    public static void RecycleEnemy()
    {
        float respawnPositionX = GetPlayerCurrentPosition() * (Random.Range(0,10) > 5 ? 5 : -5f);
        GameObject enemyFromPool = ObjectPooler.SharedInstance.GetPooledObject("Enemy");
        enemyFromPool.transform.position = new Vector3(respawnPositionX, 0, 0);
        enemyFromPool.SetActive(true);
    }

    //Update is called every frame.
    void Update()
    {
        //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
        if (enemiesMoving)
        {
            //If any of these are true, return and do not start MoveEnemies.
            return;
        }

        //Start moving enemies.
        StartCoroutine(EnemiesAi());
    }

    //Call this to add the passed in Enemy to the List of Enemy objects.
    public void AddEnemyToList(Enemy script)
    {
        //Add Enemy to List enemies.
        enemies.Add(script);
    }


    //GameOver is called when the player reaches 0 food points
    public void GameOver()
    {

        //Enable black background image gameObject.
        // levelImage.SetActive(true);

        //Disable this GameManager.
        enabled = false;
    }

    //Coroutine to move enemies in sequence.
    IEnumerator EnemiesAi()
    {
        //While enemiesMoving is true player is unable to move.
        enemiesMoving = true;

        //Loop through List of Enemy objects.
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].FaceTarget();
            if (!enemies[i].isActiveAndEnabled)
            {
                yield return new WaitForSeconds(1f);
            }

            if (!enemies[i].IsInWalkRange())
            {
                enemies[i].RunEnemy();
                yield return new WaitForSeconds(enemies[i].moveTime * moveTimeMultiplier);
            }
            else
            {
                if (enemies[i].IsInCombatRange())
                {
                    enemies[i].RunStopEnemy();
                    enemies[i].Attack();
                    yield return null;
                }
                else if (enemies[i].IsIdle())
                {
                    if (enemies[i].hasWalkAbility && !enemies[i].IsRunning())
                    {
                        enemies[i].MoveEnemy();
                    }
                    yield return new WaitForSeconds(enemies[i].moveTime * moveTimeMultiplier);
                }


            }
        }
        
        playersTurn = true;

        //Enemies are done moving, set enemiesMoving to false.
        enemiesMoving = false;
    }
}