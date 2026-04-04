using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using UnityEngine;

namespace SCP181.Exiled;

public sealed class EventHandlers
{
    private readonly Plugin plugin;
    private bool pendingAssignment;

    private readonly Dictionary<int, float> doorCooldowns = new();

    public EventHandlers(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void OnRoundStarted()
    {
        doorCooldowns.Clear();

        if (Player.List.Count() < plugin.Config.MinimumPlayers)
            return;

        if (!Roll(plugin.Config.SpawnChance))
        {
            pendingAssignment = false;

            if (plugin.Config.Debug)
                Log.Debug("SCP-181 spawn roll failed this round.");

            return;
        }

        pendingAssignment = true;
        TryAssignScp181();
    }

    public void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev.Player != null && ev.Player.Id == Plugin.SCP181Id && ev.NewRole != RoleTypeId.ClassD)
        {
            ClearScp181Visuals(ev.Player);
            Plugin.SCP181Id = 0;
            pendingAssignment = false;
            doorCooldowns.Clear();

            if (plugin.Config.Debug)
                Log.Debug("SCP-181 lost status because role changed away from Class-D.");
        }

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

        int doorKey = GetDoorKey(ev);

        if (IsDoorOnCooldown(doorKey))
        {
            if (plugin.Config.Debug)
                Log.Debug($"Lucky door roll blocked by cooldown for door {doorKey}.");

            return;
        }

        SetDoorCooldown(doorKey);

        if (Roll(plugin.Config.DoorLuck))
        {
            ev.IsAllowed = true;

            if (plugin.Config.Debug)
                Log.Debug($"SCP-181 lucky-opened door {doorKey}.");
        }
        else if (plugin.Config.Debug)
        {
            Log.Debug($"SCP-181 failed lucky-open roll for door {doorKey}.");
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

        ClearScp181Visuals(ev.Player);
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
        player.CustomInfo = "SCP-181";
        player.ClearInventory();

        foreach (var itemType in plugin.Config.StartingItems)
            player.AddItem(itemType);
    }

    private static void ClearScp181Visuals(Player player)
    {
        player.CustomInfo = string.Empty;
    }

    private int GetDoorKey(InteractingDoorEventArgs ev)
    {
        return ev.Door.Base.GetInstanceID();
    }

    private bool IsDoorOnCooldown(int doorKey)
    {
        return doorCooldowns.TryGetValue(doorKey, out float readyAt) && Time.time < readyAt;
    }

    private void SetDoorCooldown(int doorKey)
    {
        doorCooldowns[doorKey] = Time.time + plugin.Config.DoorLuckCooldownSeconds;
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