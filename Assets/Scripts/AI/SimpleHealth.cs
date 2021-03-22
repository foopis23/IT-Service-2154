using UnityEngine;

public class SimpleHealth : MonoBehaviour, IHealth
{
    public float maxHealth;
    [SerializeField] private float startingHealth;
    private float _currentHealth;

    private void Start()
    {
        _currentHealth = startingHealth;
    }

    public void ApplyDamage(float damage)
    {
        _currentHealth -= damage;

        if (_currentHealth < 0) _currentHealth = 0;
    }

    public void ApplyHealing(float healing)
    {
        _currentHealth += healing;

        if (_currentHealth > maxHealth) _currentHealth = maxHealth;
    }

    public void SetHealth(float health)
    {
        _currentHealth = health;
        
        if (_currentHealth > maxHealth) _currentHealth = maxHealth;
        if (_currentHealth < 0) _currentHealth = 0;
    }

    public float GetHealth()
    {
        return _currentHealth;
    }
}