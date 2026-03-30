using System;
using System.Collections.Generic;

public class Entity
{
    private Stats baseStats;
    private Stats BaseStats
    {
        get
        {
            return baseStats;
        }
        set
        {
            baseStats = value;
            UpdateStats();
        }
    }
    private List<Equipment> equipmentList;
    private List<Equipment> EquipmentList
    {
        get
        {
            return equipmentList;
        }
        set
        {
            equipmentList = value;
            UpdateStats();
        }
    }

    public Stats stats;

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

    public Entity(Stats baseStats)
    {
        this.baseStats = baseStats;
        EquipmentList = new List<Equipment>();
    }

    public void AddBaseStats(Stats additionalStats)
    {
        BaseStats = BaseStats.Add(additionalStats);
    }

    public void AddEquipment(Equipment equipment)
    {
        EquipmentList.Add(equipment);
    }

    private void UpdateStats()
    {
        stats = baseStats;

        foreach (var equipment in equipmentList)
        {
            stats = stats.Add(equipment.Stats);
        }
    }

    public void TakeDamage(float amount)
    {
        float mitigated = amount - (stats.Defense * stats.DefensePerc);
        mitigated = Mathf.Max(0, mitigated);

        CurrentHealth -= mitigated;
    }
}
