using UnityEngine;

public class SimpleDamageDisplayManager : MonoBehaviour, IDamageDisplayManager
{
    private static readonly int IsTakingDamage = Animator.StringToHash("isTakingDamage");
    
    [SerializeField]
    private float damageIndicatorLength = 0.4f;
    
    private float _lastDamageTime = float.MinValue;
    private Animator _animator;

    public void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Update()
    {
        _animator.SetBool(IsTakingDamage, (Time.time - _lastDamageTime) < damageIndicatorLength);
    }

    public void TakeDamage()
    {
        _lastDamageTime = Time.time;
    }
}