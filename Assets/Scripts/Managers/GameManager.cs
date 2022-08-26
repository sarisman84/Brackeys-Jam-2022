using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start() {
        PollingStation.Instance.runtimeManager.onPostStateChangeCallback += (RuntimeManager.RuntimeState previousState, RuntimeManager.RuntimeState state) =>
        {
            switch (state) {
                case RuntimeManager.RuntimeState.MainMenu:
                    OnTransitionToMainMenu();
                    break;
                case RuntimeManager.RuntimeState.Playing:
                    if (previousState == RuntimeManager.RuntimeState.MainMenu)
                        OnTransitionToPlaying();
                    break;
            }
        };
    }

    private void OnTransitionToMainMenu() {//on main menu transition
        PollingStation.Instance.mapGenerator.DestroyMap();//Delete map
        if (entityParent) Destroy(entityParent.gameObject);//Delete all entities 
    }
    private void OnTransitionToPlaying() {
        PollingStation.Instance.mapGenerator.LoadProcedualMap();//Generate map on starting the game from the main menu.
        SetEnemyGoal();
    }



    //Player: spawn in the playerSpawn tile
    //lesser Enemies: spawn in rooms
    //Core Enemy: spawn "behind the player" -> use player tracking
    private Transform entityParent;
    public Transform GetEntityParent() {
        if (!entityParent)
            entityParent = new GameObject("Entity Parent").transform;
        return entityParent;
    }

    public void SetEnemyGoal() {
        GameObject exitOJ = GameObject.FindGameObjectWithTag("Exit");
        if (!exitOJ) {
            Debug.LogError("No Exit was found");
            return;
        }

        Vector3 goal = exitOJ.transform.position;
        Spawn[] spawn = FindObjectsOfType<Spawn>();
        for (int s = 0; s < spawn.Length; s++) {
            if (spawn[s].prefab.GetComponent<LesserEnemy>()) {
                spawn[s].OnSpawn += (GameObject go) => { 
                    go.GetComponent<LesserEnemy>().travelDestination = goal; 
                };
            }
        }
    }



    public void OnPlayerExitMap() {
        //TODO: map exit
        Debug.Log("Map Exit");
    }
}
