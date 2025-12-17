# Shape of Dreams - Mods Collection

This repository contains two major mods for the game **Shape of Dreams**:

## 🎮 Mods Included

### 1. RPGItemsMod
A comprehensive RPG system that adds:
- **447+ items** with full localization (11 languages)
- Equipment system (weapons, armor, accessories)
- Stat system with allocatable points
- Weapon mastery progression
- Monster loot drops with rarity tiers
- Item upgrading and enhancement
- Set bonuses
- Consumable items and fast slots
- Merchant integration
- Full multiplayer support

### 2. InfiniteDungeonMod
An endless dungeon exploration mode featuring:
- Procedurally generated infinite dungeon layouts
- Dynamic difficulty scaling
- Special room modifiers (upgrade wells, minibosses, treasure)
- Hunter infection system
- Weakened boss encounters
- Pure White Rift endgame content
- Platinum coin rewards
- Full localization support

## 🌍 Supported Languages

Both mods support 11 languages:
- English (en-US)
- Chinese Simplified (zh-CN)
- Chinese Traditional (zh-TW)
- Japanese (ja-JP)
- Korean (ko-KR)
- Polish (pl-PL)
- German (de-DE)
- French (fr-FR)
- Spanish (es-MX)
- Portuguese (pt-BR)
- Russian (ru-RU)
- Italian (it-IT)
- Turkish (tr-TR)

## 🛠️ Building

### Requirements
- .NET Framework 4.x
- C# 5 compiler (included with .NET Framework)
- Shape of Dreams game installed
- BepInEx mod loader

### Build Instructions

Each mod has its own build script in the original distribution:
- `RPGItemsMod/compile_rpgitems.bat`
- `InfiniteDungeonMod/compile.bat`

The mods target `netstandard2.1` and use the game's assemblies as references.

## 📁 Project Structure

```
shape-of-dreams-mods/
├── RPGItemsMod/
│   ├── *.cs                    # Source files
│   └── i18n/                   # Localization files
│       └── *.json
├── InfiniteDungeonMod/
│   ├── *.cs                    # Source files
│   └── i18n/                   # Localization files
│       └── *.json
└── README.md
```

## 🎯 Features Highlights

### RPGItemsMod
- **Adaptive UI Scaling**: Automatically adjusts to different screen resolutions and user UI scale settings
- **Collapsible Config Sections**: Organized settings menu with expandable sections
- **Diablo-style Layout**: Optional UI layout inspired by Diablo games
- **Item Persistence**: Saves inventory and equipment between sessions
- **Network Sync**: Full multiplayer synchronization for stats and items

### InfiniteDungeonMod
- **Endless Exploration**: No zone limits, explore infinitely
- **Dynamic Branching**: Multiple paths and choices at each node
- **Special Events**: Hunter infections, weakened bosses, cursed nodes
- **Adaptive UI**: Scales properly for all screen sizes
- **Collapsible Config**: Clean, organized settings interface

## 📝 License

These mods are provided as-is for the Shape of Dreams community.

## 👥 Credits

Developed by Julius (Chino) for the Shape of Dreams community.

## 🐛 Known Issues

- Multiplayer stat synchronization may require testing
- Some UI elements use Unity's legacy Text component (may be migrated to TextMeshPro in future)

## 🔮 Future Plans

Development has concluded, but the source code is available for community contributions.
