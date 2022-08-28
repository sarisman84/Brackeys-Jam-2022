using UnityEngine;

public class GameManager : MonoBehaviour {
    public GameObject coreEnemy { get; set; }
    public GameObject exitRoom { private get; set; }

    public int currentRunCount = 0;
    public int runCount = 3;
    public string[] startMessage = { "Escape", "Run Away", "Find Freedom"};

    private void Start()
    {
        PollingStation.Instance.runtimeManager.onPostStateChangeCallback += (RuntimeManager.RuntimeState previousState, RuntimeManager.RuntimeState state) =>
        {
            switch (state)
            {
                case RuntimeManager.RuntimeState.MainMenu:
                    OnTransitionToMainMenu();
                    break;
                case RuntimeManager.RuntimeState.Playing:
                    if (previousState == RuntimeManager.RuntimeState.MainMenu)
                        OnLeavingMenu();
                    break;
            }
        };
    }

    private void OnTransitionToMainMenu()
    {//on main menu transition
        PollingStation.Instance.audioManager.Play("MainMenu", true);
        currentRunCount = 0;
        ResetMap();
    }
    private void OnLeavingMenu()
    {
        currentRunCount = 0;
        CreateMap();
    }
    private void RerunGame() {
        ResetMap();
        CreateMap();
    }



    //Player: spawn in the playerSpawn tile
    //lesser Enemies: spawn in rooms
    //Core Enemy: spawn "behind the player" -> use player tracking
    private Transform entityParent;
    public Transform GetEntityParent()
    {
        if (!entityParent)
            entityParent = new GameObject("Entity Parent").transform;
        return entityParent;
    }

    public void SetEnemyGoal()
    {
        GameObject exitOJ = exitRoom;
        if (!exitOJ)
        {
            Debug.LogError("No Exit was found");
            return;
        }

        Vector3 goal = exitOJ.transform.position;
        Spawn[] spawn = FindObjectsOfType<Spawn>();
        for (int s = 0; s < spawn.Length; s++)
        {
            if (spawn[s].spawnOptions.Length > 0)
                if (spawn[s].spawnOptions[0].prefab.GetComponent<LesserEnemy>())
                {
                    spawn[s].OnSpawn += (GameObject go) =>
                    {
                        go.GetComponent<LesserEnemy>().travelDestination = goal;
                    };

                }
                else if (spawn[s].spawnOptions[0].prefab.GetComponent<CoreEnemy>())
                {
                    spawn[s].OnSpawn += OnCoreEnemySpawn;
                }
        }
    }

    public void OnCoreEnemySpawn(GameObject coreEnemy)
    {
        PollingStation.Instance.audioManager.Play("CE_Spawn_Intro", true).OnComplete(manager => manager.Play("MainLoop", true));
        PollingStation.Instance.audioManager.Play("CE_Spawn");
        var _ = new PopupEffect(this, "You are not alone", 1.5f);
        this.coreEnemy = coreEnemy;
    }



    public void OnPlayerExitMap()
    {
        currentRunCount++;
        Debug.Log("Map Exit");
        if(currentRunCount < runCount)
            RerunGame();
        else {
            Debug.Log("YOU HAVE WON THE GAME");
            Win();
        }
    }

    public void ResetPlayer() {
        PollingStation.Instance.player.GetComponent<HealthHandler>().ResetHealth();
        PollingStation.Instance.weaponManager.RemoveCurrentGun();
    }


    public void CreateMap() {
        PollingStation.Instance.audioManager.Play("Before_CE_Spawn", true);

        var _ = new PopupEffect(this, startMessage[currentRunCount], 1.5f);
        PollingStation.Instance.mapGenerator.LoadProcedualMap();//Generate map on starting the game from the main menu.
        SetEnemyGoal();
    }
    public void ResetMap() {
        PollingStation.Instance.mapGenerator.DestroyMap();//Delete map
        if (entityParent) Destroy(entityParent.gameObject);//Delete all entities 

        ResetPlayer();
        coreEnemy = null;
    }



    public void Win() {
        PollingStation.Instance.menuManager.OpenCanvas(PollingStation.Instance.menuManager.winningCanvas);
        PollingStation.Instance.runtimeManager.SetStateToGameOver();
    }
}