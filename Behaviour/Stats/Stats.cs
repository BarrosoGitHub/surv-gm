public class Stats
{
    public float MaxHealth { get; private set; }
    public float MaxSpeed { get; private set; }
    public float BaseDamage { get; private set; }
    public float AttackSpeed { get; private set; }
    public float Defense { get; private set; }
    public float CriticalChance { get; private set; }
    public float CriticalDamage { get; private set; }

    public float MaxHealthPerc { get; private set; }
    public float MaxSpeedPerc { get; private set; }
    public float BaseDamagePerc { get; private set; }
    public float AttackSpeedPerc { get; private set; }
    public float DefensePerc { get; private set; }

    public Stats(float maxHealth = 0, 
                 float maxSpeed = 0, 
                 float baseDamage = 0, 
                 float attackSpeed = 0, 
                 float defense = 0, 
                 float criticalChance = 0, 
                 float criticalDamage = 0,
                 float maxHealthPerc = 0,
                 float maxSpeedPerc = 0,
                 float baseDamagePerc = 0,
                 float attackSpeedPerc = 0,
                 float defensePerc = 0)
    {
        MaxHealth = maxHealth;
        MaxSpeed = maxSpeed;
        BaseDamage = baseDamage;
        AttackSpeed = attackSpeed;
        Defense = defense;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;
        MaxHealthPerc = maxHealthPerc;
        MaxSpeedPerc = maxSpeedPerc;
        BaseDamagePerc = baseDamagePerc;
        AttackSpeedPerc = attackSpeedPerc;
        DefensePerc = defensePerc;
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
            CriticalDamage + other.CriticalDamage,
            MaxHealthPerc + other.MaxHealthPerc,
            MaxSpeedPerc + other.MaxSpeedPerc,
            BaseDamagePerc + other.BaseDamagePerc,
            AttackSpeedPerc + other.AttackSpeedPerc,
            DefensePerc + other.DefensePerc
        );
    }
}