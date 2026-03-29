public class Equipment
{
    public string Name { get; set; }
    public SlotType Type { get; set; }
    public Stats Stats { get; set; }
}

public enum SlotType
{
    Boots,
}