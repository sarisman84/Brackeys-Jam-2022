using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LesserEnemy : MonoBehaviour {

    NavMeshAgent agent;
    Transform playerTarget;
    Transform coreEnemy;

    public float moveSpeed;
    public float attackRange;
    public BaseGun weapon;
    public Transform head;
    public Transform gunHolder;
    public Vector3 travelDestination;
    public float destinationDespawnRange = 3.0f;

    private Gun gunInstance;
    private AnimationManager animationManager;
    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animationManager = GetComponent<AnimationManager>();
    }


    private void Start()
    {
        playerTarget = PollingStation.Instance.player.transform;

    }

    private void OnEnable()
    {
        if (weapon)
            gunInstance = weapon.Initialize(this, gunHolder, LayerMask.NameToLayer("Default"), false);
    }

    private void OnDisable()
    {
        gunInstance = null;
    }

    void AttackTarget()
    {
        gunInstance.Fire();

        //weapon.OnWe
    }
    // Update is called once per frame
    void Update()
    {
        if (!coreEnemy)
            coreEnemy = PollingStation.Instance?.gameManager?.coreEnemy?.transform;
        bool foundPlayer = TryGetTargetInSight(playerTarget, out var pos1);
        bool foundCoreEnemy = coreEnemy ? TryGetTargetInSight(coreEnemy, out var pos2) : false;
        bool hasFoundSomeone = foundPlayer || foundCoreEnemy;


        agent.isStopped = hasFoundSomeone || PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing;
        if (PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing) return;
        if (agent.isStopped)
        {
            animationManager?.ExecuteAnimCommand("Movement", AnimationType.AnimTypeDef.Float, 0.0f);
            animationManager?.ExecuteAnimCommand("Attack", AnimationType.AnimTypeDef.Float, 1.0f);
            if (foundCoreEnemy)
            {
                transform.LookAt(coreEnemy, Vector3.up);
                head.transform.LookAt(coreEnemy);
                gunHolder.transform.LookAt(coreEnemy);

            }
            else if (foundPlayer)
            {
                transform.LookAt(playerTarget, Vector3.up);
                head.transform.LookAt(playerTarget);
                gunHolder.transform.LookAt(playerTarget);

            }
            AttackTarget();
        }
        else
        {
            float speed = 1.0f;
            if (agent.velocity.magnitude > 0.01f)
            {
                gunHolder.transform.rotation = Quaternion.LookRotation(agent.velocity);
                speed = 0.0f;
            }

            animationManager?.ExecuteAnimCommand("Movement", AnimationType.AnimTypeDef.Float, speed);

            agent.SetDestination(travelDestination);
        }


        if(Vector3.Distance(travelDestination, transform.position) <= destinationDespawnRange) {
            Destroy(gameObject);//despawn on destination reach
        }
    }


    bool TryGetTargetInSight(Transform target, out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;
        Ray viewRay = new Ray(transform.position, (playerTarget.transform.position - transform.position).normalized);



        bool isTargetInView = Physics.Raycast(viewRay, out var hitInfo, attackRange * 2.0f) && hitInfo.collider && hitInfo.collider.transform == target;

        Debug.DrawRay(viewRay.origin, viewRay.direction * attackRange, isTargetInView ? Color.green : Color.red);

        if (isTargetInView)
        {
            targetPosition = target.transform.position;
            return true;
        }

        return false;
    }
}
