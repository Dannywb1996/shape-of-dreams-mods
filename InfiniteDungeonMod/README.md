# InfiniteDungeonMod

An endless dungeon exploration mode for Shape of Dreams with procedural generation and dynamic difficulty.

## Features

- **Infinite Exploration**: No zone limits, endless dungeon generation
- **Procedural Generation**: Dynamic branching paths with loops and dead ends
- **Dynamic Difficulty**: Monsters scale with depth
- **Room Modifiers**: 
  - Upgrade Wells
  - MiniBoss encounters
  - Hidden treasure stashes
  - Gold/Stardust bonuses
- **Hunter System**: Infection mechanic that spreads through the dungeon
- **Weakened Bosses**: Rare chance for easier boss encounters
- **Pure White Rift**: Endgame content at high depths
- **Platinum Coins**: Special currency drops from bosses
- **Full Localization**: 13 languages supported
- **Adaptive UI**: Scales to different screen resolutions
- **Collapsible Config**: Organized settings menu

## Source Files

### Core
- `InfiniteDungeonMod.cs` - Main mod class, configuration, and intro window
- `DungeonGeneration.cs` - Procedural dungeon generation algorithms
- `DungeonUI.cs` - Result window and UI elements
- `DungeonLocalization.cs` - Localization system

### Systems
- `ModifierSystem.cs` - Room modifier logic (wells, treasure, etc.)
- `HunterSystem.cs` - Infection and Hunter encounter system
- `WeakenedBossSystem.cs` - Weakened boss mechanics
- `ConnectionSyncSystem.cs` - Multiplayer synchronization

### Patches (Harmony)
- `DungeonPatches.cs` - Game integration patches

### Localization
- `i18n/*.json` - Translation files for all languages

## Configuration

The mod features a collapsible config menu with sections for:
- Dungeon layout (branches, nodes, connections)
- Monster scaling (health, damage, speed)
- Monster stats (armor, abilities)
- Room modifiers (weights for different room types)
- Pure White Rift settings
- Platinum coin drop rates

## Building

Requires references to Shape of Dreams game assemblies:
- `Dew.Core.dll`
- `Dew.UI.dll`
- `Mirror.dll`
- `Unity assemblies`

Target framework: `netstandard2.1`

## Gameplay

1. Start a new game with the mod enabled
2. An intro window explains the mechanics
3. Explore infinitely generated dungeon nodes
4. Depth increases only when visiting new nodes
5. Face increasing difficulty and special encounters
6. Reach Pure White Rift at depth 50+
7. Earn bonuses based on performance

## Technical Details

- Uses Harmony patches to integrate with game systems
- Procedural generation uses seeded random for consistency
- Node positions are tracked for map visualization
- Multiplayer-safe with host authority
- UI scales automatically based on screen resolution (1440p reference)
