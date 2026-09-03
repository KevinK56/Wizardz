namespace Wizardz.Shared.Models;

public enum ChestTier
{
    Wood,       // Common drops
    Gold,       // High mana & guaranteed gear
    Arcane      // Rare+ equipment & essence
}

public class TreasureChest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ChestTier Tier { get; set; } = ChestTier.Wood;
    public string Name { get; set; } = "Wooden Chest";
    public string Icon => Tier switch
    {
        ChestTier.Wood => "📦",
        ChestTier.Gold => "🧰",
        ChestTier.Arcane => "✨",
        _ => "📦"
    };

    public double X { get; set; } = 50; // percentage 0-100 in room
    public double Y { get; set; } = 50;
    public double ManaReward { get; set; }
    public double EssenceReward { get; set; }
    public EquipmentItem? DroppedItem { get; set; }
    public bool IsOpened { get; set; } = false;
}
