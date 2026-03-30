using System;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    private Stats baseStats;
    private Stats stats;
    private List<Equipment> equipmentList;

    public Action<float> OnHealthChanged;
    public Action OnEntityDied;

    private float currentHealth;
    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            float previousHealth = currentHealth;

            if (value > stats.MaxHealth)
            {
                currentHealth = stats.MaxHealth;
            }
            else if (value < 0)
            {
                currentHealth = 0;
                OnEntityDied?.Invoke();
            }
            else
            {
                currentHealth = value;
            }

            if (previousHealth != currentHealth)
            {
                OnHealthChanged?.Invoke(currentHealth);
            }
        }
    }

    public Entity(Stats baseStats, List<Equipment> equipmentList = null)
    {
        this.baseStats = baseStats;
        this.equipmentList = equipmentList ?? new List<Equipment>();
        UpdateStats();
        currentHealth = stats.MaxHealth;
    }

    public void AddBaseStats(Stats additionalStats)
    {
        baseStats = baseStats.Add(additionalStats);
        UpdateStats();
    }

    public void Equip(Equipment equipment)
    {
        if (equipment == null) return;

        equipmentList.Add(equipment);
        UpdateStats();
    }

    public void Unequip(Equipment equipment)
    {
        if (equipment == null) return;
        if (!equipmentList.Remove(equipment)) return;

        UpdateStats();
    }

    private void UpdateStats()
    {
        stats = baseStats;

        foreach (var equipment in equipmentList)
        {
            stats = stats.Add(equipment.Stats);
        }
    }

    private DamageResult CalculateDamage()
    {
        float damage = stats.BaseDamage * (1 + stats.BaseDamagePerc);

        bool isCritical = Random.Range(0f, 100f) <= stats.CriticalChance;

        if (isCritical)
            damage *= stats.CriticalDamage;

        return new DamageResult
        {
            Damage = damage,
            IsCritical = isCritical
        };
    }

    public DamageResult Attack(Entity target)
    {
        DamageResult result = CalculateDamage();
        float mitigated = target.TakeDamage(result.Damage);
        return new DamageResult { Damage = mitigated, IsCritical = result.IsCritical };
    }

    public float TakeDamage(float amount)
    {
        float mitigated = amount - (stats.Defense * (1 + stats.DefensePerc));
        mitigated = Mathf.Max(0, mitigated);

        CurrentHealth -= mitigated;

        return mitigated;
    }

    public void Heal(float amount)
    {
        CurrentHealth += amount;
    }
}
