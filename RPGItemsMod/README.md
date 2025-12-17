# RPGItemsMod

A comprehensive RPG system mod for Shape of Dreams that adds items, equipment, stats, and progression systems.

## Features

- **447+ Items**: Weapons, armor, rings, amulets, belts, and consumables
- **Full Localization**: 11 languages supported
- **Stat System**: Allocatable stat points (Strength, Vitality, Defense, Intelligence, Agility, Luck)
- **Weapon Mastery**: Level up weapons as you use them
- **Equipment System**: Full character equipment with slots
- **Monster Loot**: Dynamic loot drops based on monster type and rarity
- **Item Upgrading**: Enhance items with upgrade levels
- **Set Bonuses**: Matching rarity items grant bonus stats
- **Multiplayer Support**: Full network synchronization
- **Adaptive UI**: Scales to different screen resolutions

## Source Files

### Core Systems
- `RPGItemsMod.cs` - Main mod class and configuration
- `ItemTypes.cs` - Item data structures and enums
- `ItemDatabase.cs` - Central item repository
- `ItemTranslations.cs` - Localization for all items

### Systems
- `StatsSystem.cs` - Character stat allocation system
- `WeaponMasterySystem.cs` - Weapon progression system
- `EquipmentManager.cs` - Equipment slot management
- `InventoryManager.cs` - Inventory and item management
- `MonsterLootSystem.cs` - Loot generation and drops
- `UpgradeSystem.cs` - Item enhancement system
- `SetBonusSystem.cs` - Set bonus calculations
- `AutoTargetSystem.cs` - Auto-targeting for ranged weapons

### UI Components
- `InventoryUI.cs` - Main inventory interface
- `StatsUI.cs` - Stat allocation interface
- `WeaponMasteryUI.cs` - Weapon mastery display
- `RPGResultsUI.cs` - End-of-run results screen
- `ItemSlot.cs` - Individual item slot UI
- `EquipmentSlot.cs` - Equipment slot UI
- `FastSlotUI.cs` - Consumable quick slots
- `ConsumableBar.cs` - Consumable hotbar
- `SettingsPanel.cs` - Settings interface
- `UIHelper.cs` - UI utility functions

### Patches (Harmony)
- `ChatPatches.cs` - Multiplayer communication
- `MerchantPatches.cs` - Merchant integration
- `HiddenStashPatches.cs` - Treasure chest integration
- `ScoringPatches.cs` - Score system integration
- `AutoAimPatches.cs` - Auto-aim functionality
- `ElementalAttackPatches.cs` - Elemental damage integration

### Persistence
- `ItemPersistence.cs` - Save/load system
- `NetworkedItemSystem.cs` - Multiplayer synchronization
- `DroppedItem.cs` - Dropped item entities

### Localization
- `Localization.cs` - Localization system
- `i18n/*.json` - Translation files for all languages

## Configuration

The mod features a collapsible config menu with sections for:
- General settings
- Stat system
- Item drops and rarity weights
- Upgrade system
- Weapon mastery
- Gold/dust on kill
- Set bonuses
- Debug options

## Building

Requires references to Shape of Dreams game assemblies:
- `Dew.Core.dll`
- `Dew.UI.dll`
- `Mirror.dll`
- `Unity assemblies`

Target framework: `netstandard2.1`
