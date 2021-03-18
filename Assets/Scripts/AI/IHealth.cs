public interface IHealth
{
    public void ApplyDamage(float damage);
    public void ApplyHealing(float healing);
    public void SetHealth(float health);
    public float GetHealth();
}