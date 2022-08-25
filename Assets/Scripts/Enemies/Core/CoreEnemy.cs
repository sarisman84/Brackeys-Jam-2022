using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class CoreEnemy : MonoBehaviour
{
    public enum CE_Behaviour
    {
        Chase, Search, Track
    }

    private StateMachine<CE_Behaviour> stateMachine;
    private MapGenerator mapGen;


    private NavMeshAgent agent;
    private GameObject target;

    private Vector3 lastKnownPositionOfTarget;

    public float visionRadius = 4.0f, visionAngleDeg = 30.0f;
    public float trackRate = 8.0f, searchRate = 5.0f;
    public int searchAttempt = 1;

    [Header("Move Speed Settings")]
    public float moveSpeed;
    public float trackMoveSpeed, searchMoveSpeed, chaseMoveSpeed;


    [Header("Debug")]
    public CE_Behaviour currentBehaviour;
    public float currentAngle;
    private float currentTrackRate, currentSearchRate;
    private int currSearchAttempt;

    private void Start()
    {
        target = PollingStation.Instance.player;
        mapGen = PollingStation.Instance.mapGenerator;
    }
    private void Awake()
    {

        currentTrackRate = trackRate;
        stateMachine = new StateMachine<CE_Behaviour>();
        agent = GetComponent<NavMeshAgent>();

        stateMachine.StartingState(TrackDef);
        stateMachine.AddCommand(CE_Behaviour.Chase, new Command<CE_Behaviour>(
            new List<System.Action<StateMachine<CE_Behaviour>.State>> { SearchDef, TrackDef },
            ChasingDef));
        stateMachine.AddCommand(CE_Behaviour.Search, new Command<CE_Behaviour>(
            new List<System.Action<StateMachine<CE_Behaviour>.State>> { ChasingDef },
            SearchDef));
        stateMachine.AddCommand(CE_Behaviour.Track, new Command<CE_Behaviour>(
            new List<System.Action<StateMachine<CE_Behaviour>.State>> { SearchDef },
             TrackDef));
    }


    void ChasingDef(StateMachine<CE_Behaviour>.State currentState)
    {
        if (currentState != StateMachine<CE_Behaviour>.State.Running) return;

        Debug.Log($"{gameObject.name}: Chasing Player!");



        bool lostSight = !TryGetTargetInSight(out var pos);
        if (!lostSight)
        {
            agent.speed = moveSpeed + chaseMoveSpeed;
            lastKnownPositionOfTarget = pos;
            agent.SetDestination(target.transform.position);
            transform.rotation = Quaternion.LookRotation((target.transform.position - transform.position).normalized);

        }
        else
        {

            stateMachine.ExecuteCommand(CE_Behaviour.Search);
        }
    }

    void TrackDef(StateMachine<CE_Behaviour>.State currentState)
    {
        if (currentState != StateMachine<CE_Behaviour>.State.Running) return;

        Debug.Log($"{gameObject.name}: Tracking Player's Wearabouts!");
        agent.speed = moveSpeed + trackMoveSpeed;

        currentTrackRate += Time.deltaTime;
        if (currentTrackRate >= trackRate)
        {
            lastKnownPositionOfTarget = target.transform.position;
            currentTrackRate = 0;
        }

        agent.SetDestination(lastKnownPositionOfTarget);

        if (TryGetTargetInSight(out var pos))
        {
            stateMachine.ExecuteCommand(CE_Behaviour.Chase);
        }
        if (agent.desiredVelocity.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(agent.desiredVelocity);
    }



    void SearchDef(StateMachine<CE_Behaviour>.State currentState)
    {

        if (currentState == StateMachine<CE_Behaviour>.State.Entering)
        {
            currSearchAttempt = searchAttempt;
        }

        if (currentState != StateMachine<CE_Behaviour>.State.Running) return;

        agent.speed = moveSpeed + searchMoveSpeed;

        if (agent.desiredVelocity.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(agent.desiredVelocity);

        Debug.Log($"{gameObject.name}: Searching Player!");

        if (agent.remainingDistance <= 0.01f)
            currentSearchRate += Time.deltaTime;
        if (currentSearchRate >= searchRate)
        {
            Vector3 newSearchPos = SearchInNewArea();

            agent.SetDestination(newSearchPos);

            currentSearchRate = 0;
            currSearchAttempt--;
        }

        if (currSearchAttempt < 0)
        {
            stateMachine.ExecuteCommand(CE_Behaviour.Track);
            return;
        }

        if (TryGetTargetInSight(out var pos))
        {
            stateMachine.ExecuteCommand(CE_Behaviour.Chase);
        }
    }

    private Vector3 SearchInNewArea()
    {
        Vector3 result = new Vector3();


        Vector3Int currentGridPos = mapGen.GetGridPos(transform.position);

        Vector3Int newGridPos = currentGridPos + UnityEngine.Random.insideUnitSphere.Cast<Vector3, Vector3Int>();

        //Check if grid pos is valid (As in, there is a tile there)
        if (mapGen.map == null)
        {
            return result;
        }


        if (mapGen.map.InBounds(newGridPos) && !mapGen.map[newGridPos].IsEmpty())
        {
            //Walk to either the center of the tile(easy) or some random valid position on the tile(hard, define valid position)
            result = new Vector3(
                newGridPos.x * mapGen.tileSize.x + mapGen.mapSize.x / 2.0f,
                newGridPos.y * mapGen.tileSize.y + mapGen.mapSize.y / 2.0f,
                newGridPos.z * mapGen.tileSize.z + mapGen.mapSize.z / 2.0f);


        }




        return result;
    }

    private void Update()
    {
        bool isPaused = PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing;
        agent.isStopped = isPaused;
        if (isPaused) return;


        stateMachine.UpdateCurrentState();
        currentBehaviour = stateMachine.CurrentState;






    }



    bool TryGetTargetInSight(out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;
        Ray viewRay = new Ray(transform.position, (target.transform.position - transform.position).normalized);



        bool isTargetInView = Physics.Raycast(viewRay, out var hitInfo, visionRadius * 2.0f) && hitInfo.collider && hitInfo.collider.CompareTag("Player") && IsInVisionCone(viewRay);

        Debug.DrawRay(viewRay.origin, viewRay.direction * visionRadius, isTargetInView ? Color.green : Color.red);

        if (isTargetInView)
        {
            targetPosition = target.transform.position;
            return true;
        }

        return false;
    }

    private bool IsInVisionCone(Ray viewRay)
    {
        float angle = Vector3.Angle(viewRay.direction, transform.forward);
        currentAngle = angle;
        //Taken from this http://answers.unity.com/answers/16049/view.html
        return angle < visionAngleDeg / 2.0f;




    }
}
