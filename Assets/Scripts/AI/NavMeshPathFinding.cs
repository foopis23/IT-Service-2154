using UnityEngine;
using UnityEngine.AI;

public class NavMeshPathFinding : MonoBehaviour, IPathFinding
{
    private NavMeshAgent _navMeshAgent;

    public void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public Vector3[] GetPath(Vector3 destination)
    {
        var path = new NavMeshPath();
        _navMeshAgent.CalculatePath(destination, path);
        return path.corners;
    }
}