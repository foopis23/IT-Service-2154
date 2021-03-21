using UnityEngine;

public interface IMovement
{
    public void SetPath(Vector3[] path);
    public void SetSpeed(float speed);
    public float GetSpeed();
}