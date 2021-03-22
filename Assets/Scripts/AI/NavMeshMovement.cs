using System;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovement : MonoBehaviour, IMovement
{
    private NavMeshAgent _navMeshAgent;
    private Vector3[] _path;

    public void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Start()
    {
        _navMeshAgent.updateRotation = false;
    }

    public void SetPath(Vector3[] path)
    {
        _path = path;
        if (path == null || path.Length <= 0)
        {
            _navMeshAgent.SetDestination(transform.position);
        }
        else
        {
            _navMeshAgent.SetDestination(_path[_path.Length - 1]);
        }
    }

    public void SetSpeed(float speed)
    {
        if (_navMeshAgent != null)
            _navMeshAgent.speed = speed;
    }

    public float GetSpeed()
    {
        if (_navMeshAgent == null) return -1;
        
        return _navMeshAgent.speed;
    }
}