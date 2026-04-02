using System.Linq;
using MEC;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;

namespace SCP181.Exiled;

public sealed class EventHandlers
{
    private readonly Plugin plugin;

    public EventHandlers(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void OnRoundStarted()
    {
        if (Player.List.Count() < plugin.Config.MinimumPlayers)
            return;

        Timing.CallDelayed(3f, () =>
        {
            var candidates = Player.List.Where(p => p.IsAlive && p.Role.Type == RoleTypeId.ClassD).ToList();
            if (candidates.Count == 0)
                return;

            var player = candidates[Plugin.Random.Next(candidates.Count)];
            MakeScp181(player);
        });
    }

    public void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        if (ev.Player == null || ev.Player.Id != Plugin.SCP181Id)
            return;

        if (ev.CanOpen || ev.Door.IsLocked)
            return;

        if (Roll(plugin.Config.DoorLuck))
            ev.CanOpen = true;
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
    }

    public void OnRoundRestarting()
    {
        Plugin.SCP181Id = 0;
        Log.Debug("SCP181 state has been reset.", plugin.Config.Debug);
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
