public class Stats
{
    public float MaxHealth { get; private set; }
    public float MaxSpeed { get; private set; }
    public float BaseDamage { get; private set; }
    public float AttackSpeed { get; private set; }
    public float Defense { get; private set; }
    public float CriticalChance { get; private set; }
    public float CriticalDamage { get; private set; }

    public Stats(float maxHealth = 0, 
                 float maxSpeed = 0, 
                 float baseDamage = 0, 
                 float attackSpeed = 0, 
                 float defense = 0, 
                 float criticalChance = 0, 
                 float criticalDamage = 0)
    {
        MaxHealth = maxHealth;
        MaxSpeed = maxSpeed;
        BaseDamage = baseDamage;
        AttackSpeed = attackSpeed;
        Defense = defense;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;
    }

    public Stats Add(Stats other)
    {
        return new Stats(
            MaxHealth + other.MaxHealth,
            MaxSpeed + other.MaxSpeed,
            BaseDamage + other.BaseDamage,
            AttackSpeed + other.AttackSpeed,
            Defense + other.Defense,
            CriticalChance + other.CriticalChance,
            CriticalDamage + other.CriticalDamage
        );
    }
}