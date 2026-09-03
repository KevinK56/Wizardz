namespace Wizardz.Shared.Models;

public enum EquipmentSlot
{
    Weapon,     // Wand / Staff
    Robe,       // Wizard Armor
    Hat,        // Arcane Cowl / Wizard Hat
    Ring        // Runic Ring / Amulet
}

public enum ItemRarity
{
    Common,     // Gray / Off-white
    Uncommon,   // Forest Green
    Rare,       // Arcane Cyan / Blue
    Epic,       // Mystic Purple
    Legendary   // Radiant Gold / Orange
}

public class EquipmentItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EquipmentSlot Slot { get; set; }
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public string Icon { get; set; } = "🪄";
    public int ItemLevel { get; set; } = 1;

    // Combat Stats
    public double AttackPower { get; set; } // Adds to base attack damage
    public double AttackSpeedBonus { get; set; } // +% attacks per second
    public double CriticalChanceBonus { get; set; } // +% crit chance
    public double CriticalDamageBonus { get; set; } // +% crit damage
    public double ManaFindBonus { get; set; } // +% mana from monsters & chests

    public string GetRarityCssClass() => Rarity switch
    {
        ItemRarity.Common => "rarity-common",
        ItemRarity.Uncommon => "rarity-uncommon",
        ItemRarity.Rare => "rarity-rare",
        ItemRarity.Epic => "rarity-epic",
        ItemRarity.Legendary => "rarity-legendary",
        _ => "rarity-common"
    };

    public string GetRarityColorHex() => Rarity switch
    {
        ItemRarity.Common => "#c4b5a5",
        ItemRarity.Uncommon => "#4ade80",
        ItemRarity.Rare => "#38bdf8",
        ItemRarity.Epic => "#c084fc",
        ItemRarity.Legendary => "#fbbf24",
        _ => "#c4b5a5"
    };
}
