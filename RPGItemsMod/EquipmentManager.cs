using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Equipment Manager - Handles equipment slots and applies real stat bonuses to the game
/// Each item type can only go in its specific slot
/// </summary>
public class EquipmentManager
{
    private Dictionary<EquipmentSlotType, RPGItem> equippedItems = new Dictionary<EquipmentSlotType, RPGItem>();
    
    /// <summary>
    /// Public accessor for equipped items (for SetBonusSystem)
    /// </summary>
    public Dictionary<EquipmentSlotType, RPGItem> EquippedItems { get { return equippedItems; } }
    private Dictionary<EquipmentSlotType, StatBonus> statBonuses = new Dictionary<EquipmentSlotType, StatBonus>();
    private Dictionary<EquipmentSlotType, SkillBonus> skillBonuses = new Dictionary<EquipmentSlotType, SkillBonus>();
    
    // Reference to inventory manager for returning displaced items
    private InventoryManager _inventoryManager;
    
    // Set Bonus System - grants bonuses for wearing items of same rarity
    private SetBonusSystem _setBonusSystem;
    
    // Track bonus stats from our mod's equipment
    private int bonusAttack = 0;
    private int bonusDefense = 0;
    private int bonusHealth = 0;
    private int bonusAbilityPower = 0;
    private float bonusMoveSpeed = 0f;
    private int bonusDodgeCharges = 0;
    private float bonusDodgeCooldownReduction = 0f;
    private int bonusGoldOnKill = 0;
    private int bonusDustOnKill = 0;
    private float bonusLifesteal = 0f;
    private float bonusThorns = 0f;
    private float bonusRegeneration = 0f;
    private float bonusShieldRegeneration = 0f;
    private float bonusAttackSpeed = 0f;
    private float bonusMemoryHaste = 0f;
    private float bonusCritChance = 0f;
    private float bonusCritDamage = 0f;
    
    // Public accessor for dodge cooldown reduction (for Harmony patches)
    public float DodgeCooldownReduction { get { return bonusDodgeCooldownReduction; } }
    
    // On-hit tracking
    private Action<EventInfoDamage> onDealDamageHandler;
    private Action<EventInfoDamage> onTakeDamageHandler;
    private bool isSubscribedToDamageEvent = false;
    private bool isSubscribedToTakeDamageEvent = false;
    
    // Flag to track if stats need to be reapplied (hero wasn't ready during load)
    private bool _needsStatReapply = false;
    
    // Elemental attack tracking
    private ElementalType? activeElementalType = null;
    private int activeElementalStacks = 0;
    
    // Shield regeneration timer
    private float shieldRegenTimer = 0f;
    
    // Balance limits for weapon elemental effects
    public const int MAX_FIRE_STACKS = 3;       // Fire DoT stacks cap
    public const int MAX_LIGHT_STACKS = 3;      // Light damage amp stacks cap (+15% max)
    public const int MAX_DARK_STACKS = 3;       // Dark damage amp stacks cap (+15% max)
    public const float COLD_SLOW_REDUCTION = 0.5f;    // Cold slow reduced by 50%
    public const float COLD_CRIPPLE_REDUCTION = 0.5f; // Cold cripple reduced by 50%

    public EquipmentManager()
    {
        // Initialize set bonus system
        _setBonusSystem = new SetBonusSystem(this);
    }

    public void SetInventoryManager(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager;
    }
    
    /// <summary>
    /// Get the set bonus system for UI access
    /// </summary>
    public SetBonusSystem SetBonusSystem { get { return _setBonusSystem; } }
    
    public void Initialize()
    {
        // Initialize all 10 equipment slots
        equippedItems[EquipmentSlotType.Head] = null;
        equippedItems[EquipmentSlotType.Amulet] = null;
        equippedItems[EquipmentSlotType.Chest] = null;
        equippedItems[EquipmentSlotType.Belt] = null;
        equippedItems[EquipmentSlotType.Legs] = null;
        equippedItems[EquipmentSlotType.Boots] = null;
        equippedItems[EquipmentSlotType.LeftHand] = null;
        equippedItems[EquipmentSlotType.RightHand] = null;
        equippedItems[EquipmentSlotType.LeftRing] = null;
        equippedItems[EquipmentSlotType.RightRing] = null;
    }

    /// <summary>
    /// Check if an item can be equipped in a specific slot
    /// Each item type has specific slots it can go into
    /// </summary>
    public bool CanEquip(RPGItem item, EquipmentSlotType slotType)
    {
        if (item == null) return false;
        
        // Get hero for requirement checks
        Hero hero = DewPlayer.local != null ? DewPlayer.local.hero : null;
        
        // Check hero requirement
        if (!string.IsNullOrEmpty(item.requiredHero))
        {
            if (hero == null)
            {
                RPGLog.Debug(" Cannot equip " + item.name + " - no hero found");
                return false;
            }
            
            string heroTypeName = hero.GetType().Name;
            
            if (!RPGItem.HeroMatchesRequirement(heroTypeName, item.requiredHero))
            {
                // This is expected behavior - UI should prevent equipping incompatible items
                RPGLog.Debug(" Cannot equip " + item.name + " - requires " + item.requiredHero + ", you are " + heroTypeName);
                return false;
            }
        }

        // Check for Vesper-specific restrictions (max 1 hammer, max 1 shield)
        bool isVesper = hero != null && hero.GetType().Name == "Hero_Vesper";
        
        if (isVesper && item.requiredHero == "Hero_Vesper")
        {
            // Vesper can only equip 1 hammer (Weapon) total
            if (item.type == ItemType.Weapon)
            {
                RPGItem leftHand = GetEquipped(EquipmentSlotType.LeftHand);
                RPGItem rightHand = GetEquipped(EquipmentSlotType.RightHand);
                
                // Check if there's already a hammer in the other hand
                if (slotType == EquipmentSlotType.RightHand && leftHand != null && leftHand.type == ItemType.Weapon && leftHand.requiredHero == "Hero_Vesper")
                {
                    RPGLog.Warning(" Vesper can only equip 1 hammer. Unequip the other hammer first.");
                    return false;
                }
                if (slotType == EquipmentSlotType.LeftHand && rightHand != null && rightHand.type == ItemType.Weapon && rightHand.requiredHero == "Hero_Vesper")
                {
                    RPGLog.Warning(" Vesper can only equip 1 hammer. Unequip the other hammer first.");
                    return false;
                }
            }
            
            // Vesper can only equip 1 shield (OffHand) total
            if (item.type == ItemType.OffHand)
            {
                RPGItem leftHand = GetEquipped(EquipmentSlotType.LeftHand);
                RPGItem rightHand = GetEquipped(EquipmentSlotType.RightHand);
                
                // Check if there's already a shield equipped
                if ((leftHand != null && leftHand.type == ItemType.OffHand && leftHand.requiredHero == "Hero_Vesper") ||
                    (rightHand != null && rightHand.type == ItemType.OffHand && rightHand.requiredHero == "Hero_Vesper"))
                {
                    RPGLog.Warning(" Vesper can only equip 1 shield. Unequip the other shield first.");
                    return false;
                }
            }
        }

        switch (slotType)
        {
            // Head slot - only helmets
            case EquipmentSlotType.Head:
                return item.type == ItemType.Helmet;
            
            // Chest slot - only chest armor
            case EquipmentSlotType.Chest:
                return item.type == ItemType.ChestArmor;
            
            // Legs slot - only pants
            case EquipmentSlotType.Legs:
                return item.type == ItemType.Pants;
            
            // Boots slot - only boots
            case EquipmentSlotType.Boots:
                return item.type == ItemType.Boots;
            
            // Belt slot - only belts
            case EquipmentSlotType.Belt:
                return item.type == ItemType.Belt;
            
            // Right Hand - main hand weapon or two-handed weapon
            case EquipmentSlotType.RightHand:
                return item.type == ItemType.Weapon || item.type == ItemType.TwoHandedWeapon;
            
            // Left Hand - off-hand (shield), weapon, or two-handed weapon
            // Two-handed weapons can go in either hand slot
            case EquipmentSlotType.LeftHand:
                return item.type == ItemType.Weapon || item.type == ItemType.OffHand || item.type == ItemType.TwoHandedWeapon;
            
            // Amulet slot - only amulets
            case EquipmentSlotType.Amulet:
                return item.type == ItemType.Amulet;
            
            // Ring slots - only rings
            case EquipmentSlotType.LeftRing:
            case EquipmentSlotType.RightRing:
                return item.type == ItemType.Ring;
            
            default:
                return false;
        }
    }

