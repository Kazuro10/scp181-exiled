using System;
using CommandSystem;
using Exiled.API.Features;
using PlayerRoles;

namespace SCP181.Exiled;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class SCP181Command : ICommand
{
    public string Command => "set181";

    public string[] Aliases => new[] { "get181", "s181" };

    public string Description => "Set or query the SCP-181 player. Usage: s181 set <id> | s181 get";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Usage: s181 set <id> | s181 get";
            return false;
        }

        var action = arguments.At(0)?.ToLowerInvariant();

        switch (action)
        {
            case "set":
                if (arguments.Count < 2 || !int.TryParse(arguments.At(1), out var id))
                {
                    response = "Please provide a valid in-game player ID.";
                    return false;
                }

                var target = Player.Get(id);
                if (target == null)
                {
                    response = $"Player with ID {id} was not found.";
                    return false;
                }

                if (Plugin.SCP181Id != 0)
                {
                    var oldPlayer = Player.Get(Plugin.SCP181Id);
                    oldPlayer?.Kill("Replaced as SCP-181 by an administrator.");
                }

                target.Role.Set(RoleTypeId.ClassD);
                target.MaxHealth = Plugin.Instance.Config.MaxHealth;
                target.Health = target.MaxHealth;
                target.RankName = "SCP-181";
                target.RankColor = "yellow";
                target.ClearInventory();

                foreach (var itemType in Plugin.Instance.Config.StartingItems)
                    target.AddItem(itemType);

                Plugin.SCP181Id = target.Id;
                response = $"SCP-181 assigned to {target.Nickname} (ID {target.Id}).";
                return true;

            case "get":
                if (Plugin.SCP181Id == 0)
                {
                    response = "There is currently no SCP-181 player.";
                    return true;
                }

                var current = Player.Get(Plugin.SCP181Id);
                if (current == null)
                {
                    response = "Stored SCP-181 player is no longer valid.";
                    return true;
                }

                response = $"Current SCP-181: ID {current.Id} | Role {current.Role.Type} | Name {current.Nickname}";
                return true;

            default:
                response = "Usage: s181 set <id> | s181 get";
                return false;
        }
    }
}
