using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using InventorySystem.Items;

namespace SCP181.Exiled;

public sealed class Config : IConfig
{
    [Description("Master toggle.\nIf false, the plugin does not load or run.")]
    public bool IsEnabled { get; set; } = true;

    [Description("Enables additional debug logging in server logs.")]
    public bool Debug { get; set; } = false;

    [Description("Percent chance (0-100) for SCP-181 to open a denied but unlocked door.")]
    public int DoorLuck { get; set; } = 10;

    [Description("Cooldown in seconds before SCP-181 can try lucky door opening on the same door again.")]
    public float DoorLuckCooldownSeconds { get; set; } = 5f;

    [Description("Percent chance (0-100) for SCP-181 to negate an incoming damage event.")]
    public int DamageAvoidLuck { get; set; } = 10;

    [Description("Maximum health assigned to SCP-181 when the role is applied.")]
    public int MaxHealth { get; set; } = 150;

    [Description("Minimum online player count required before SCP-181 can be assigned.")]
    public int MinimumPlayers { get; set; } = 1;

    [Description("Percent chance (0-100) for SCP-181 to spawn this round once minimum players is met.")]
    public int SpawnChance { get; set; } = 100;

    [Description("ItemType list granted to SCP-181 immediately after assignment.")]
    public List<ItemType> StartingItems { get; set; } = new()
    {
        ItemType.KeycardJanitor,
        ItemType.Medkit,
        ItemType.Coin
    };
}