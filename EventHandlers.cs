using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;

namespace SCP181.Exiled;

public sealed class EventHandlers
{
    private readonly Plugin plugin;
    private bool pendingAssignment;

    public EventHandlers(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void OnRoundStarted()
    {
        if (Player.List.Count() < plugin.Config.MinimumPlayers)
            return;

        pendingAssignment = true;
        TryAssignScp181();
    }

    public void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (!pendingAssignment || ev.Player == null)
            return;

        TryAssignScp181();
    }

    public void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        if (ev.Player == null || ev.Player.Id != Plugin.SCP181Id)
            return;

        if (ev.Door == null || ev.Door.IsLocked)
            return;

        if (ev.IsAllowed)
            return;

        if (Roll(plugin.Config.DoorLuck))
            ev.IsAllowed = true;
    }

    public void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Player == null || ev.Player.Id != Plugin.SCP181Id)
            return;

        if (ev.Attacker == null)
            return;

        if (Roll(plugin.Config.DamageAvoidLuck))
            ev.IsAllowed = false;
    }

    public void OnDied(DiedEventArgs ev)
    {
        if (ev.Player == null || ev.Player.Id != Plugin.SCP181Id)
            return;

        ClearScp181Visuals(ev.Player);
        Plugin.SCP181Id = 0;
        pendingAssignment = false;
    }

    public void OnRestartingRound()
    {
        Plugin.SCP181Id = 0;
        pendingAssignment = false;

        if (plugin.Config.Debug)
            Log.Debug("SCP181 state has been reset.");
    }

    private void TryAssignScp181()
    {
        if (!pendingAssignment)
            return;

        if (Plugin.SCP181Id != 0)
        {
            pendingAssignment = false;
            return;
        }

        if (Player.List.Count() < plugin.Config.MinimumPlayers)
            return;

        var candidates = Player.List
            .Where(p => p.IsAlive && p.Role.Type == RoleTypeId.ClassD)
            .ToList();

        if (candidates.Count == 0)
            return;

        var player = candidates[Plugin.Random.Next(candidates.Count)];
        MakeScp181(player);
        pendingAssignment = false;
    }

    private void MakeScp181(Player player)
    {
        Plugin.SCP181Id = player.Id;

        player.Role.Set(RoleTypeId.ClassD);
        player.MaxHealth = plugin.Config.MaxHealth;
        player.Health = player.MaxHealth;
        player.CustomName = null;
        player.RankName = "SCP-181";
        player.RankColor = "yellow";
        player.ClearInventory();

        foreach (var itemType in plugin.Config.StartingItems)
            player.AddItem(itemType);
    }

    private static void ClearScp181Visuals(Player player)
    {
        player.RankName = string.Empty;
        player.RankColor = string.Empty;
    }

    private static bool Roll(int percent)
    {
        if (percent <= 0)
            return false;

        if (percent >= 100)
            return true;

        return Plugin.Random.Next(0, 100) < percent;
    }
}