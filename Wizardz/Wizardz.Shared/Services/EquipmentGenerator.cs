using Wizardz.Shared.Models;

namespace Wizardz.Shared.Services;

public static class EquipmentGenerator
{
    private static readonly Random _rand = new();

    private static readonly string[] WeaponPrefixes = { "Worn", "Reinforced", "Engraved", "Runed", "Mystic", "Eldritch", "Astral" };
    private static readonly string[] WeaponBases = { "Wand", "Staff", "Rod", "Scepter" };

    private static readonly string[] RobePrefixes = { "Tattered", "Apprentice", "Silken", "Shadow", "Spellweaver", "Archmage" };
    private static readonly string[] RobeBases = { "Robe", "Cloak", "Vestment", "Tunic" };

    private static readonly string[] HatPrefixes = { "Pointy", "Scholar", "Enchanted", "Cosmic", "Crown", "Diadem" };
    private static readonly string[] HatBases = { "Hat", "Cowl", "Hood", "Circlet" };

    private static readonly string[] RingPrefixes = { "Copper", "Silver", "Sapphire", "Ruby", "Void", "Celestial" };
    private static readonly string[] RingBases = { "Ring", "Band", "Amulet", "Loop" };

    public static EquipmentItem GenerateLoot(int floorNumber, ItemRarity? guaranteedRarity = null)
    {
        var slot = (EquipmentSlot)_rand.Next(0, 4);
        var rarity = guaranteedRarity ?? RollRarity(floorNumber);

        string name = GenerateName(slot, rarity);
        string icon = slot switch
        {
            EquipmentSlot.Weapon => rarity >= ItemRarity.Epic ? "🔮" : "🪄",
            EquipmentSlot.Robe => "🥋",
            EquipmentSlot.Hat => "🧙‍♂️",
            EquipmentSlot.Ring => "💍",
            _ => "✨"
        };

        double rarityMultiplier = rarity switch
        {
            ItemRarity.Common => 1.0,
            ItemRarity.Uncommon => 1.5,
            ItemRarity.Rare => 2.3,
            ItemRarity.Epic => 3.5,
            ItemRarity.Legendary => 5.5,
            _ => 1.0
        };

        double basePower = (5.0 + (floorNumber * 4.0)) * rarityMultiplier;

        var item = new EquipmentItem
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"Forged for deep dungeon explorations on Floor {floorNumber}.",
            Slot = slot,
            Rarity = rarity,
            Icon = icon,
            ItemLevel = floorNumber
        };

        switch (slot)
        {
            case EquipmentSlot.Weapon:
                item.AttackPower = Math.Round(basePower, 1);
                if (rarity >= ItemRarity.Uncommon) item.AttackSpeedBonus = Math.Round((int)rarity * 4.0, 1); // +4% to +16%
                if (rarity >= ItemRarity.Rare) item.CriticalDamageBonus = Math.Round((int)rarity * 10.0, 1);
                break;

            case EquipmentSlot.Robe:
                item.AttackPower = Math.Round(basePower * 0.4, 1);
                item.ManaFindBonus = Math.Round(10.0 * (int)rarity, 1); // +10% to +40%
                break;

            case EquipmentSlot.Hat:
                item.AttackPower = Math.Round(basePower * 0.6, 1);
                item.CriticalChanceBonus = Math.Round(1.5 * (int)rarity, 1); // +1.5% to +6%
                break;

            case EquipmentSlot.Ring:
                item.AttackPower = Math.Round(basePower * 0.5, 1);
                item.CriticalChanceBonus = Math.Round(1.0 * (int)rarity, 1);
                item.ManaFindBonus = Math.Round(5.0 * (int)rarity, 1);
                break;
        }

        return item;
    }

    private static ItemRarity RollRarity(int floorNumber)
    {
        int roll = _rand.Next(0, 100);

        // Odds shift with floor depth
        int legendaryChance = Math.Min(12, 1 + (floorNumber / 5)); // 1% to 12%
        int epicChance = Math.Min(25, 5 + (floorNumber / 3));      // 5% to 25%
        int rareChance = Math.Min(40, 15 + (floorNumber / 2));     // 15% to 40%
        int uncommonChance = 35;

        if (roll < legendaryChance) return ItemRarity.Legendary;
        roll -= legendaryChance;

        if (roll < epicChance) return ItemRarity.Epic;
        roll -= epicChance;

        if (roll < rareChance) return ItemRarity.Rare;
        roll -= rareChance;

        if (roll < uncommonChance) return ItemRarity.Uncommon;

        return ItemRarity.Common;
    }

    private static string GenerateName(EquipmentSlot slot, ItemRarity rarity)
    {
        string[] prefixes = slot switch
        {
            EquipmentSlot.Weapon => WeaponPrefixes,
            EquipmentSlot.Robe => RobePrefixes,
            EquipmentSlot.Hat => HatPrefixes,
            EquipmentSlot.Ring => RingPrefixes,
            _ => WeaponPrefixes
        };

        string[] bases = slot switch
        {
            EquipmentSlot.Weapon => WeaponBases,
            EquipmentSlot.Robe => RobeBases,
            EquipmentSlot.Hat => HatBases,
            EquipmentSlot.Ring => RingBases,
            _ => WeaponBases
        };

        int pIdx = Math.Min(prefixes.Length - 1, (int)rarity + _rand.Next(0, 2));
        int bIdx = _rand.Next(0, bases.Length);

        return $"{prefixes[pIdx]} {bases[bIdx]}";
    }

    public static EquipmentItem CreateStarterWand()
    {
        return new EquipmentItem
        {
            Id = "starter_wand",
            Name = "Apprentice Oak Wand",
            Description = "A simple wand carved from elder oak. Focuses raw magical sparks.",
            Slot = EquipmentSlot.Weapon,
            Rarity = ItemRarity.Common,
            Icon = "🪄",
            ItemLevel = 1,
            AttackPower = 8.0,
            AttackSpeedBonus = 0.0,
            CriticalChanceBonus = 2.0
        };
    }
}
