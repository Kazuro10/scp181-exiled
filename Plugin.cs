using System;
using Exiled.API.Features;
using PlayerRoles;
using ServerHandler = Exiled.Events.Handlers.Server;
using PlayerHandler = Exiled.Events.Handlers.Player;

namespace SCP181.Exiled;

public sealed class Plugin : Plugin<Config>
{
    public override string Name => "SCP181";

    public override string Author => "YF-OFFICE / ported by ChatGPT for Kotek";

    public override string Prefix => "scp181";

    public override Version Version { get; } = new(1, 0, 0);

    public override Version RequiredExiledVersion { get; } = new(9, 13, 1);

    public static Plugin Instance { get; private set; } = null!;

    public static int SCP181Id { get; set; }

    public static Random Random { get; } = new();

    public EventHandlers Handlers { get; private set; } = null!;

    public override void OnEnabled()
    {
        Instance = this;
        Handlers = new EventHandlers(this);

        ServerHandler.RoundStarted += Handlers.OnRoundStarted;
        ServerHandler.RoundRestarting += Handlers.OnRoundRestarting;
        PlayerHandler.InteractingDoor += Handlers.OnInteractingDoor;
        PlayerHandler.Hurting += Handlers.OnHurting;
        PlayerHandler.Died += Handlers.OnDied;

        Log.Info("SCP181 EXILED plugin enabled.");
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        ServerHandler.RoundStarted -= Handlers.OnRoundStarted;
        ServerHandler.RoundRestarting -= Handlers.OnRoundRestarting;
        PlayerHandler.InteractingDoor -= Handlers.OnInteractingDoor;
        PlayerHandler.Hurting -= Handlers.OnHurting;
        PlayerHandler.Died -= Handlers.OnDied;

        Handlers = null!;
        SCP181Id = 0;
        Instance = null!;

        Log.Info("SCP181 EXILED plugin disabled.");
        base.OnDisabled();
    }
}
