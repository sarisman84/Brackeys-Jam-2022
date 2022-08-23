using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class CoreEnemy : MonoBehaviour
{
    enum CE_Behaviour
    {
        Chase, Search, Track
    }

    private StateMachine<CE_Behaviour> stateMachine;

    private NavMeshAgent agent;
    private GameObject target;


    private void Start()
    {
        target = PollingStation.Instance.player;
    }
    private void Awake()
    {
        stateMachine = new StateMachine<CE_Behaviour>();
        agent = GetComponent<NavMeshAgent>();


        stateMachine.AddCommand(CE_Behaviour.Chase, new Command<CE_Behaviour>(
            new List<System.Action<StateMachine<CE_Behaviour>.State>> { SearchDef, TrackDef },
            ChasingDef));
    }


    void ChasingDef(StateMachine<CE_Behaviour>.State currentState)
    {

    }

    void TrackDef(StateMachine<CE_Behaviour>.State currentState)
    {

    }



    void SearchDef(StateMachine<CE_Behaviour>.State currentState)
    {

    }

    private void Update()
    {
        if (PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing) return;


        stateMachine.UpdateCurrentState();

        agent.SetDestination(target.transform.position);

    }


}
