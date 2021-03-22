using UnityEngine;

public interface IVision
{
    public bool CanSeeObject();
    public GameObject[] GetVisibleObjects();
}