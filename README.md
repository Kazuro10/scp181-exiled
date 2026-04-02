# SCP181

Adds an SCP-181 role to the game.

## Configuration

```yaml
# Enable or disable this plugin.
is_enabled: true
# Enable extra debug messages in the server console.
debug: false

# Percent chance for SCP-181 to open a denied but unlocked door.
door_luck: 10
# Percent chance for SCP-181 to negate incoming damage.
damage_avoid_luck: 10
# Maximum health assigned to SCP-181.
max_health: 150
# Minimum number of players required before SCP-181 can be assigned.
minimum_players: 1

# Items given to SCP-181 on assignment.
starting_items:
- KeycardJanitor
- Medkit
- Coin
```

## Installation

1. Download `SCP181.Exiled.dll`
2. Put it in your EXILED plugins folder
3. Restart the server
