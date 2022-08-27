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
    [Space]
    public float killDistance = 1.0f;

    [Header("Move Speed Settings")]
    public float moveSpeed;
    public float trackMoveSpeed, searchMoveSpeed, chaseMoveSpeed;

    [Header("Animation")]
    public AnimationManager animationManager;

    [Header("Debug")]
    public bool showLogs = false;
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
        animationManager?.ExecuteAnimCommand("EndIdle");
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
        if (showLogs)
            Debug.Log($"{gameObject.name}: Chasing Player!");



        bool lostSight = !TryGetTargetInSight(out var pos);
        if (!lostSight)
        {
            animationManager?.ExecuteAnimCommand("StartAttack");
            agent.speed = moveSpeed + chaseMoveSpeed;
            lastKnownPositionOfTarget = pos;
            agent.SetDestination(target.transform.position);
            transform.rotation = Quaternion.LookRotation(agent.desiredVelocity);

            //- Changes by Sytoplis
            if(Vector3.Distance(target.transform.position, transform.position) <= killDistance) {//Instantly kill the player when coming close enough
                float killingDamage = PollingStation.Instance.playerHealthHandler.maxHealth * 2.0f;
                PollingStation.Instance.playerHealthHandler.OnDamageTaken(killingDamage);
            }
        }
        else
        {
            animationManager?.ExecuteAnimCommand("EndAttack");
            stateMachine.ExecuteCommand(CE_Behaviour.Search);
        }
    }

    void TrackDef(StateMachine<CE_Behaviour>.State currentState)
    {
        if (currentState == StateMachine<CE_Behaviour>.State.Entering)
        {
            currentTrackRate = trackRate;
        }

        if (currentState != StateMachine<CE_Behaviour>.State.Running) return;

        if (showLogs)
            Debug.Log($"{gameObject.name}: Tracking Player's Wearabouts!");
        agent.speed = moveSpeed + trackMoveSpeed;

        currentTrackRate += Time.deltaTime;
        if (currentTrackRate >= trackRate)
        {
            lastKnownPositionOfTarget = target.transform.position;
            currentTrackRate = 0;
        }

        agent.SetDestination(lastKnownPositionOfTarget);

        if (agent.remainingDistance < 0.1f)
        {
            animationManager?.ExecuteAnimCommand("StartIdle");
        }
        else
        {
            animationManager?.ExecuteAnimCommand("EndIdle");
        }

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
            currentSearchRate = 0;
        }

        if (currentState != StateMachine<CE_Behaviour>.State.Running) return;

        agent.speed = moveSpeed + searchMoveSpeed;

        if (agent.desiredVelocity.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(agent.desiredVelocity);
        if (showLogs)
            Debug.Log($"{gameObject.name}: Searching Player!");

        if (agent.remainingDistance <= 0.01f)
        {
            currentSearchRate += Time.deltaTime;
            animationManager?.ExecuteAnimCommand("StartIdle");
        }
        else
        {
            animationManager?.ExecuteAnimCommand("EndIdle");
        }

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

        //Check if grid pos is valid (As in, there is a tile there)
        if (mapGen.map == null)
            return result;

        //there are 27 possible newGridPos -> try a few times
        for (int iter = 0; iter < 60; iter++) {
            Vector3Int newGridPos = currentGridPos + UnityExtensions.RndmUnitVector();

            if (mapGen.map.InBounds(newGridPos) && !mapGen.map[newGridPos].IsEmpty()) {
                //Walk to either the center of the tile(easy) or some random valid position on the tile(hard, define valid position)
                result = mapGen.GetWorldPos(newGridPos);
                break;
            }
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

        LayerMask everything = -1;

        bool isTargetInView = Physics.Raycast(viewRay, out var hitInfo, visionRadius * 2.0f, everything) && hitInfo.collider && hitInfo.collider.CompareTag("Player") && IsInVisionCone(viewRay);

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
