using System.Collections.Generic;
using Exiled.API.Interfaces;
using InventorySystem.Items;

namespace SCP181.Exiled;

public sealed class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;

    public bool Debug { get; set; } = false;

    public int DoorLuck { get; set; } = 10;

    public int DamageAvoidLuck { get; set; } = 10;

    public int MaxHealth { get; set; } = 150;

    public int MinimumPlayers { get; set; } = 1;

    public List<ItemType> StartingItems { get; set; } = new()
    {
        ItemType.KeycardJanitor,
        ItemType.Medkit,
        ItemType.Coin,
    };
}
