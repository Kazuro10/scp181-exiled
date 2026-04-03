using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;

namespace SCP181.Exiled;

public sealed class EventHandlers
{
    private readonly Plugin plugin;
    private bool pendingAssignment;
    private readonly Dictionary<Door, DateTime> doorCooldowns = new();

    public EventHandlers(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void OnRoundStarted()
    {
        doorCooldowns.Clear();

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

        if (IsDoorOnCooldown(ev.Door, out double remainingSeconds))
        {
            if (plugin.Config.Debug)
                Log.Debug($"SCP-181 door luck blocked by per-door cooldown. Door={ev.Door.Type}, Remaining={remainingSeconds:F2}s");

            return;
        }

        SetDoorCooldown(ev.Door);

        if (Roll(plugin.Config.DoorLuck))
        {
            ev.IsAllowed = true;

            if (plugin.Config.Debug)
                Log.Debug($"SCP-181 lucky door success. Door={ev.Door.Type}");
        }
        else if (plugin.Config.Debug)
        {
            Log.Debug($"SCP-181 lucky door failed. Door={ev.Door.Type}");
        }
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

        Plugin.SCP181Id = 0;
        pendingAssignment = false;
        doorCooldowns.Clear();
    }

    public void OnRestartingRound()
    {
        Plugin.SCP181Id = 0;
        pendingAssignment = false;
        doorCooldowns.Clear();

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
        player.ClearInventory();

        foreach (var itemType in plugin.Config.StartingItems)
            player.AddItem(itemType);
    }

    private bool IsDoorOnCooldown(Door door, out double remainingSeconds)
    {
        remainingSeconds = 0;

        if (!doorCooldowns.TryGetValue(door, out DateTime lastAttempt))
            return false;

        double elapsed = (DateTime.UtcNow - lastAttempt).TotalSeconds;
        double cooldown = plugin.Config.DoorLuckPerDoorCooldown;

        if (elapsed >= cooldown)
            return false;

        remainingSeconds = cooldown - elapsed;
        return true;
    }

    private void SetDoorCooldown(Door door)
    {
        doorCooldowns[door] = DateTime.UtcNow;
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