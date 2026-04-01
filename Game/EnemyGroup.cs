using System.Collections.Generic;

public class EnemyGroup
{
    public EnemyType EnemyType { get; set; }
    public List<EnemyController> EnemyList { get; set; }
    public Controller TargetController { get; set; }
    public int MaxNumberOfEnemies { get; set; }
    public float MaxEngagementDistance { get; set; }
    public int MaxNumberOfEnemiesThatCanEngageSimultaneously { get; set; }
    public float AttackInterval { get; set; }
    

    public EnemyGroup(Controller targetController, int maxNumberOfEnemies, float maxEngagementDistance, int maxNumberOfEnemiesThatCanEngageSimultaneously, float attackInterval)
    {
        TargetController = targetController;
        MaxNumberOfEnemies = maxNumberOfEnemies;
        MaxEngagementDistance = maxEngagementDistance;
        MaxNumberOfEnemiesThatCanEngageSimultaneously = maxNumberOfEnemiesThatCanEngageSimultaneously;
        AttackInterval = attackInterval;
        EnemyList = new();
    }
}