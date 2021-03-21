using UnityEngine;

public class SimpleLerpRotation : MonoBehaviour, IRotation
{
    [SerializeField]
    private float rotationDamping;

    private Vector3 _targetPos;

    public void Update()
    {
        var lookDirection = _targetPos - transform.position;
        lookDirection.y = 0;
        var rotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
    }
    
    public void RotateTowards(Vector3 targetPos)
    {
        _targetPos = targetPos;
    }
}