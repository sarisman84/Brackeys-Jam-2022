using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LesserEnemy : MonoBehaviour
{

    NavMeshAgent agent;
    Transform target;

    public float moveSpeed;
    public float attackRange;
    public BaseGun weapon;
    public Transform head;
    public Transform gunHolder;
    public Vector3 travelDestination;

    private float curFireRate;
    private Gun gunInstance;
    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    private void Start()
    {
        target = PollingStation.Instance.player.transform;

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
        agent.isStopped = TryGetTargetInSight(out var pos) || PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing;
        if (PollingStation.Instance.runtimeManager.currentState != RuntimeManager.RuntimeState.Playing) return;
        if (agent.isStopped)
        {
            head.transform.LookAt(target);
            gunHolder.transform.LookAt(target);
            AttackTarget();
        }
        else
        {
            if (agent.velocity.magnitude > 0.01f)
                gunHolder.transform.rotation = Quaternion.LookRotation(agent.velocity);
            curFireRate = 0;
            agent.SetDestination(travelDestination);
        }





    }


    bool TryGetTargetInSight(out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;
        Ray viewRay = new Ray(transform.position, (target.transform.position - transform.position).normalized);



        bool isTargetInView = Physics.Raycast(viewRay, out var hitInfo, attackRange * 2.0f) && hitInfo.collider && hitInfo.collider.CompareTag("Player");

        Debug.DrawRay(viewRay.origin, viewRay.direction * attackRange, isTargetInView ? Color.green : Color.red);

        if (isTargetInView)
        {
            targetPosition = target.transform.position;
            return true;
        }

        return false;
    }
}
