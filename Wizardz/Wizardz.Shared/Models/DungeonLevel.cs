namespace Wizardz.Shared.Models;

public enum DungeonBiome
{
    MossyCatacombs, // Floors 1-10
    SunkenCrypt,     // Floors 11-20
    MagmaCaldera,    // Floors 21-30
    AstralVault      // Floors 31+
}

public class DungeonLevelInfo
{
    public int FloorNumber { get; set; } = 1;
    public bool IsBossFloor => FloorNumber % 10 == 0;
    public DungeonBiome Biome => GetBiomeForFloor(FloorNumber);

    public string BiomeName => Biome switch
    {
        DungeonBiome.MossyCatacombs => "Mossy Catacombs",
        DungeonBiome.SunkenCrypt => "Sunken Crypt of the Damned",
        DungeonBiome.MagmaCaldera => "The Molten Caldera",
        DungeonBiome.AstralVault => "The Celestial Void Vault",
        _ => "Ancient Dungeon"
    };

    public string AmbientColor => Biome switch
    {
        DungeonBiome.MossyCatacombs => "#1a2418",
        DungeonBiome.SunkenCrypt => "#171420",
        DungeonBiome.MagmaCaldera => "#291512",
        DungeonBiome.AstralVault => "#151829",
        _ => "#1c140e"
    };

    public static DungeonBiome GetBiomeForFloor(int floor)
    {
        int cycle = ((floor - 1) / 10) % 4;
        return cycle switch
        {
            0 => DungeonBiome.MossyCatacombs,
            1 => DungeonBiome.SunkenCrypt,
            2 => DungeonBiome.MagmaCaldera,
            _ => DungeonBiome.AstralVault
        };
    }
}