    public RPGItem Equip(RPGItem item, EquipmentSlotType slotType)
    {
        if (!CanEquip(item, slotType)) 
        {
            // This is expected validation - UI should prevent invalid equips
            RPGLog.Debug(" Cannot equip " + item.name + " (" + item.type + ") to " + slotType);
            return item;
        }

        // Handle two-handed weapons - clear the OTHER hand when equipping
        if (item.type == ItemType.TwoHandedWeapon)
        {
            if (slotType == EquipmentSlotType.RightHand)
            {
                // Equipping to right hand - clear left hand and return to inventory
                RPGItem offHandItem = GetEquipped(EquipmentSlotType.LeftHand);
                if (offHandItem != null)
                {
                    Unequip(EquipmentSlotType.LeftHand);
                    // Return the displaced item to inventory
                    if (_inventoryManager != null)
                    {
                        _inventoryManager.AddItem(offHandItem);
                        RPGLog.Debug(" Returned " + offHandItem.name + " to inventory (displaced by two-handed weapon)");
                    }
                }
            }
            else if (slotType == EquipmentSlotType.LeftHand)
            {
                // Equipping to left hand - clear right hand and return to inventory
                RPGItem mainHandItem = GetEquipped(EquipmentSlotType.RightHand);
                if (mainHandItem != null)
                {
                    Unequip(EquipmentSlotType.RightHand);
                    // Return the displaced item to inventory
                    if (_inventoryManager != null)
                    {
                        _inventoryManager.AddItem(mainHandItem);
                        RPGLog.Debug(" Returned " + mainHandItem.name + " to inventory (displaced by two-handed weapon)");
                    }
                }
            }
        }
        else if (slotType == EquipmentSlotType.LeftHand)
        {
            // Block off-hand equip if two-handed weapon is equipped in right hand
            RPGItem mainHandItem = GetEquipped(EquipmentSlotType.RightHand);
            if (mainHandItem != null && mainHandItem.type == ItemType.TwoHandedWeapon)
            {
                // This is expected validation - two-handed weapons block off-hand
                RPGLog.Debug(" Cannot equip off-hand while two-handed weapon is equipped");
                return item;
            }
        }
        else if (slotType == EquipmentSlotType.RightHand)
        {
            // Block right-hand equip if two-handed weapon is equipped in left hand
            RPGItem leftHandItem = GetEquipped(EquipmentSlotType.LeftHand);
            if (leftHandItem != null && leftHandItem.type == ItemType.TwoHandedWeapon)
            {
                // This is expected validation - two-handed weapons block other hand
                RPGLog.Debug(" Cannot equip right-hand while two-handed weapon is equipped in left hand");
                return item;
            }
        }

        // Unequip current item first
        RPGItem previousItem = null;
        if (equippedItems.ContainsKey(slotType))
        {
            previousItem = equippedItems[slotType];
        }
        
        if (previousItem != null)
        {
            RemoveItemStats(slotType, previousItem);
        }

        // Equip new item
        equippedItems[slotType] = item;
        if (item != null)
        {
            ApplyItemStats(item, slotType);
        }

        // Recalculate set bonuses after equipment change
        if (_setBonusSystem != null)
        {
            _setBonusSystem.OnEquipmentChanged();
        }

        RPGLog.Debug(" Equipped " + item.name + " to " + slotType);
        return previousItem;
    }

    public RPGItem Unequip(EquipmentSlotType slotType)
    {
        if (!equippedItems.ContainsKey(slotType)) return null;
        
        RPGItem item = equippedItems[slotType];
        if (item != null)
        {
            RemoveItemStats(slotType, item);
            equippedItems[slotType] = null;
            RPGLog.Debug(" Unequipped " + item.name + " from " + slotType);
            
            // Recalculate set bonuses after equipment change
            if (_setBonusSystem != null)
            {
                _setBonusSystem.OnEquipmentChanged();
            }
        }
        return item;
    }

    public RPGItem GetEquipped(EquipmentSlotType slotType)
    {
        if (equippedItems.ContainsKey(slotType))
        {
            return equippedItems[slotType];
        }
        return null;
    }

    private void ApplyItemStats(RPGItem item, EquipmentSlotType slotType)
    {
        // Track bonus stats locally
        bonusAttack += item.attackBonus;
        bonusDefense += item.defenseBonus;
        bonusHealth += item.healthBonus;
        bonusAbilityPower += item.abilityPowerBonus;
        bonusMoveSpeed += item.moveSpeedBonus;
        bonusDodgeCharges += item.dodgeChargesBonus;
        bonusDodgeCooldownReduction += item.dodgeCooldownReduction;
        bonusGoldOnKill += item.goldOnKill;
        bonusDustOnKill += item.dustOnKill;
        bonusLifesteal += item.lifeSteal;
        bonusThorns += item.thorns;
        bonusRegeneration += item.regeneration;
        bonusAttackSpeed += item.attackSpeed;
        bonusMemoryHaste += item.memoryHaste;
        bonusCritChance += item.criticalChance;
        bonusCritDamage += item.criticalDamage;
        
        // Subscribe to damage events if we have on-hit effects (lifesteal only now)
        if (bonusLifesteal > 0)
        {
            SubscribeToOnHitEffects();
        }
        
        // Subscribe to take damage event if we have thorns
        if (bonusThorns > 0)
        {
            SubscribeToThornsEffect();
        }
        
        // Sync on-kill bonuses to server (for multiplayer)
        SyncOnKillBonuses();
        
        // Get the local player's hero (store reference to avoid race condition)
        Hero hero = null;
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            hero = DewPlayer.local.hero;
        }
        
        if (hero == null)
        {
            RPGLog.Debug(" Cannot apply stats - no local hero");
            return;
        }
        
        // Create stat bonus with ALL applicable stats
        StatBonus bonus = new StatBonus();
        bonus.attackDamageFlat = item.attackBonus;
        bonus.armorFlat = item.defenseBonus;
        bonus.maxHealthFlat = item.healthBonus;
        bonus.abilityPowerFlat = item.abilityPowerBonus;
        bonus.movementSpeedPercentage = item.moveSpeedBonus;
        bonus.healthRegenFlat = item.regeneration;           // HP per second
        bonus.attackSpeedPercentage = item.attackSpeed;      // % attack speed
        bonus.abilityHasteFlat = item.memoryHaste;           // Memory/ability haste
        // Game expects crit values as decimals (3% = 0.03), our items store as whole numbers (3)
        bonus.critChanceFlat = item.criticalChance / 100f;   // Convert 3 -> 0.03
        bonus.critAmpFlat = item.criticalDamage / 100f;      // Convert 50 -> 0.5
        
