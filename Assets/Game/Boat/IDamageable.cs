public interface IDamageable
{
    public CannonballOwner CannonballOwner { get; }
    public bool CanDamage { get; }
    public void Damage(float _damage);
}