        // Server applies stats directly, clients request via customData
        if (NetworkServer.active)
        {
            // Remove existing stat bonus for this slot first (if any)
            if (statBonuses.ContainsKey(slotType) && statBonuses[slotType] != null)
            {
                try { hero.Status.RemoveStatBonus(statBonuses[slotType]); } catch { }
            }
            
            StatBonus addedBonus = hero.Status.AddStatBonus(bonus);
            statBonuses[slotType] = addedBonus;
            
            // Apply dodge charges and cooldown reduction via SkillBonus on Movement skill
            if ((item.dodgeChargesBonus != 0 || item.dodgeCooldownReduction > 0) && hero.Skill != null && hero.Skill.Movement != null)
            {
                // IMPORTANT: Stop existing skill bonus for this slot first to prevent accumulation!
                if (skillBonuses.ContainsKey(slotType) && skillBonuses[slotType] != null)
                {
                    try { skillBonuses[slotType].Stop(); } catch { }
                }
                
                SkillBonus skillBonus = hero.Skill.Movement.AddSkillBonus(new SkillBonus
                {
                    addedCharge = item.dodgeChargesBonus,
                    // cooldownMultiplier: 1.0 = normal, 0.8 = 20% faster (so 20% reduction = 0.8 multiplier)
                    cooldownMultiplier = item.dodgeCooldownReduction > 0 ? (1f - item.dodgeCooldownReduction / 100f) : 1f
                });
                skillBonuses[slotType] = skillBonus;
            }
        }
        else
        {
            // Client: request server to apply stats (including dodge charges)
            RequestStatChange(hero.netId, (int)slotType, item, true);
        }
    }
    
    private void RequestStatChange(uint heroNetId, int slotType, RPGItem item, bool add)
    {
        // Use chat to request stat change from host
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Format: [RPGSTAT3]heroNetId|slotType|atk|def|hp|ap|moveSpd|dodgeCharges|dodgeCdRed|regen|atkSpd|haste|critCh|critDmg|add
            string op = add ? "A" : "R";
            string content = string.Format("[RPGSTAT3]{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                heroNetId, slotType, 
                item.attackBonus, item.defenseBonus, item.healthBonus, item.abilityPowerBonus,
                (int)item.moveSpeedBonus, item.dodgeChargesBonus, (int)item.dodgeCooldownReduction,
                (int)item.regeneration, (int)item.attackSpeed, (int)item.memoryHaste,
                (int)item.criticalChance, (int)item.criticalDamage, op);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch { }
    }

    private void RemoveItemStats(EquipmentSlotType slotType, RPGItem item)
    {
        // Track bonus stats locally
        if (item != null)
        {
            bonusAttack -= item.attackBonus;
            bonusDefense -= item.defenseBonus;
            bonusHealth -= item.healthBonus;
            bonusAbilityPower -= item.abilityPowerBonus;
            bonusMoveSpeed -= item.moveSpeedBonus;
            bonusDodgeCharges -= item.dodgeChargesBonus;
            bonusDodgeCooldownReduction -= item.dodgeCooldownReduction;
            bonusGoldOnKill -= item.goldOnKill;
            bonusDustOnKill -= item.dustOnKill;
            bonusLifesteal -= item.lifeSteal;
            bonusThorns -= item.thorns;
            bonusRegeneration -= item.regeneration;
            bonusShieldRegeneration -= item.shieldRegeneration;
            bonusAttackSpeed -= item.attackSpeed;
            bonusMemoryHaste -= item.memoryHaste;
            bonusCritChance -= item.criticalChance;
            bonusCritDamage -= item.criticalDamage;
        }
        
        // Unsubscribe from damage event if no more on-hit effects (lifesteal only)
        if (bonusLifesteal <= 0)
        {
            UnsubscribeFromOnHitEffects();
        }
        
        // Unsubscribe from take damage event if no more thorns
        if (bonusThorns <= 0)
        {
            UnsubscribeFromThornsEffect();
        }
        
        // Sync on-kill bonuses to server (for multiplayer)
        SyncOnKillBonuses();

        // Store hero reference locally to avoid race condition
        Hero hero = null;
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            hero = DewPlayer.local.hero;
        }
        
        if (hero == null) return;
        
        if (NetworkServer.active)
        {
            // Remove stat bonus
            if (statBonuses.ContainsKey(slotType))
            {
                StatBonus bonus = statBonuses[slotType];
                if (bonus != null)
                {
                    hero.Status.RemoveStatBonus(bonus);
                }
                statBonuses.Remove(slotType);
            }
            
            // Remove skill bonus (dodge charges)
            if (skillBonuses.ContainsKey(slotType))
            {
                SkillBonus skillBonus = skillBonuses[slotType];
                if (skillBonus != null)
                {
                    skillBonus.Stop();
                }
                skillBonuses.Remove(slotType);
            }
        }
        else if (item != null)
        {
            // Client: request server to remove stats
            RequestStatChange(hero.netId, (int)slotType, item, false);
        }
    }

    public void UsePotion(RPGItem potion)
    {
        if (potion == null || potion.type != ItemType.Consumable) return;
        
        if (DewPlayer.local == null || DewPlayer.local.hero == null)
        {
            RPGLog.Debug(" Cannot use potion - no local hero");
            return;
        }

        Hero hero = DewPlayer.local.hero;
        float healAmount = hero.Status.maxHealth * (potion.healPercentage / 100f);
        float shieldAmount = hero.Status.maxHealth * (potion.shieldPercentage / 100f);
        
        if (NetworkServer.active)
        {
            // Host: Direct heal
            if (healAmount > 0)
            {
                HealData healData = new HealData(healAmount);
                healData.SetActor(hero);
                hero.DoHeal(healData, hero);
            }
            
            // Apply shield if potion has shield effect
            if (shieldAmount > 0)
            {
                // Use game's GiveShield method - 10 second duration, non-decaying
                hero.GiveShield(hero, shieldAmount, 10f, false);
            }
        }
        else
        {
            // Client: request server to heal and shield with VFX
            RequestHealWithVFX(hero.netId, (int)healAmount, (int)shieldAmount);
        }
        
        string logMsg = string.Format("[RPGItemsMod] Used {0}, healing {1} HP", potion.name, healAmount);
        if (shieldAmount > 0)
        {
            logMsg += string.Format(" + {0} shield", shieldAmount);
        }
        RPGLog.Debug(logMsg);
    }
    
    private void PlayHealEffect(Hero hero)
    {
        try
        {
            // Try to use the game's level up effect (green sparkles) or similar
            GameObject levelUpPrefab = Resources.Load<GameObject>("Effects/HeroLevelUp");
            if (levelUpPrefab != null)
            {
                GameObject effect = UnityEngine.Object.Instantiate(levelUpPrefab, hero.transform.position, Quaternion.identity);
                // Tint it green if possible
                ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in particles)
                {
                    ParticleSystem.MainModule main = ps.main;
                    main.startColor = new Color(0.3f, 1f, 0.4f, 1f); // Green tint
                }
                UnityEngine.Object.Destroy(effect, 2f);
                return;
            }
            
            // Fallback: simple green flash on hero using LineRenderer (more reliable than particles)
            GameObject flashObj = new GameObject("HealFlash");
            flashObj.transform.SetParent(hero.transform);
            flashObj.transform.localPosition = Vector3.up * 1f;
            
            // Create a simple expanding ring using LineRenderer
            LineRenderer line = flashObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 32;
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.startColor = new Color(0.2f, 1f, 0.3f, 1f);
            line.endColor = new Color(0.2f, 1f, 0.3f, 1f);
            
            // Use unlit shader
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.material.color = new Color(0.2f, 1f, 0.3f, 1f);
            
            // Create circle points
            float radius = 0.5f;
            for (int i = 0; i < 32; i++)
            {
                float angle = i * Mathf.PI * 2f / 32f;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
            }
            
            // Animate and destroy
            HealFlashAnimator animator = flashObj.AddComponent<HealFlashAnimator>();
            animator.line = line;
        }
        catch (System.Exception e)
        {
            RPGLog.Warning(" Failed to play heal effect: " + e.Message);
        }
    }
    
    private void RequestHeal(uint heroNetId, int healAmount, int shieldAmount = 0)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Format: [RPGHEAL]heroNetId|healAmount|shieldAmount
            string content = string.Format("[RPGHEAL]{0}|{1}|{2}", heroNetId, healAmount, shieldAmount);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch { }
    }
    
    private void RequestHealWithVFX(uint heroNetId, int healAmount, int shieldAmount = 0)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Format: [RPGHEALVFX]heroNetId|healAmount|shieldAmount
            // Server will use Se_GenericHealOverTime for proper VFX
            string content = string.Format("[RPGHEALVFX]{0}|{1}|{2}", heroNetId, healAmount, shieldAmount);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch { }
    }

    public string GetDetailedStats()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null)
        {
            return Localization.NoHeroActive;
        }

        Hero hero = DewPlayer.local.hero;
        EntityStatus status = hero.Status;
        
        string result = "";
        
        // Compact base stats with gear bonus inline
        result += string.Format("<color=#FF6666>{0}:</color> {1:#,##0}", Localization.HP, status.maxHealth);
        if (bonusHealth > 0) result += string.Format(" <color=#95e66b>(+{0})</color>", bonusHealth);
        result += "\n";
        
        result += string.Format("<color=#FFAA00>{0}:</color> {1:#,##0}", Localization.ATK, status.attackDamage);
        if (bonusAttack > 0) result += string.Format(" <color=#ff6b6b>(+{0})</color>", bonusAttack);
        result += "\n";
        
        // Calculate damage reduction using game's formula: 100 / (100 + armor)
        float damageMultiplier = 100f / (100f + status.armor);
        float damageReduction = (1f - damageMultiplier) * 100f;
        string reductionLabel = Localization.CurrentLanguage == ModLanguage.Chinese ? "减伤" : "DR";
        result += string.Format("<color=#6699FF>{0}:</color> {1:#,##0} <color=#888>({2}:{3:F1}%)</color>", Localization.DEF, status.armor, reductionLabel, damageReduction);
        if (bonusDefense > 0) result += string.Format(" <color=#4ecdc4>(+{0})</color>", bonusDefense);
        result += "\n";
        
        result += string.Format("<color=#CC66FF>{0}:</color> {1:#,##0}", Localization.AP, status.abilityPower);
        if (bonusAbilityPower > 0) result += string.Format(" <color=#ff6b6b>(+{0})</color>", bonusAbilityPower);
        
        // Compact gear effects - multiple per line
        List<string> effects = new List<string>();
        bool isChinese = Localization.CurrentLanguage == ModLanguage.Chinese;
        
        if (bonusCritChance > 0) effects.Add(string.Format("<color=#fd79a8>+{0}%{1}</color>", bonusCritChance, isChinese ? "暴" : "Crit"));
        if (bonusCritDamage > 0) effects.Add(string.Format("<color=#e17055>+{0}%{1}</color>", bonusCritDamage, isChinese ? "暴伤" : "CritD"));
        if (bonusAttackSpeed > 0) effects.Add(string.Format("<color=#fdcb6e>+{0}%{1}</color>", bonusAttackSpeed, isChinese ? "攻速" : "AtkSpd"));
        if (bonusMoveSpeed > 0) effects.Add(string.Format("<color=#ffeaa7>+{0}%{1}</color>", bonusMoveSpeed, isChinese ? "移速" : "MoveSpd"));
        if (bonusMemoryHaste > 0) effects.Add(string.Format("<color=#74b9ff>+{0}{1}</color>", bonusMemoryHaste, isChinese ? "急速" : "Haste"));
        if (bonusDodgeCharges > 0) effects.Add(string.Format("<color=#a29bfe>+{0}{1}</color>", bonusDodgeCharges, isChinese ? "闪避" : "Dodge"));
        if (bonusDodgeCooldownReduction > 0) effects.Add(string.Format("<color=#a29bfe>-{0}%{1}</color>", bonusDodgeCooldownReduction, isChinese ? "闪避CD" : "DodgeCD"));
        if (bonusRegeneration > 0) effects.Add(string.Format("<color=#00b894>+{0}{1}</color>", bonusRegeneration, isChinese ? "回/s" : "HP/s"));
        if (bonusLifesteal > 0) effects.Add(string.Format("<color=#e84393>+{0}%{1}</color>", bonusLifesteal, isChinese ? "吸血" : "Steal"));
        if (bonusThorns > 0) effects.Add(string.Format("<color=#d63031>+{0}%{1}</color>", bonusThorns, isChinese ? "荆棘" : "Thorns"));
        if (bonusGoldOnKill > 0) effects.Add(string.Format("<color=#ffd700>{0}</color>", isChinese ? "击杀掉金" : "Gold/Kill"));
        if (bonusDustOnKill > 0) effects.Add(string.Format("<color=#b2bec3>{0}</color>", isChinese ? "击杀获尘" : "Dust/Kill"));
        
        if (effects.Count > 0)
        {
            result += "\n<color=#888>---</color>\n";
            // Show 3 effects per line
            for (int i = 0; i < effects.Count; i++)
            {
                result += effects[i];
                if (i < effects.Count - 1)
                {
                    result += (i % 3 == 2) ? "\n" : " ";
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Get full detailed stats with complete labels (for Diablo-style UI with more space)
    /// </summary>
    public string GetFullDetailedStats()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null)
        {
            return Localization.NoHeroActive;
        }

        Hero hero = DewPlayer.local.hero;
        EntityStatus status = hero.Status;
        bool isChinese = Localization.CurrentLanguage == ModLanguage.Chinese;
        
        string result = "";
        
        // Base stats with full labels
        result += string.Format("<color=#FF6666>{0}:</color> {1:#,##0}", Localization.HP, status.maxHealth);
        if (bonusHealth > 0) result += string.Format(" <color=#95e66b>(+{0})</color>", bonusHealth);
        result += "\n";
        
        result += string.Format("<color=#FFAA00>{0}:</color> {1:#,##0}", Localization.ATK, status.attackDamage);
        if (bonusAttack > 0) result += string.Format(" <color=#ff6b6b>(+{0})</color>", bonusAttack);
        result += "\n";
        
        // Calculate damage reduction using game's formula: 100 / (100 + armor)
        float damageMultiplier = 100f / (100f + status.armor);
        float damageReduction = (1f - damageMultiplier) * 100f;
        string reductionLabel = isChinese ? "减伤" : "Damage Reduction";
        result += string.Format("<color=#6699FF>{0}:</color> {1:#,##0}", Localization.DEF, status.armor);
        if (bonusDefense > 0) result += string.Format(" <color=#4ecdc4>(+{0})</color>", bonusDefense);
        result += string.Format("\n<color=#888>  ({0}: {1:F1}%)</color>\n", reductionLabel, damageReduction);
        
        result += string.Format("<color=#CC66FF>{0}:</color> {1:#,##0}", Localization.AP, status.abilityPower);
        if (bonusAbilityPower > 0) result += string.Format(" <color=#ff6b6b>(+{0})</color>", bonusAbilityPower);
        
        // Full gear effects - one per line with complete labels
        result += "\n\n<color=#aaa>── " + (isChinese ? "装备效果" : "Gear Effects") + " ──</color>";
        
        if (bonusCritChance > 0)
            result += string.Format("\n<color=#fd79a8>{0}:</color> +{1}%", isChinese ? "暴击几率" : "Critical Chance", bonusCritChance);
        if (bonusCritDamage > 0)
            result += string.Format("\n<color=#e17055>{0}:</color> +{1}%", isChinese ? "暴击伤害" : "Critical Damage", bonusCritDamage);
        if (bonusAttackSpeed > 0)
            result += string.Format("\n<color=#fdcb6e>{0}:</color> +{1}%", isChinese ? "攻击速度" : "Attack Speed", bonusAttackSpeed);
        if (bonusMoveSpeed > 0)
            result += string.Format("\n<color=#ffeaa7>{0}:</color> +{1}%", isChinese ? "移动速度" : "Movement Speed", bonusMoveSpeed);
        if (bonusMemoryHaste > 0)
            result += string.Format("\n<color=#74b9ff>{0}:</color> +{1}", isChinese ? "技能急速" : "Ability Haste", bonusMemoryHaste);
        if (bonusDodgeCharges > 0)
            result += string.Format("\n<color=#a29bfe>{0}:</color> +{1}", isChinese ? "闪避次数" : "Dodge Charges", bonusDodgeCharges);
        if (bonusDodgeCooldownReduction > 0)
            result += string.Format("\n<color=#a29bfe>{0}:</color> -{1}%", isChinese ? "闪避冷却" : "Dodge Cooldown", bonusDodgeCooldownReduction);
        if (bonusRegeneration > 0)
            result += string.Format("\n<color=#00b894>{0}:</color> +{1}/s", isChinese ? "生命回复" : "Health Regen", bonusRegeneration);
        if (bonusShieldRegeneration > 0)
            result += string.Format("\n<color=#00cec9>{0}:</color> +{1}/s", isChinese ? "护盾回复" : "Shield Regen", bonusShieldRegeneration);
        if (bonusLifesteal > 0)
            result += string.Format("\n<color=#e84393>{0}:</color> +{1}%", isChinese ? "生命偷取" : "Life Steal", bonusLifesteal);
        if (bonusThorns > 0)
            result += string.Format("\n<color=#d63031>{0}:</color> +{1}%", isChinese ? "荆棘反伤" : "Thorns Damage", bonusThorns);
        if (bonusGoldOnKill > 0)
            result += string.Format("\n<color=#ffd700>{0}</color>", isChinese ? "击杀获得额外金币" : "Bonus Gold on Kill");
        if (bonusDustOnKill > 0)
            result += string.Format("\n<color=#b2bec3>{0}</color>", isChinese ? "击杀获得星尘" : "Stardust on Kill");
        
        return result;
    }
    
    // ============ Persistence Helper Methods ============
    
    /// <summary>
    /// Get equipped item for a slot (alias for GetEquipped)
    /// </summary>
    public RPGItem GetEquippedItem(EquipmentSlotType slotType)
    {
        return GetEquipped(slotType);
    }
    
    /// <summary>
    /// Get total gold on kill bonus from all equipped items
    /// </summary>
    public int GetGoldOnKillBonus()
    {
        return bonusGoldOnKill;
    }
    
    /// <summary>
    /// Get total dust on kill bonus from all equipped items
    /// </summary>
    public int GetDustOnKillBonus()
    {
        return bonusDustOnKill;
    }
    
    /// <summary>
    /// Unequip all items
    /// </summary>
    public void UnequipAll()
    {
        foreach (EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentSlotType)))
        {
            Unequip(slotType);
        }
        consumables.Clear();
        RPGLog.Debug(" Unequipped all items");
    }
    
    /// <summary>
    /// Equip item to slot without applying stats (for loading)
    /// </summary>
    public void EquipToSlot(RPGItem item, EquipmentSlotType slotType, bool applyStats)
    {
        if (!CanEquip(item, slotType)) return;
        
        equippedItems[slotType] = item;
        
        if (applyStats)
        {
            ApplyItemStats(item, slotType);
        }
    }
    
    /// <summary>
    /// Reapply all stats after loading
    /// </summary>
    public void ReapplyAllStats()
    {
        // Reset tracking
        bonusAttack = 0;
        bonusDefense = 0;
        bonusHealth = 0;
        bonusAbilityPower = 0;
        bonusMoveSpeed = 0f;
        bonusDodgeCharges = 0;
        bonusDodgeCooldownReduction = 0f;
        bonusGoldOnKill = 0;
        bonusDustOnKill = 0;
        bonusLifesteal = 0f;
        bonusThorns = 0f;
        bonusRegeneration = 0f;
        bonusShieldRegeneration = 0f;
        bonusAttackSpeed = 0f;
        bonusMemoryHaste = 0f;
        bonusCritChance = 0f;
        bonusCritDamage = 0f;
        
        // IMPORTANT: Stop all existing skill bonuses before clearing the dictionary
        // Otherwise dodge charges accumulate without being removed!
        foreach (var kvp in skillBonuses)
        {
            if (kvp.Value != null)
            {
                try { kvp.Value.Stop(); } catch { }
            }
        }
        skillBonuses.Clear();
        
        // Also remove stat bonuses properly
        if (DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            Hero hero = DewPlayer.local.hero;
            foreach (var kvp in statBonuses)
            {
                if (kvp.Value != null)
                {
                    try { hero.Status.RemoveStatBonus(kvp.Value); } catch { }
                }
            }
        }
        statBonuses.Clear();
        
        // Unsubscribe from events first
        UnsubscribeFromOnHitEffects();
        UnsubscribeFromThornsEffect();
        
        // Check if hero is ready
        if (DewPlayer.local == null || DewPlayer.local.hero == null)
        {
            // Hero not ready yet - mark for retry
            _needsStatReapply = true;
            RPGLog.Debug(" Hero not ready, will reapply stats when available");
            
            // Still accumulate local tracking values
            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    AccumulateItemStatsLocally(kvp.Value);
                }
            }
            return;
        }
        
        _needsStatReapply = false;
        
        // Apply stats for all equipped items
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null)
            {
                ApplyItemStats(kvp.Value, kvp.Key);
            }
        }
        
        // Recalculate set bonuses after all items are equipped
        if (_setBonusSystem != null)
        {
            _setBonusSystem.OnEquipmentChanged();
            RPGLog.Debug(" Set bonuses recalculated after equipment load");
        }
        
        RPGLog.Debug(" Reapplied all equipment stats");
    }
    
    /// <summary>
    /// Accumulate item stats locally without applying to hero (used when hero isn't ready)
    /// </summary>
    private void AccumulateItemStatsLocally(RPGItem item)
    {
        if (item == null) return;
        
        bonusAttack += item.attackBonus;
        bonusDefense += item.defenseBonus;
        bonusHealth += item.healthBonus;
        bonusAbilityPower += item.abilityPowerBonus;
        bonusMoveSpeed += item.moveSpeedBonus;
        bonusDodgeCharges += item.dodgeChargesBonus;
        bonusDodgeCooldownReduction += item.dodgeCooldownReduction;
        bonusGoldOnKill += item.goldOnKill;
        bonusDustOnKill += item.dustOnKill;
        bonusLifesteal += item.lifeSteal;
        bonusThorns += item.thorns;
        bonusRegeneration += item.regeneration;
        bonusShieldRegeneration += item.shieldRegeneration;
        bonusAttackSpeed += item.attackSpeed;
        bonusMemoryHaste += item.memoryHaste;
        bonusCritChance += item.criticalChance;
        bonusCritDamage += item.criticalDamage;
    }
    
    /// <summary>
    /// Check if stats need to be reapplied (called from Update)
    /// </summary>
    public void CheckPendingStatReapply()
    {
        if (!_needsStatReapply) return;
        
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        RPGLog.Debug(" Hero now ready, reapplying equipment stats...");
        _needsStatReapply = false;
        
        Hero hero = DewPlayer.local.hero;
        
        // IMPORTANT: Stop all existing skill bonuses before clearing
        foreach (var kvp in skillBonuses)
        {
            if (kvp.Value != null)
            {
                try { kvp.Value.Stop(); } catch { }
            }
        }
        skillBonuses.Clear();
        
        // Remove stat bonuses properly
        foreach (var kvp in statBonuses)
        {
            if (kvp.Value != null)
            {
                try { hero.Status.RemoveStatBonus(kvp.Value); } catch { }
            }
        }
        statBonuses.Clear();
        
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null)
            {
                ApplyItemStatsToHero(kvp.Value, kvp.Key);
            }
        }
        
        // Subscribe to on-hit effects if needed
        if (bonusLifesteal > 0)
        {
            SubscribeToOnHitEffects();
        }
        
        // Subscribe to thorns if needed
        if (bonusThorns > 0)
        {
            SubscribeToThornsEffect();
        }
        
        // Sync on-kill bonuses
        SyncOnKillBonuses();
        
        RPGLog.Debug(" Equipment stats applied after hero became ready");
    }
    
    /// <summary>
    /// Apply item stats directly to hero (assumes hero is ready)
    /// </summary>
    private void ApplyItemStatsToHero(RPGItem item, EquipmentSlotType slotType)
    {
        if (item == null) return;
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        
        // Create stat bonus
        StatBonus bonus = new StatBonus();
        bonus.attackDamageFlat = item.attackBonus;
        bonus.armorFlat = item.defenseBonus;
        bonus.maxHealthFlat = item.healthBonus;
        bonus.abilityPowerFlat = item.abilityPowerBonus;
        bonus.movementSpeedPercentage = item.moveSpeedBonus;
        bonus.healthRegenFlat = item.regeneration;
        bonus.attackSpeedPercentage = item.attackSpeed;
        bonus.abilityHasteFlat = item.memoryHaste;
        bonus.critChanceFlat = item.criticalChance / 100f;
        bonus.critAmpFlat = item.criticalDamage / 100f;
        
        if (NetworkServer.active)
        {
            // Note: This is called after clearing dictionaries, so no need to remove old bonuses here
            StatBonus addedBonus = hero.Status.AddStatBonus(bonus);
            statBonuses[slotType] = addedBonus;
            
            // Apply dodge charges and cooldown reduction
            if ((item.dodgeChargesBonus != 0 || item.dodgeCooldownReduction > 0) && hero.Skill != null && hero.Skill.Movement != null)
            {
                SkillBonus skillBonus = hero.Skill.Movement.AddSkillBonus(new SkillBonus
                {
                    addedCharge = item.dodgeChargesBonus,
                    cooldownMultiplier = item.dodgeCooldownReduction > 0 ? (1f - item.dodgeCooldownReduction / 100f) : 1f
                });
                skillBonuses[slotType] = skillBonus;
            }
        }
        else
        {
            // Client: request server to apply stats
            RequestStatChange(hero.netId, (int)slotType, item, true);
        }
    }
    
    // ============ Consumable Slots ============
    
    private Dictionary<int, RPGItem> consumables = new Dictionary<int, RPGItem>();
    
    /// <summary>
    /// Get consumable in slot (0-3)
    /// </summary>
    public RPGItem GetConsumable(int slot)
    {
        RPGItem item;
        consumables.TryGetValue(slot, out item);
        return item;
    }
    
    /// <summary>
    /// Set consumable in slot (0-3)
    /// </summary>
    public void SetConsumable(int slot, RPGItem item)
    {
        if (slot < 0 || slot > 3) return;
        
        if (item == null)
        {
            consumables.Remove(slot);
        }
        else
        {
            consumables[slot] = item;
        }
    }
    
    // ============ On-Hit Effects System ============
    // Note: ActorEvent_OnDealDamage only fires on the server, so we need server-side handling
    
    private void SubscribeToOnHitEffects()
    {
        if (isSubscribedToDamageEvent) return;
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        
        if (NetworkServer.active)
        {
            // Server/Host: Subscribe directly to damage event
            onDealDamageHandler = (EventInfoDamage info) =>
            {
                OnDealDamageForOnHitEffects(info);
            };
            
            hero.ActorEvent_OnDealDamage += onDealDamageHandler;
            isSubscribedToDamageEvent = true;
            RPGLog.Debug(" Host subscribed to damage event for on-hit effects");
        }
        else
        {
            // Client: Tell server to track our on-hit effects (lifesteal only, dust is on-kill now)
            RequestOnHitSubscription(hero.netId, 0, 0, bonusLifesteal, true);
            isSubscribedToDamageEvent = true;
            RPGLog.Debug(" Client requested on-hit subscription");
        }
    }
    
    private void UnsubscribeFromOnHitEffects()
    {
        if (!isSubscribedToDamageEvent) return;
        
        if (NetworkServer.active)
        {
            // Store hero reference locally to avoid race condition
            Hero hero = null;
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                hero = DewPlayer.local.hero;
            }
            
            if (hero != null && onDealDamageHandler != null)
            {
                try
                {
                    hero.ActorEvent_OnDealDamage -= onDealDamageHandler;
                }
                catch (Exception e)
                {
                    RPGLog.Warning(" Error unsubscribing from damage event: " + e.Message);
                }
            }
        }
        else
        {
            // Client: Tell server to stop tracking
            Hero hero = null;
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                hero = DewPlayer.local.hero;
            }
            
            if (hero != null)
            {
                RequestOnHitSubscription(hero.netId, 0, 0, 0f, false);
            }
        }
        
        onDealDamageHandler = null;
        isSubscribedToDamageEvent = false;
    }
    
    private void SubscribeToThornsEffect()
    {
        if (isSubscribedToTakeDamageEvent) return;
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        
        if (NetworkServer.active)
        {
            // Server/Host: Subscribe directly to take damage event (on Entity, not Actor)
            onTakeDamageHandler = (EventInfoDamage info) =>
            {
                OnTakeDamageForThorns(info);
            };
            
            hero.EntityEvent_OnTakeDamage += onTakeDamageHandler;
            isSubscribedToTakeDamageEvent = true;
            RPGLog.Debug(" Host subscribed to take damage event for thorns");
        }
        else
        {
            // Client: Tell server to track our thorns
            RequestThornsSubscription(hero.netId, bonusThorns, true);
            isSubscribedToTakeDamageEvent = true;
        }
    }
    
    private void UnsubscribeFromThornsEffect()
    {
        if (!isSubscribedToTakeDamageEvent) return;
        
        if (NetworkServer.active)
        {
            // Store hero reference locally to avoid race condition
            Hero hero = null;
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                hero = DewPlayer.local.hero;
            }
            
            if (hero != null && onTakeDamageHandler != null)
            {
                try
                {
                    hero.EntityEvent_OnTakeDamage -= onTakeDamageHandler;
                }
                catch (Exception e)
                {
                    RPGLog.Warning(" Error unsubscribing from take damage event: " + e.Message);
                }
            }
        }
        else
        {
            // Store hero reference locally to avoid race condition
            Hero hero = null;
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                hero = DewPlayer.local.hero;
            }
            
            if (hero != null)
            {
                RequestThornsSubscription(hero.netId, 0f, false);
            }
        }
        
        onTakeDamageHandler = null;
        isSubscribedToTakeDamageEvent = false;
    }
    
    private void OnDealDamageForOnHitEffects(EventInfoDamage info)
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        if (DewPlayer.local.hero.IsNullOrInactive()) return;
        
        // Only trigger if we actually dealt damage to an enemy
        if (info.damage.amount <= 0) return;
        if (info.victim == null) return;
        
        // Don't trigger for self-damage
        if (info.victim == DewPlayer.local.hero) return;
        
        // Lifesteal (heal based on damage dealt)
        if (bonusLifesteal > 0 && DewPlayer.local.hero != null)
        {
            float healAmount = info.damage.amount * (bonusLifesteal / 100f);
            if (healAmount >= 1f)
            {
                HealData healData = new HealData(healAmount);
                healData.SetActor(DewPlayer.local.hero);
                DewPlayer.local.hero.DoHeal(healData, DewPlayer.local.hero);
            }
        }
    }
    
    private void OnTakeDamageForThorns(EventInfoDamage info)
    {
        try
        {
            if (bonusThorns <= 0) return;
            if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
            if (DewPlayer.local.hero.IsNullOrInactive()) return;
            
            // Only trigger if we actually took damage
            if (info.damage.amount <= 0) return;
            
            // Get the attacker from EventInfoDamage
            Actor attacker = info.actor;
            if (attacker == null) return;
            
            // Get the first entity in the actor chain (the actual source)
            Entity attackerEntity = attacker.firstEntity;
            if (attackerEntity == null) return;
            
            // CRITICAL: Don't reflect damage from self (e.g., Immolation memory)
            if (attackerEntity == DewPlayer.local.hero) return;
            
            // Don't reflect damage from any hero (self or allies)
            if (attackerEntity is Hero) return;
            
            // Only reflect damage from monsters (enemies)
            if (!(attackerEntity is Monster)) return;
            
            // Don't reflect if attacker is dead/inactive
            if (attackerEntity.IsNullInactiveDeadOrKnockedOut()) return;
            
            // Calculate thorns damage (% of damage taken)
            float thornsDamage = info.damage.amount * (bonusThorns / 100f);
            if (thornsDamage >= 1f)
            {
                // Deal damage back to attacker using Actor.DealDamage
                DamageData thornsData = new DamageData(DamageData.SourceType.Physical, thornsDamage, 0f);
                thornsData.SetActor(DewPlayer.local.hero);
                DewPlayer.local.hero.DealDamage(thornsData, attackerEntity);
            }
        }
        catch (Exception e)
        {
            // Silently catch any errors to prevent crashes
            RPGLog.Warning(" Thorns damage error: " + e.Message);
        }
    }
    
    private void RequestOnHitSubscription(uint heroNetId, int goldOnHit, int dustOnHit, float lifesteal, bool subscribe)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Format: [RPGONHITSUB]heroNetId|gold|dust|lifesteal|S/U (subscribe/unsubscribe)
            string op = subscribe ? "S" : "U";
            string content = string.Format("[RPGONHITSUB]{0}|{1}|{2}|{3}|{4}", heroNetId, goldOnHit, dustOnHit, (int)lifesteal, op);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch { }
    }
    
    /// <summary>
    /// Sync on-kill bonuses to server for multiplayer support
    /// Now includes upgrade levels for scaling
    /// </summary>
    private void SyncOnKillBonuses()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        uint heroNetId = DewPlayer.local.hero.netId;
        
        // Find the upgrade levels of items providing gold/dust on kill
        int goldUpgradeLevel = GetOnKillUpgradeLevel(true);
        int dustUpgradeLevel = GetOnKillUpgradeLevel(false);
        
        if (NetworkServer.active)
        {
            // Host: Register directly
            MonsterLootSystem.RegisterOnKillBonuses(heroNetId, bonusGoldOnKill, goldUpgradeLevel, bonusDustOnKill, dustUpgradeLevel);
        }
        else
        {
            // Client: Send to server via chat
            try
            {
                ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
                if (chatManager == null) return;
                
                // Format: [RPGONKILLSYNC]heroNetId|goldOnKill|goldUpgrade|dustOnKill|dustUpgrade
                string content = string.Format("[RPGONKILLSYNC]{0}|{1}|{2}|{3}|{4}", 
                    heroNetId, bonusGoldOnKill, goldUpgradeLevel, bonusDustOnKill, dustUpgradeLevel);
                chatManager.CmdSendChatMessage(content, null);
            }
            catch { }
        }
    }
    
    /// <summary>
    /// Get the highest upgrade level of items providing gold or dust on kill
    /// </summary>
    private int GetOnKillUpgradeLevel(bool forGold)
    {
        int maxUpgrade = 0;
        
        foreach (var kvp in equippedItems)
        {
            RPGItem item = kvp.Value;
            if (item == null) continue;
            
            if (forGold && item.goldOnKill > 0)
            {
                if (item.upgradeLevel > maxUpgrade)
                    maxUpgrade = item.upgradeLevel;
            }
            else if (!forGold && item.dustOnKill > 0)
            {
                if (item.upgradeLevel > maxUpgrade)
                    maxUpgrade = item.upgradeLevel;
            }
        }
        
        return maxUpgrade;
    }
    
    private void RequestThornsSubscription(uint heroNetId, float thorns, bool subscribe)
    {
        try
        {
            ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
            if (chatManager == null) return;
            
            // Format: [RPGTHORNSSUB]heroNetId|thorns|S/U (subscribe/unsubscribe)
            string op = subscribe ? "S" : "U";
            string content = string.Format("[RPGTHORNSSUB]{0}|{1}|{2}", heroNetId, (int)thorns, op);
            chatManager.CmdSendChatMessage(content, null);
        }
        catch { }
    }
    
    // ============ Elemental Attack System ============
    
    /// <summary>
    /// Update active elemental type from equipped weapons
    /// Called when equipment changes
    /// </summary>
    public void UpdateElementalEffects()
    {
        activeElementalType = null;
        activeElementalStacks = 0;
        
        // Check both hand slots for elemental weapons
        RPGItem leftHand = GetEquipped(EquipmentSlotType.LeftHand);
        RPGItem rightHand = GetEquipped(EquipmentSlotType.RightHand);
        
        // Use the first elemental weapon found (priority: right hand > left hand)
        if (rightHand != null && rightHand.elementalType.HasValue)
        {
            activeElementalType = rightHand.elementalType;
            activeElementalStacks = rightHand.elementalStacks;
        }
        else if (leftHand != null && leftHand.elementalType.HasValue)
        {
            activeElementalType = leftHand.elementalType;
            activeElementalStacks = leftHand.elementalStacks;
        }
        
        if (activeElementalType.HasValue)
        {
            RPGLog.Debug(" Active elemental: " + activeElementalType.Value + " +" + activeElementalStacks + " stacks");
            // Subscribe to hero attacks if we have elemental weapons
            ElementalAttackPatches.SubscribeToHeroAttacks();
        }
    }
    
    /// <summary>
    /// Get the active elemental type for attacks
    /// </summary>
    public ElementalType? GetActiveElementalType()
    {
        return activeElementalType;
    }
    
    /// <summary>
    /// Get the number of elemental stacks to apply
    /// </summary>
    public int GetActiveElementalStacks()
    {
        return activeElementalStacks;
    }
    
    // ============ Shield Regeneration System ============
    
    /// <summary>
    /// Update shield regeneration (called from Update, applies shield per second)
    /// Shield is capped at 50% of max HP
    /// </summary>
    public void UpdateShieldRegeneration(float deltaTime)
    {
        if (bonusShieldRegeneration <= 0f) return;
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        if (!NetworkServer.active) return; // Only apply on server/host
        
        Hero hero = DewPlayer.local.hero;
        EntityStatus status = hero.Status;
        
        // Accumulate time
        shieldRegenTimer += deltaTime;
        
        // Apply shield regeneration every second
        if (shieldRegenTimer >= 1f)
        {
            shieldRegenTimer = 0f;
            
            // Calculate max shield (50% of max HP)
            float maxShield = status.maxHealth * 0.5f;
            
            // Get current shield value using reflection (shield is stored in EntityStatus)
            float currentShield = 0f;
            try
            {
                // Try to get shield property via reflection
                var shieldProperty = status.GetType().GetProperty("shield");
                if (shieldProperty != null)
                {
                    currentShield = (float)shieldProperty.GetValue(status);
                }
                else
                {
                    // Try field instead
                    var shieldField = status.GetType().GetField("shield");
                    if (shieldField != null)
                    {
                        currentShield = (float)shieldField.GetValue(status);
                    }
                }
            }
            catch
            {
                // If we can't get current shield, assume 0 (will apply regen anyway)
                currentShield = 0f;
            }
            
            // Only apply if under cap
            if (currentShield < maxShield)
            {
                float shieldToAdd = bonusShieldRegeneration;
                
                // Cap the shield addition to not exceed max
                if (currentShield + shieldToAdd > maxShield)
                {
                    shieldToAdd = maxShield - currentShield;
                }
                
                if (shieldToAdd > 0f)
                {
                    // Apply shield with 10 second duration, non-decaying (same as potions)
                    hero.GiveShield(hero, shieldToAdd, 10f, false);
                }
            }
        }
    }
    
    /// <summary>
    /// Apply elemental effect to a target (called on basic attack hit)
    /// This is server-side only
    /// </summary>
    public void ApplyElementalToTarget(Entity target, Hero attacker)
    {
        if (!NetworkServer.active) return;
        if (!activeElementalType.HasValue) return;
        if (target == null || attacker == null) return;
        
        ElementalType elemType = activeElementalType.Value;
        int stacksToApply = Mathf.Max(1, activeElementalStacks);
        
        // Apply limited stacks based on element type
        switch (elemType)
        {
            case ElementalType.Fire:
                ApplyFireWithLimit(target, attacker, stacksToApply);
                break;
            case ElementalType.Cold:
                ApplyColdWithReduction(target, attacker, stacksToApply);
                break;
            case ElementalType.Light:
                ApplyLightWithLimit(target, attacker, stacksToApply);
                break;
            case ElementalType.Dark:
                ApplyDarkWithLimit(target, attacker, stacksToApply);
                break;
        }
    }
    
    private void ApplyFireWithLimit(Entity target, Hero attacker, int stacks)
    {
        // Check current fire stacks on target
        int currentStacks = target.Status.fireStack;
        
        if (currentStacks >= MAX_FIRE_STACKS)
        {
            // Already at max, just refresh duration
            Se_Elm_Fire fire;
            if (target.Status.TryGetStatusEffect<Se_Elm_Fire>(out fire))
            {
                fire.ResetDecayTimer();
            }
            return;
        }
        
        // Calculate how many stacks we can add
        int canAdd = Mathf.Min(stacks, MAX_FIRE_STACKS - currentStacks);
        if (canAdd > 0)
        {
            // Apply elemental via the game's system
            attacker.ApplyElemental(ElementalType.Fire, target, canAdd);
        }
    }
    
    private void ApplyColdWithReduction(Entity target, Hero attacker, int stacks)
    {
        // Cold has reduced effectiveness from weapons
        // Apply normally but the effect itself will be reduced
        // We apply the stacks, but the slow/cripple values are reduced via a patch
        attacker.ApplyElemental(ElementalType.Cold, target, stacks);
    }
    
    private void ApplyLightWithLimit(Entity target, Hero attacker, int stacks)
    {
        int currentStacks = target.Status.lightStack;
        
        if (currentStacks >= MAX_LIGHT_STACKS)
        {
            // Already at max, just refresh
            Se_Elm_Light light;
            if (target.Status.TryGetStatusEffect<Se_Elm_Light>(out light))
            {
                light.ResetDecayTimer();
            }
            return;
        }
        
        int canAdd = Mathf.Min(stacks, MAX_LIGHT_STACKS - currentStacks);
        if (canAdd > 0)
        {
            attacker.ApplyElemental(ElementalType.Light, target, canAdd);
        }
    }
    
    private void ApplyDarkWithLimit(Entity target, Hero attacker, int stacks)
    {
        int currentStacks = target.Status.darkStack;
        
        if (currentStacks >= MAX_DARK_STACKS)
        {
            // Already at max, just refresh
            Se_Elm_Dark dark;
            if (target.Status.TryGetStatusEffect<Se_Elm_Dark>(out dark))
            {
                dark.ResetDecayTimer();
            }
            return;
        }
        
        int canAdd = Mathf.Min(stacks, MAX_DARK_STACKS - currentStacks);
        if (canAdd > 0)
        {
            attacker.ApplyElemental(ElementalType.Dark, target, canAdd);
        }
    }
    
    /// <summary>
    /// Clean up when leaving game
    /// </summary>
    public void Cleanup()
    {
        UnsubscribeFromOnHitEffects();
        UnsubscribeFromThornsEffect();
        activeElementalType = null;
        activeElementalStacks = 0;
        
        // Clean up set bonus system
        if (_setBonusSystem != null)
        {
            _setBonusSystem.Cleanup();
        }
    }
}

/// <summary>
/// Simple animator for heal flash effect - expands ring and fades out
/// </summary>
public class HealFlashAnimator : MonoBehaviour
{
    public LineRenderer line;
    private float elapsed = 0f;
    private float duration = 0.8f;
    
    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        
        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        
        // Expand ring
        float radius = 0.5f + t * 1.5f;
        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
        }
        
        // Fade out
        float alpha = 1f - t;
        Color c = new Color(0.2f, 1f, 0.3f, alpha);
        line.startColor = c;
        line.endColor = c;
        
        // Rise up
        transform.localPosition = Vector3.up * (1f + t * 0.5f);
    }
}
