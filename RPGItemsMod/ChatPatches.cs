using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Mirror;

/// <summary>
/// Harmony patch to intercept mod chat messages on server and prevent broadcast
/// </summary>
public static class ChatManagerPatch
{
    // Track stat bonuses per hero per slot
    private static Dictionary<uint, Dictionary<int, StatBonus>> _heroStatBonuses = new Dictionary<uint, Dictionary<int, StatBonus>>();
    // Track skill bonuses (dodge charges) per hero per slot
    private static Dictionary<uint, Dictionary<int, SkillBonus>> _heroSkillBonuses = new Dictionary<uint, Dictionary<int, SkillBonus>>();
    
    // Track Wind Rush buffs per hero (for proper cleanup)
    // NOTE: Using Time.realtimeSinceStartup instead of Time.time because Time.time can reset between scenes!
    private static Dictionary<uint, StatBonus> _heroWindRushBonuses = new Dictionary<uint, StatBonus>();
    private static Dictionary<uint, float> _heroWindRushEndTimes = new Dictionary<uint, float>(); // realtime values
    
    // Reusable lists to avoid GC allocations in Update loop
    private static List<uint> _expiredHeroesCache = new List<uint>();
    private static List<uint> _keysSnapshotCache = new List<uint>();
    
    /// <summary>
    /// Called every frame to update timed effects (like Wind Rush buff expiration)
    /// </summary>
    public static void Update()
    {
        if (!NetworkServer.active) return;
        
        // Check for expired Wind Rush buffs - use realtime (doesn't reset between scenes!)
        float realtime = Time.realtimeSinceStartup;
        
        // CRITICAL FIX: Collect keys first to avoid modifying dictionary during iteration
        // Use cached lists to avoid GC allocations every frame
        _expiredHeroesCache.Clear();
        _keysSnapshotCache.Clear();
        _keysSnapshotCache.AddRange(_heroWindRushEndTimes.Keys);
        
        foreach (uint heroNetId in _keysSnapshotCache)
        {
            // Double-check the key still exists (race condition protection)
            if (!_heroWindRushEndTimes.ContainsKey(heroNetId)) continue;
            
            float endTime = _heroWindRushEndTimes[heroNetId];
            if (realtime >= endTime)
            {
                _expiredHeroesCache.Add(heroNetId);
            }
        }
        
        // Remove expired buffs (safe to modify now)
        foreach (uint heroNetId in _expiredHeroesCache)
        {
            // Use TryGetValue for thread safety
            StatBonus buff;
            if (_heroWindRushBonuses.TryGetValue(heroNetId, out buff))
            {
                if (buff != null)
                {
                    // Find the hero and remove the buff
                    foreach (DewPlayer player in DewPlayer.allHumanPlayers)
                    {
                        if (player != null && player.hero != null && player.hero.netId == heroNetId)
                        {
                            try 
                            { 
                                player.hero.Status.RemoveStatBonus(buff);
                                RPGLog.Debug(" ChatPatches: Wind Rush buff expired for hero " + heroNetId);
                            } 
                            catch (Exception e)
                            {
                                RPGLog.Warning(" Error removing Wind Rush buff: " + e.Message);
                            }
                            break;
                        }
                    }
                }
                _heroWindRushBonuses.Remove(heroNetId);
            }
            _heroWindRushEndTimes.Remove(heroNetId);
        }
    }
    
    public static bool Prefix_CmdSendChatMessage(ChatManager __instance, string content, Mirror.NetworkConnectionToClient sender)
    {
        if (string.IsNullOrEmpty(content)) return true;
        
        // Handle item sync requests
        if (content.Contains("[RPGREQ]"))
        {
            NetworkedItemSystem.ProcessChatMessage(content);
            return false;
        }
        
        // Handle stat change requests from clients (v3, v2, and v1 formats)
        if (content.Contains("[RPGSTAT3]"))
        {
            ProcessStatRequest3(content);
            return false;
        }
        
        if (content.Contains("[RPGSTAT2]"))
        {
            ProcessStatRequest2(content);
            return false;
        }
        
        if (content.Contains("[RPGSTAT]"))
        {
            ProcessStatRequest(content);
            return false;
        }
        
        // Handle heal requests from clients (with VFX version first since it contains [RPGHEAL] substring)
        if (content.Contains("[RPGHEALVFX]"))
        {
            ProcessHealRequestWithVFX(content);
            return false;
        }
        
        // Handle heal requests from clients (simple version)
        if (content.Contains("[RPGHEAL]"))
        {
            ProcessHealRequest(content);
            return false;
        }
        
        // Handle gold on hit subscription requests from clients
        if (content.Contains("[RPGGOLDSUB]"))
        {
            ProcessGoldOnHitSubscription(content);
            return false;
        }
        
        // Handle sell item requests from clients (give gold)
        if (content.Contains("[RPGSELL]"))
        {
            ProcessSellRequest(content);
            return false;
        }
        
        // Handle spend dust requests from clients (for upgrades)
        if (content.Contains("[RPGSPEND]"))
        {
            ProcessSpendDustRequest(content);
            return false;
        }
        
        // Handle give dust requests from clients (for dismantle/cleanse refund)
        if (content.Contains("[RPGDUST]"))
        {
            ProcessGiveDustRequest(content);
            return false;
        }
        
        // Handle cleanse gold spend requests from clients
        if (content.Contains("[RPGCLEANSE]"))
        {
            ProcessCleanseGoldRequest(content);
            return false;
        }
        
        // Legacy: Handle old upgrade format (backwards compatibility)
        if (content.Contains("[RPGUPGRADE]"))
        {
            ProcessSpendDustRequest(content.Replace("[RPGUPGRADE]", "[RPGSPEND]"));
            return false;
        }
        
        // Handle StatsSystem stat apply requests from clients
        if (content.Contains("[RPGSTATS]"))
        {
            RPGLog.Info(" SERVER: Intercepted [RPGSTATS] message, processing...");
            ProcessStatsSystemRequest(content);
            return false;
        }
        
        // Handle WeaponMastery bonus requests from clients
        if (content.Contains("[RPGMASTERY]"))
        {
            ProcessMasteryBonusRequest(content);
            return false;
        }
        
        // Handle thorns damage subscription requests from clients
        if (content.Contains("[RPGTHORNSSUB]"))
        {
            ProcessThornsSubscription(content);
            return false;
        }
        
        // Handle on-hit effects subscription requests from clients (lifesteal only now)
        if (content.Contains("[RPGONHITSUB]"))
        {
            ProcessOnHitSubscription(content);
            return false;
        }
        
        // Handle on-kill bonuses sync from clients (gold on kill, dust on kill)
        if (content.Contains("[RPGONKILLSYNC]"))
        {
            ProcessOnKillSync(content);
            return false;
        }
        
        // Handle set bonus stat requests from clients
        if (content.Contains("[RPGSETBONUS]"))
        {
            ProcessSetBonusRequest(content);
            return false;
        }
        
        // Handle set bonus proc requests from clients
        if (content.Contains("[RPGSETPROC]"))
        {
            ProcessSetProcRequest(content);
            return false;
        }
        
        // Handle Cheat Death registration/unregistration from clients
        if (content.Contains("[RPGCHEATDEATH]"))
        {
            ProcessCheatDeathRegistration(content);
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Process on-kill bonuses sync from client
    /// Format: [RPGONKILLSYNC]heroNetId|goldOnKill|goldUpgrade|dustOnKill|dustUpgrade
    /// </summary>
    private static void ProcessOnKillSync(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            int idx = content.IndexOf("[RPGONKILLSYNC]");
            if (idx < 0) return;
            string data = content.Substring(idx + 15);
            string[] parts = data.Split('|');
            
            uint heroNetId = uint.Parse(parts[0]);
            int goldOnKill = int.Parse(parts[1]);
            int goldUpgrade = 0;
            int dustOnKill = 0;
            int dustUpgrade = 0;
            
            // Support both old format (3 parts) and new format (5 parts)
            if (parts.Length >= 5)
            {
                goldUpgrade = int.Parse(parts[2]);
                dustOnKill = int.Parse(parts[3]);
                dustUpgrade = int.Parse(parts[4]);
            }
            else if (parts.Length >= 3)
            {
                // Old format: heroNetId|goldOnKill|dustOnKill
                dustOnKill = int.Parse(parts[2]);
            }
            
            // Register with MonsterLootSystem
            MonsterLootSystem.RegisterOnKillBonuses(heroNetId, goldOnKill, goldUpgrade, dustOnKill, dustUpgrade);
        }
        catch { }
    }
    
    // Track set bonus stat bonuses per hero
    private static Dictionary<uint, StatBonus> _heroSetBonuses = new Dictionary<uint, StatBonus>();
    
    // Track Cheat Death status per hero (server-side only)
    // Key: heroNetId, Value: cooldown end time (realtime)
    private static Dictionary<uint, float> _heroCheatDeathCooldowns = new Dictionary<uint, float>();
    
    // Track which heroes have Cheat Death active (server-side only)
    private static HashSet<uint> _heroesWithCheatDeath = new HashSet<uint>();
    
    // Handler for server-side Cheat Death damage interception
    private static Dictionary<Hero, Action<EventInfoDamage>> _cheatDeathHandlers = new Dictionary<Hero, Action<EventInfoDamage>>();
    
    /// <summary>
    /// Process set bonus stat request from client
    /// Format: [RPGSETBONUS]heroNetId|atk|def|hp|moveSpd|atkSpd|critCh|critDmg|regen
    /// </summary>
    private static void ProcessSetBonusRequest(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            int idx = content.IndexOf("[RPGSETBONUS]");
            if (idx < 0) return;
            string data = content.Substring(idx + 13);
            string[] parts = data.Split('|');
            if (parts.Length < 9) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int atk = int.Parse(parts[1]);
            int def = int.Parse(parts[2]);
            int hp = int.Parse(parts[3]);
            float moveSpd = float.Parse(parts[4]);
            float atkSpd = float.Parse(parts[5]);
            float critCh = float.Parse(parts[6]);
            float critDmg = float.Parse(parts[7]);
            float regen = float.Parse(parts[8]);
            
            // Validate netId
            if (heroNetId == 0)
            {
                RPGLog.Debug(" SetBonus: Invalid netId (0) received in set bonus request");
                return;
            }
            
            // Find the hero - try multiple methods for reliability
            Hero hero = null;
            
            // Method 1: Try DewPlayer.allHumanPlayers (most common case - ensures we get the player's hero)
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            // Method 2: Try ActorManager.allHeroes (fallback - more reliable, includes all heroes)
            if (hero == null && NetworkedManagerBase<ActorManager>.instance != null)
            {
                foreach (Hero h in NetworkedManagerBase<ActorManager>.instance.allHeroes)
                {
                    if (h != null && !h.IsNullOrInactive() && h.netId == heroNetId)
                    {
                        hero = h;
                        break;
                    }
                }
            }
            
            // Method 3: Try NetworkClient.spawned (fallback for network objects)
            if (hero == null && NetworkClient.spawned != null)
            {
                NetworkIdentity identity;
                if (NetworkClient.spawned.TryGetValue(heroNetId, out identity))
                {
                    hero = identity.GetComponent<Hero>();
                }
            }
            
            if (hero == null || hero.IsNullOrInactive())
            {
                RPGLog.Debug(string.Format(" SetBonus: Hero {0} not found or inactive (cannot apply set bonus)", heroNetId));
                return;
            }
            
            // Remove old set bonus if exists (use TryGetValue for thread safety)
            StatBonus oldBonus;
            if (_heroSetBonuses.TryGetValue(heroNetId, out oldBonus))
            {
                if (oldBonus != null)
                {
                    try { hero.Status.RemoveStatBonus(oldBonus); } catch { }
                }
                _heroSetBonuses.Remove(heroNetId);
            }
            
            // Apply new set bonus
            StatBonus bonus = new StatBonus();
            bonus.attackDamageFlat = atk;
            bonus.armorFlat = def;
            bonus.maxHealthFlat = hp;
            bonus.movementSpeedPercentage = moveSpd;
            bonus.attackSpeedPercentage = atkSpd;
            bonus.critChanceFlat = critCh;
            bonus.critAmpFlat = critDmg;
            bonus.healthRegenFlat = regen;
            
            StatBonus addedBonus = hero.Status.AddStatBonus(bonus);
            _heroSetBonuses[heroNetId] = addedBonus;
            
            RPGLog.Debug(string.Format(" Applied set bonus to hero {0}: ATK={1}, DEF={2}, HP={3}", 
                heroNetId, atk, def, hp));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing set bonus request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Process set bonus proc request from client
    /// Format: [RPGSETPROC]heroNetId|rarity|extraData|posX|posY|posZ
    /// </summary>
    private static void ProcessSetProcRequest(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            int idx = content.IndexOf("[RPGSETPROC]");
            if (idx < 0) return;
            string data = content.Substring(idx + 12);
            string[] parts = data.Split('|');
            if (parts.Length < 6) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int rarityInt = int.Parse(parts[1]);
            int extraData = int.Parse(parts[2]);
            float posX = float.Parse(parts[3]);
            float posY = float.Parse(parts[4]);
            float posZ = float.Parse(parts[5]);
            
            ItemRarity rarity = (ItemRarity)rarityInt;
            Vector3 position = new Vector3(posX, posY, posZ);
            
            // Validate netId
            if (heroNetId == 0)
            {
                RPGLog.Debug(" SetProc: Invalid netId (0) received in set proc request");
                return;
            }
            
            // Find the hero - try multiple methods for reliability
            Hero hero = null;
            
            // Method 1: Try DewPlayer.allHumanPlayers (most common case - ensures we get the player's hero)
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            // Method 2: Try ActorManager.allHeroes (fallback - more reliable, includes all heroes)
            if (hero == null && NetworkedManagerBase<ActorManager>.instance != null)
            {
                foreach (Hero h in NetworkedManagerBase<ActorManager>.instance.allHeroes)
                {
                    if (h != null && !h.IsNullOrInactive() && h.netId == heroNetId)
                    {
                        hero = h;
                        break;
                    }
                }
            }
            
            // Method 3: Try NetworkClient.spawned (fallback for network objects)
            if (hero == null && NetworkClient.spawned != null)
            {
                NetworkIdentity identity;
                if (NetworkClient.spawned.TryGetValue(heroNetId, out identity))
                {
                    hero = identity.GetComponent<Hero>();
                }
            }
            
            if (hero == null || hero.IsNullOrInactive())
            {
                RPGLog.Debug(string.Format(" SetProc: Hero {0} not found or inactive (cannot execute proc)", heroNetId));
                return;
            }
            
            // Execute the proc effect on server
            ExecuteProcOnServer(hero, rarity, extraData, position);
            
            RPGLog.Debug(string.Format(" SetProc executed for hero {0}: rarity={1}", heroNetId, rarity));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing set proc request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Execute a proc effect on the server for a specific hero
    /// </summary>
    private static void ExecuteProcOnServer(Hero hero, ItemRarity rarity, int extraData, Vector3 position)
    {
        if (!NetworkServer.active || hero == null) return;
        
        switch (rarity)
        {
            case ItemRarity.Common:
                // Cheat Death: Heal to 1 HP + Give shield equal to 100% max HP
                // CRITICAL: Heal to 1 HP FIRST to prevent death
                if (hero.Status.currentHealth < 1f)
                {
                    HealData healData = new HealData(1f - hero.Status.currentHealth);
                    healData.SetActor(hero);
                    hero.DoHeal(healData, hero);
                    RPGLog.Debug(" Cheat Death: Healed to 1 HP.");
                }
                
                float shieldAmount = hero.Status.maxHealth * 1.0f;
                hero.GiveShield(hero, shieldAmount, 10f, false);
                RPGLog.Debug(string.Format(" Cheat Death: Shield {0} given to hero", shieldAmount));
                break;
                
            case ItemRarity.Rare:
                // Wind Rush: Apply 50% movement speed buff for 3 seconds
                {
                    uint heroNetId = hero.netId;
                    
                    // Remove old Wind Rush buff if exists (use TryGetValue for thread safety)
                    StatBonus oldWindRushBuff;
                    if (_heroWindRushBonuses.TryGetValue(heroNetId, out oldWindRushBuff) && oldWindRushBuff != null)
                    {
                        try { hero.Status.RemoveStatBonus(oldWindRushBuff); } catch { }
                    }
                    
                    // Apply new buff
                StatBonus speedBuff = new StatBonus();
                speedBuff.movementSpeedPercentage = 50f;
                    StatBonus appliedBuff = hero.Status.AddStatBonus(speedBuff);
                    
                    // Track for removal - use realtime (doesn't reset between scenes!)
                    _heroWindRushBonuses[heroNetId] = appliedBuff;
                    _heroWindRushEndTimes[heroNetId] = Time.realtimeSinceStartup + 3f; // 3 second duration
                    
                    RPGLog.Debug(" Wind Rush: +50% speed applied, will expire in 3s (realtime)");
                }
                break;
                
            case ItemRarity.Epic:
                // Nightmare Burst: AoE damage = 300% ATK
                {
                    float aoeDamage = hero.Status.attackDamage * 3f;
                    DealAoEDamageForHero(hero, position, 5f, aoeDamage);
                    RPGLog.Debug(string.Format(" Nightmare Burst: {0} AoE damage", aoeDamage));
                }
                break;
                
            case ItemRarity.Legendary:
                // Divine Wrath: 500% ATK AoE + 20% heal
                {
                    float aoeDamage = hero.Status.attackDamage * 5f;
                    DealAoEDamageForHero(hero, position, 6f, aoeDamage);
                    
                    // Heal for 20% max HP
                    float healAmount = hero.Status.maxHealth * 0.20f;
                    HealData healData = new HealData(healAmount);
                    healData.SetActor(hero);
                    hero.DoHeal(healData, hero);
                    RPGLog.Debug(string.Format(" Divine Wrath: {0} AoE damage, healed {1}", aoeDamage, healAmount));
                }
                break;
        }
    }
    
    // Cache for enemy target validator (reused for AoE damage)
    private static AbilityTargetValidator _enemyValidator = new AbilityTargetValidator
    {
        targets = EntityRelation.Enemy
    };
    
    /// <summary>
    /// Deal AoE damage centered at a position for a specific hero
    /// Uses the game's proper DewPhysics.OverlapCircleAllEntities method (like Essence of Confidence and other AoE effects)
    /// </summary>
    private static void DealAoEDamageForHero(Hero attacker, Vector3 center, float radius, float damage)
    {
        if (!NetworkServer.active) return;
        
        int hitCount = 0;
        
        // Use the game's proper physics-based AoE detection (like Ai_Mon_Despair_WretchedArtillery_BarrageAtk_AoE)
        // This automatically handles entity filtering and avoids "Collection was modified" exceptions
        ListReturnHandle<Entity> handle;
        List<Entity> entitiesInRange = DewPhysics.OverlapCircleAllEntities(out handle, center, radius, _enemyValidator, attacker);
        
        // Deal damage to all entities in range
        foreach (Entity entity in entitiesInRange)
        {
            if (entity == null || entity.IsNullInactiveDeadOrKnockedOut()) continue;
            
            // Deal damage
            DamageData dmg = attacker.MagicDamage(damage, 0f); // 0 proc coefficient to avoid infinite loops
            attacker.DealDamage(dmg, entity);
            hitCount++;
        }
        
        // Return the handle to the pool (important for memory management)
        handle.Return();
        
        RPGLog.Debug(string.Format(" AoE hit {0} enemies for {1} damage each", hitCount, damage));
    }
    
    /// <summary>
    /// Process Cheat Death registration/unregistration from clients
    /// Format: [RPGCHEATDEATH]heroNetId|register (1=register, 0=unregister)
    /// </summary>
    private static void ProcessCheatDeathRegistration(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            int idx = content.IndexOf("[RPGCHEATDEATH]");
            if (idx < 0) return;
            // "[RPGCHEATDEATH]" is 15 characters, so we need idx + 15 to get the data after it
            string data = content.Substring(idx + 15);
            string[] parts = data.Split('|');
            if (parts.Length < 2) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            bool register = parts[1] == "1";
            
            // Validate netId
            if (heroNetId == 0)
            {
                RPGLog.Debug(" CheatDeath: Invalid netId (0) received in registration message");
                return;
            }
            
            // Find the hero - try multiple methods for reliability
            Hero hero = null;
            
            // Method 1: Try DewPlayer.allHumanPlayers (most common case)
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            // Method 2: Try ActorManager.allHeroes (more reliable, includes all heroes)
            if (hero == null && NetworkedManagerBase<ActorManager>.instance != null)
            {
                foreach (Hero h in NetworkedManagerBase<ActorManager>.instance.allHeroes)
                {
                    if (h != null && !h.IsNullOrInactive() && h.netId == heroNetId)
                    {
                        hero = h;
                        break;
                    }
                }
            }
            
            // Method 3: Try NetworkClient.spawned (fallback for network objects)
            if (hero == null && NetworkClient.spawned != null)
            {
                NetworkIdentity identity;
                if (NetworkClient.spawned.TryGetValue(heroNetId, out identity))
                {
                    hero = identity.GetComponent<Hero>();
                }
            }
            
            if (hero == null || hero.IsNullOrInactive())
            {
                // Hero not found or inactive - this is a real issue if we've been in game for a while
                // Log available heroes for debugging
                string availableHeroes = "";
                if (NetworkedManagerBase<ActorManager>.instance != null)
                {
                    var heroNetIds = new List<uint>();
                    foreach (Hero h in NetworkedManagerBase<ActorManager>.instance.allHeroes)
                    {
                        if (h != null && !h.IsNullOrInactive())
                        {
                            heroNetIds.Add(h.netId);
                        }
                    }
                    availableHeroes = string.Format(" Available heroes: [{0}]", string.Join(", ", heroNetIds));
                }
                
                RPGLog.Debug(string.Format(" CheatDeath: Hero {0} not found or inactive. Register={1}.{2}", 
                    heroNetId, register, availableHeroes));
                return;
            }
            
            if (register)
            {
                // Register Cheat Death for this hero
                if (!_heroesWithCheatDeath.Contains(heroNetId))
                {
                    _heroesWithCheatDeath.Add(heroNetId);
                    _heroCheatDeathCooldowns[heroNetId] = 0f; // Initialize cooldown
                    
                    // Subscribe to EntityEvent_OnTakeDamage for this hero (if not already subscribed)
                    if (!_cheatDeathHandlers.ContainsKey(hero))
                    {
                        Action<EventInfoDamage> handler = (EventInfoDamage damageInfo) =>
                        {
                            OnServerCheatDeathDamage(damageInfo);
                        };
                        hero.EntityEvent_OnTakeDamage += handler;
                        _cheatDeathHandlers[hero] = handler;
                        RPGLog.Debug(string.Format(" CheatDeath: Registered for hero {0} (server-side)", heroNetId));
                    }
                }
            }
            else
            {
                // Unregister Cheat Death for this hero
                if (_heroesWithCheatDeath.Contains(heroNetId))
                {
                    _heroesWithCheatDeath.Remove(heroNetId);
                    _heroCheatDeathCooldowns.Remove(heroNetId);
                    
                    // Unsubscribe from EntityEvent_OnTakeDamage
                    Action<EventInfoDamage> handler;
                    if (_cheatDeathHandlers.TryGetValue(hero, out handler))
                    {
                        try
                        {
                            hero.EntityEvent_OnTakeDamage -= handler;
                        }
                        catch (Exception e)
                        {
                            RPGLog.Warning(" Error unsubscribing Cheat Death handler: " + e.Message);
                        }
                        _cheatDeathHandlers.Remove(hero);
                        RPGLog.Debug(string.Format(" CheatDeath: Unregistered for hero {0}", heroNetId));
                    }
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing Cheat Death registration: " + e.Message);
        }
    }
    
    /// <summary>
    /// Server-side handler for Cheat Death - intercepts fatal damage for ALL heroes
    /// This runs on the server when ANY hero takes damage
    /// </summary>
    private static void OnServerCheatDeathDamage(EventInfoDamage damageInfo)
    {
        if (!NetworkServer.active) return;
        
        Hero hero = damageInfo.victim as Hero;
        if (hero == null || hero.IsNullOrInactive()) return;
        
        uint heroNetId = hero.netId;
        
        // Check if this hero has Cheat Death active
        if (!_heroesWithCheatDeath.Contains(heroNetId)) return;
        
        // Check cooldown
        float realtime = Time.realtimeSinceStartup;
        float cooldownEndTime;
        if (!_heroCheatDeathCooldowns.TryGetValue(heroNetId, out cooldownEndTime))
        {
            cooldownEndTime = 0f;
        }
        
        if (realtime < cooldownEndTime)
        {
            // On cooldown, don't trigger
            return;
        }
        
        // Check if damage would be fatal (current health - damage <= 0)
        float healthAfterDamage = hero.Status.currentHealth - damageInfo.damage.amount;
        if (healthAfterDamage > 0f)
        {
            // Not fatal, don't trigger
            return;
        }
        
        // Fatal damage detected! Trigger Cheat Death
        // Set cooldown FIRST to prevent double triggers
        RPGItemsConfig config = RPGItemsMod.GetConfig();
        float cooldown = (config != null) ? config.setBonusProcCooldownCommon : 60f;
        _heroCheatDeathCooldowns[heroNetId] = realtime + cooldown;
        
        // CRITICAL: Heal to 1 HP FIRST to prevent death
        if (hero.Status.currentHealth < 1f)
        {
            HealData healData = new HealData(1f - hero.Status.currentHealth);
            healData.SetActor(hero);
            hero.DoHeal(healData, hero);
            RPGLog.Debug(" Cheat Death: Healed to 1 HP.");
        }
        
        // Give shield equal to 100% max HP for 10 seconds
        float shieldAmount = hero.Status.maxHealth * 1.0f;
        hero.GiveShield(hero, shieldAmount, 10f, false);
        
        // IMPORTANT: Nullify the incoming damage to prevent actual death
        damageInfo.damage.amount = 0f;
        damageInfo.damage.discardedAmount = 0f;
        
        RPGLog.Debug(string.Format(" === CHEAT DEATH TRIGGERED for hero {0} === Prevented death! Shield: {1}, Next available in {2}s", 
            heroNetId, shieldAmount, cooldown));
    }
    
    // Track gold on hit subscriptions for client heroes
    private static Dictionary<uint, GoldOnHitData> _goldOnHitSubscriptions = new Dictionary<uint, GoldOnHitData>();
    
    private class GoldOnHitData
    {
        public Hero hero;
        public DewPlayer player;
        public int goldAmount;
        public Action<EventInfoDamage> handler;
    }
    
    private static void ProcessGoldOnHitSubscription(string content)
    {
        try
        {
            // Format: [RPGGOLDSUB]heroNetId|amount|S/U
            int idx = content.IndexOf("[RPGGOLDSUB]");
            string msgData = content.Substring(idx + 12);
            string[] parts = msgData.Split('|');
            if (parts.Length < 3) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int amount = int.Parse(parts[1]);
            bool subscribe = parts[2] == "S";
            
            if (subscribe)
            {
                // Find the hero and player
                Hero hero = null;
                DewPlayer player = null;
                foreach (DewPlayer p in DewPlayer.allHumanPlayers)
                {
                    if (p != null && p.hero != null && p.hero.netId == heroNetId)
                    {
                        hero = p.hero;
                        player = p;
                        break;
                    }
                }
                
                if (hero == null || player == null) return;
                
                // Remove existing subscription if any
                if (_goldOnHitSubscriptions.ContainsKey(heroNetId))
                {
                    GoldOnHitData oldData = _goldOnHitSubscriptions[heroNetId];
                    if (oldData.hero != null && oldData.handler != null)
                    {
                        oldData.hero.ActorEvent_OnDealDamage -= oldData.handler;
                    }
                    _goldOnHitSubscriptions.Remove(heroNetId);
                }
                
                // Create new subscription
                GoldOnHitData subData = new GoldOnHitData();
                subData.hero = hero;
                subData.player = player;
                subData.goldAmount = amount;
                
                subData.handler = (EventInfoDamage info) =>
                {
                    // Safety checks - hero or player may have been destroyed
                    if (subData.hero == null || subData.hero.IsNullInactiveDeadOrKnockedOut()) return;
                    if (subData.player == null) return;
                    if (info.damage.amount <= 0) return;
                    if (info.victim == null || info.victim == subData.hero) return;
                    
                    // Give gold to the player
                    try { subData.player.AddGold(subData.goldAmount); } catch { }
                };
                
                hero.ActorEvent_OnDealDamage += subData.handler;
                _goldOnHitSubscriptions[heroNetId] = subData;
            }
            else
            {
                // Unsubscribe
                if (_goldOnHitSubscriptions.ContainsKey(heroNetId))
                {
                    GoldOnHitData subData = _goldOnHitSubscriptions[heroNetId];
                    if (subData.hero != null && subData.handler != null)
                    {
                        subData.hero.ActorEvent_OnDealDamage -= subData.handler;
                    }
                    _goldOnHitSubscriptions.Remove(heroNetId);
                }
            }
        }
        catch { }
    }
    
    private static void ProcessStatRequest(string content)
    {
        try
        {
            // Format: [RPGSTAT]heroNetId|slotType|atk|def|hp|moveSpd|dodgeCharges|goldOnHit|A/R
            int idx = content.IndexOf("[RPGSTAT]");
            string data = content.Substring(idx + 9);
            string[] parts = data.Split('|');
            if (parts.Length < 9) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int slotType = int.Parse(parts[1]);
            int atk = int.Parse(parts[2]);
            int def = int.Parse(parts[3]);
            int hp = int.Parse(parts[4]);
            float moveSpd = float.Parse(parts[5]);
            int dodgeCharges = int.Parse(parts[6]);
            int goldOnHit = int.Parse(parts[7]); // Gold on hit (tracked locally, not applied as stat)
            bool add = parts[8] == "A";
            
            // Find the hero by netId
            Hero hero = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            if (hero == null) return;
            
            if (add)
            {
                // Add stat bonus
                StatBonus bonus = new StatBonus();
                bonus.attackDamageFlat = atk;
                bonus.armorFlat = def;
                bonus.maxHealthFlat = hp;
                bonus.movementSpeedPercentage = moveSpd;
                
                StatBonus addedBonus = hero.Status.AddStatBonus(bonus);
                
                // Track it
                if (!_heroStatBonuses.ContainsKey(heroNetId))
                {
                    _heroStatBonuses[heroNetId] = new Dictionary<int, StatBonus>();
                }
                _heroStatBonuses[heroNetId][slotType] = addedBonus;
                
                // Add dodge charges via SkillBonus on Movement skill (like constellations do)
                if (dodgeCharges != 0 && hero.Skill != null && hero.Skill.Movement != null)
                {
                    // IMPORTANT: Stop any existing skill bonus for this slot first to prevent accumulation!
                    if (_heroSkillBonuses.ContainsKey(heroNetId) && _heroSkillBonuses[heroNetId].ContainsKey(slotType))
                    {
                        SkillBonus oldBonus = _heroSkillBonuses[heroNetId][slotType];
                        if (oldBonus != null)
                        {
                            try { oldBonus.Stop(); } catch { }
                        }
                    }
                    
                    SkillBonus skillBonus = hero.Skill.Movement.AddSkillBonus(new SkillBonus
                    {
                        addedCharge = dodgeCharges
                    });
                    
                    if (!_heroSkillBonuses.ContainsKey(heroNetId))
                    {
                        _heroSkillBonuses[heroNetId] = new Dictionary<int, SkillBonus>();
                    }
                    _heroSkillBonuses[heroNetId][slotType] = skillBonus;
                }
            }
            else
            {
                // Remove stat bonus (use TryGetValue for nested dictionary access - thread safety)
                Dictionary<int, StatBonus> heroStatBonuses;
                if (_heroStatBonuses.TryGetValue(heroNetId, out heroStatBonuses))
                {
                    StatBonus bonus;
                    if (heroStatBonuses.TryGetValue(slotType, out bonus))
                    {
                    if (bonus != null)
                    {
                            try { hero.Status.RemoveStatBonus(bonus); } catch { }
                    }
                        heroStatBonuses.Remove(slotType);
                    }
                }
                
                // Remove skill bonus (dodge charges) - use TryGetValue for thread safety
                Dictionary<int, SkillBonus> heroSkillBonuses;
                if (_heroSkillBonuses.TryGetValue(heroNetId, out heroSkillBonuses))
                {
                    SkillBonus skillBonus;
                    if (heroSkillBonuses.TryGetValue(slotType, out skillBonus))
                    {
                    if (skillBonus != null)
                    {
                            try { skillBonus.Stop(); } catch { }
                    }
                        heroSkillBonuses.Remove(slotType);
                    RPGLog.Debug(" Server removed dodge charges for client hero");
                    }
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing stat request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Process stat request v2 - includes more stats (AP, regen, attack speed, haste, crit)
    /// Format: [RPGSTAT2]heroNetId|slotType|atk|def|hp|ap|moveSpd|dodgeCharges|regen|atkSpd|haste|critCh|critDmg|A/R
    /// </summary>
    private static void ProcessStatRequest2(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            int idx = content.IndexOf("[RPGSTAT2]");
            if (idx < 0) return;
            string data = content.Substring(idx + 10);
            string[] parts = data.Split('|');
            if (parts.Length < 14) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int slotType = int.Parse(parts[1]);
            int atk = int.Parse(parts[2]);
            int def = int.Parse(parts[3]);
            int hp = int.Parse(parts[4]);
            int ap = int.Parse(parts[5]);
            float moveSpd = float.Parse(parts[6]);
            int dodgeCharges = int.Parse(parts[7]);
            float regen = float.Parse(parts[8]);
            float atkSpd = float.Parse(parts[9]);
            float haste = float.Parse(parts[10]);
            float critCh = float.Parse(parts[11]);
            float critDmg = float.Parse(parts[12]);
            bool add = parts[13] == "A";
            
            // Find the hero by netId
            Hero hero = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            if (hero == null) return;
            
            if (add)
            {
                // Add stat bonus
                StatBonus bonus = new StatBonus();
                bonus.attackDamageFlat = atk;
                bonus.armorFlat = def;
                bonus.maxHealthFlat = hp;
                bonus.abilityPowerFlat = ap;
                bonus.movementSpeedPercentage = moveSpd;
                bonus.healthRegenFlat = regen;
                bonus.attackSpeedPercentage = atkSpd;
                bonus.abilityHasteFlat = haste;
                bonus.critChanceFlat = critCh / 100f; // Convert from percentage
                bonus.critAmpFlat = critDmg / 100f; // Convert from percentage (crit damage = crit amp)
                
                StatBonus addedBonus = hero.Status.AddStatBonus(bonus);
                
                // Track it
                if (!_heroStatBonuses.ContainsKey(heroNetId))
                {
                    _heroStatBonuses[heroNetId] = new Dictionary<int, StatBonus>();
                }
                _heroStatBonuses[heroNetId][slotType] = addedBonus;
                
                // Add dodge charges via SkillBonus on Movement skill
                if (dodgeCharges != 0 && hero.Skill != null && hero.Skill.Movement != null)
                {
                    // IMPORTANT: Stop any existing skill bonus for this slot first to prevent accumulation!
                    if (_heroSkillBonuses.ContainsKey(heroNetId) && _heroSkillBonuses[heroNetId].ContainsKey(slotType))
                    {
                        SkillBonus oldBonus = _heroSkillBonuses[heroNetId][slotType];
                        if (oldBonus != null)
                        {
                            try { oldBonus.Stop(); } catch { }
                        }
                    }
                    
                    SkillBonus skillBonus = hero.Skill.Movement.AddSkillBonus(new SkillBonus
                    {
                        addedCharge = dodgeCharges
                    });
                    
                    if (!_heroSkillBonuses.ContainsKey(heroNetId))
                    {
                        _heroSkillBonuses[heroNetId] = new Dictionary<int, SkillBonus>();
                    }
                    _heroSkillBonuses[heroNetId][slotType] = skillBonus;
                }
                
                RPGLog.Debug(" Server applied stat bonus v2 for client hero");
            }
            else
            {
                // Remove stat bonus (use TryGetValue for nested dictionary access - thread safety)
                Dictionary<int, StatBonus> heroStatBonuses;
                if (_heroStatBonuses.TryGetValue(heroNetId, out heroStatBonuses))
                {
                    StatBonus bonus;
                    if (heroStatBonuses.TryGetValue(slotType, out bonus))
                    {
                    if (bonus != null)
                    {
                            try { hero.Status.RemoveStatBonus(bonus); } catch { }
                    }
                        heroStatBonuses.Remove(slotType);
                    }
                }
                
                // Remove skill bonus (dodge charges) - use TryGetValue for thread safety
                Dictionary<int, SkillBonus> heroSkillBonuses;
                if (_heroSkillBonuses.TryGetValue(heroNetId, out heroSkillBonuses))
                {
                    SkillBonus skillBonus;
                    if (heroSkillBonuses.TryGetValue(slotType, out skillBonus))
                    {
                    if (skillBonus != null)
                    {
                            try { skillBonus.Stop(); } catch { }
                    }
                        heroSkillBonuses.Remove(slotType);
                    }
                }
                
                RPGLog.Debug(" Server removed stat bonus v2 for client hero");
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing stat request v2: " + e.Message);
        }
    }
    
    /// <summary>
    /// Process stat request v3 - includes dodge cooldown reduction
    /// Format: [RPGSTAT3]heroNetId|slotType|atk|def|hp|ap|moveSpd|dodgeCharges|dodgeCdRed|regen|atkSpd|haste|critCh|critDmg|A/R
    /// </summary>
    private static void ProcessStatRequest3(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            int idx = content.IndexOf("[RPGSTAT3]");
            if (idx < 0) return;
            string data = content.Substring(idx + 10);
            string[] parts = data.Split('|');
            if (parts.Length < 15) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int slotType = int.Parse(parts[1]);
            int atk = int.Parse(parts[2]);
            int def = int.Parse(parts[3]);
            int hp = int.Parse(parts[4]);
            int ap = int.Parse(parts[5]);
            float moveSpd = float.Parse(parts[6]);
            int dodgeCharges = int.Parse(parts[7]);
            float dodgeCdRed = float.Parse(parts[8]);
            float regen = float.Parse(parts[9]);
            float atkSpd = float.Parse(parts[10]);
            float haste = float.Parse(parts[11]);
            float critCh = float.Parse(parts[12]);
            float critDmg = float.Parse(parts[13]);
            bool add = parts[14] == "A";
            
            // Find the hero by netId
            Hero hero = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            if (hero == null) return;
            
            if (add)
            {
                // Add stat bonus
                StatBonus bonus = new StatBonus();
                bonus.attackDamageFlat = atk;
                bonus.armorFlat = def;
                bonus.maxHealthFlat = hp;
                bonus.abilityPowerFlat = ap;
                bonus.movementSpeedPercentage = moveSpd;
                bonus.healthRegenFlat = regen;
                bonus.attackSpeedPercentage = atkSpd;
                bonus.abilityHasteFlat = haste;
                bonus.critChanceFlat = critCh / 100f;
                bonus.critAmpFlat = critDmg / 100f;
                
                StatBonus addedBonus = hero.Status.AddStatBonus(bonus);
                
                if (!_heroStatBonuses.ContainsKey(heroNetId))
                {
                    _heroStatBonuses[heroNetId] = new Dictionary<int, StatBonus>();
                }
                _heroStatBonuses[heroNetId][slotType] = addedBonus;
                
                // Add dodge charges and cooldown reduction via SkillBonus
                if ((dodgeCharges != 0 || dodgeCdRed > 0) && hero.Skill != null && hero.Skill.Movement != null)
                {
                    // IMPORTANT: Stop any existing skill bonus for this slot first to prevent accumulation!
                    if (_heroSkillBonuses.ContainsKey(heroNetId) && _heroSkillBonuses[heroNetId].ContainsKey(slotType))
                    {
                        SkillBonus oldBonus = _heroSkillBonuses[heroNetId][slotType];
                        if (oldBonus != null)
                        {
                            try { oldBonus.Stop(); } catch { }
                        }
                    }
                    
                    SkillBonus skillBonus = hero.Skill.Movement.AddSkillBonus(new SkillBonus
                    {
                        addedCharge = dodgeCharges,
                        // cooldownMultiplier: 1.0 = normal, 0.8 = 20% faster
                        cooldownMultiplier = dodgeCdRed > 0 ? (1f - dodgeCdRed / 100f) : 1f
                    });
                    
                    if (!_heroSkillBonuses.ContainsKey(heroNetId))
                    {
                        _heroSkillBonuses[heroNetId] = new Dictionary<int, SkillBonus>();
                    }
                    _heroSkillBonuses[heroNetId][slotType] = skillBonus;
                }
                
                RPGLog.Debug(" Server applied stat bonus v3 for client hero");
            }
            else
            {
                // Remove stat bonus (use TryGetValue for nested dictionary access - thread safety)
                Dictionary<int, StatBonus> heroStatBonuses;
                if (_heroStatBonuses.TryGetValue(heroNetId, out heroStatBonuses))
                {
                    StatBonus bonus;
                    if (heroStatBonuses.TryGetValue(slotType, out bonus))
                    {
                    if (bonus != null)
                    {
                            try { hero.Status.RemoveStatBonus(bonus); } catch { }
                    }
                        heroStatBonuses.Remove(slotType);
                    }
                }
                
                // Remove skill bonus - use TryGetValue for thread safety
                Dictionary<int, SkillBonus> heroSkillBonuses;
                if (_heroSkillBonuses.TryGetValue(heroNetId, out heroSkillBonuses))
                {
                    SkillBonus skillBonus;
                    if (heroSkillBonuses.TryGetValue(slotType, out skillBonus))
                    {
                    if (skillBonus != null)
                    {
                            try { skillBonus.Stop(); } catch { }
                    }
                        heroSkillBonuses.Remove(slotType);
                    }
                }
                
                RPGLog.Debug(" Server removed stat bonus v3 for client hero");
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing stat request v3: " + e.Message);
        }
    }
    
    private static void ProcessHealRequest(string content)
    {
        try
        {
            // Format: [RPGHEAL]heroNetId|healAmount|shieldAmount
            int idx = content.IndexOf("[RPGHEAL]");
            string data = content.Substring(idx + 9);
            string[] parts = data.Split('|');
            if (parts.Length < 2) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int healAmount = int.Parse(parts[1]);
            int shieldAmount = parts.Length >= 3 ? int.Parse(parts[2]) : 0;
            
            // Find the hero by netId
            Hero hero = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            if (hero == null) return;
            
            // Apply heal on server
            if (healAmount > 0)
            {
                HealData healData = new HealData(healAmount);
                healData.SetActor(hero);
                healData.Dispatch(hero);
            }
            
            // Apply shield on server
            if (shieldAmount > 0)
            {
                // Use game's GiveShield method - 10 second duration, non-decaying
                hero.GiveShield(hero, shieldAmount, 10f, false);
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing heal request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Process heal request with VFX from client - uses Se_GenericHealOverTime for proper heal effects
    /// </summary>
    private static void ProcessHealRequestWithVFX(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Format: [RPGHEALVFX]heroNetId|healAmount|shieldAmount
            int idx = content.IndexOf("[RPGHEALVFX]");
            string data = content.Substring(idx + 12);
            string[] parts = data.Split('|');
            if (parts.Length < 2) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int healAmount = int.Parse(parts[1]);
            int shieldAmount = parts.Length >= 3 ? int.Parse(parts[2]) : 0;
            
            // Find the hero by netId
            Hero hero = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            if (hero == null) return;
            
            // Apply heal on server - direct heal
            if (healAmount > 0)
            {
                HealData healData = new HealData(healAmount);
                healData.SetActor(hero);
                healData.Dispatch(hero);
            }
            
            // Apply shield on server
            if (shieldAmount > 0)
            {
                hero.GiveShield(hero, shieldAmount, 10f, false);
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing heal VFX request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Process sell item request from client - give gold to player
    /// </summary>
    private static void ProcessSellRequest(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Format: [RPGSELL]playerNetId|goldAmount
            int idx = content.IndexOf("[RPGSELL]");
            string data = content.Substring(idx + 9);
            string[] parts = data.Split('|');
            if (parts.Length < 2) return;
            
            uint playerNetId = uint.Parse(parts[0]);
            int goldAmount = int.Parse(parts[1]);
            
            // Find the player by hero netId
            DewPlayer targetPlayer = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == playerNetId)
                {
                    targetPlayer = player;
                    break;
                }
            }
            
            if (targetPlayer == null) return;
            
            // Give gold to player (server-side)
            targetPlayer.AddGold(goldAmount);
            RPGLog.Debug(" Server gave " + goldAmount + " gold to player " + targetPlayer.playerName);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing sell request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Process spend dust request from client - for upgrades (SPENDS dust)
    /// </summary>
    private static void ProcessSpendDustRequest(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Format: [RPGSPEND]playerNetId|dustAmount
            int idx = content.IndexOf("[RPGSPEND]");
            if (idx < 0) return;
            string data = content.Substring(idx + 10);
            string[] parts = data.Split('|');
            if (parts.Length < 2) return;
            
            uint playerNetId = uint.Parse(parts[0]);
            int dustAmount = int.Parse(parts[1]);
            
            // Find the player by hero netId
            DewPlayer targetPlayer = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == playerNetId)
                {
                    targetPlayer = player;
                    break;
                }
            }
            
            if (targetPlayer == null) return;
            
            // Check if player can afford
            if (targetPlayer.dreamDust < dustAmount)
            {
                RPGLog.Warning(" Player cannot afford upgrade: " + dustAmount + " dust required, has " + targetPlayer.dreamDust);
                return;
            }
            
            // Spend dust (server-side)
            targetPlayer.SpendDreamDust(dustAmount);
            RPGLog.Debug(" Server spent " + dustAmount + " dust for player " + targetPlayer.playerName);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing spend dust request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Process give dust request from client - for dismantle/cleanse refund (GIVES dust)
    /// </summary>
    private static void ProcessGiveDustRequest(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Format: [RPGDUST]playerNetId|dustAmount
            int idx = content.IndexOf("[RPGDUST]");
            if (idx < 0) return;
            string data = content.Substring(idx + 9);
            string[] parts = data.Split('|');
            if (parts.Length < 2) return;
            
            uint playerNetId = uint.Parse(parts[0]);
            int dustAmount = int.Parse(parts[1]);
            
            // Find the player by hero netId
            DewPlayer targetPlayer = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == playerNetId)
                {
                    targetPlayer = player;
                    break;
                }
            }
            
            if (targetPlayer == null)
            {
                RPGLog.Warning(" Could not find player with netId " + playerNetId + " for dust reward");
                return;
            }
            
            // Give dust (server-side) - this is for dismantle/cleanse refund
            targetPlayer.AddDreamDust(dustAmount);
            RPGLog.Debug(" Server gave " + dustAmount + " dust to player " + targetPlayer.playerName);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing give dust request: " + e.Message);
        }
    }
    
    /// <summary>
    /// Process cleanse gold spend request from clients - spends gold for cleansing
    /// </summary>
    private static void ProcessCleanseGoldRequest(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Format: [RPGCLEANSE]playerNetId|goldAmount
            int idx = content.IndexOf("[RPGCLEANSE]");
            if (idx < 0) return;
            string data = content.Substring(idx + 12);
            string[] parts = data.Split('|');
            if (parts.Length < 2) return;
            
            uint playerNetId = uint.Parse(parts[0]);
            int goldAmount = int.Parse(parts[1]);
            
            // Find the player by hero netId
            DewPlayer targetPlayer = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == playerNetId)
                {
                    targetPlayer = player;
                    break;
                }
            }
            
            if (targetPlayer == null)
            {
                RPGLog.Warning(" Could not find player with netId " + playerNetId + " for cleanse gold spend");
                return;
            }
            
            // Verify player has enough gold
            if (targetPlayer.gold < goldAmount)
            {
                RPGLog.Warning(" Player " + targetPlayer.playerName + " doesn't have enough gold for cleanse");
                return;
            }
            
            // Spend gold (server-side) - uses SpendGold to properly deduct
            targetPlayer.SpendGold(goldAmount);
            RPGLog.Debug(" Server spent " + goldAmount + " gold from player " + targetPlayer.playerName + " for cleanse");
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing cleanse gold request: " + e.Message);
        }
    }
    
    // Track StatsSystem bonuses per hero
    private static Dictionary<uint, StatBonus> _heroStatsSystemBonuses = new Dictionary<uint, StatBonus>();
    
    /// <summary>
    /// Process StatsSystem stat apply request from client
    /// </summary>
    private static void ProcessStatsSystemRequest(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Format: [RPGSTATS]heroNetId|atk|hp|def|ap|moveSpd|atkSpd|crit
            int idx = content.IndexOf("[RPGSTATS]");
            if (idx < 0) return;
            string data = content.Substring(idx + 10);
            string[] parts = data.Split('|');
            if (parts.Length < 8) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            float atk = float.Parse(parts[1]);
            float hp = float.Parse(parts[2]);
            float def = float.Parse(parts[3]);
            float ap = float.Parse(parts[4]);
            float moveSpd = float.Parse(parts[5]);
            float atkSpd = float.Parse(parts[6]);
            float crit = float.Parse(parts[7]);
            
            // Find the hero by netId
            Hero hero = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            if (hero == null) return;
            
            // Remove old bonus if exists (use TryGetValue for thread safety)
            StatBonus oldBonus;
            if (_heroStatsSystemBonuses.TryGetValue(heroNetId, out oldBonus))
            {
                if (oldBonus != null)
                {
                    try { hero.Status.RemoveStatBonus(oldBonus); } catch { }
                }
                _heroStatsSystemBonuses.Remove(heroNetId);
            }
            
            // Create and apply new bonus
            StatBonus bonus = new StatBonus();
            bonus.attackDamageFlat = atk;
            bonus.maxHealthFlat = hp;
            bonus.armorFlat = def;
            bonus.abilityPowerFlat = ap;
            bonus.movementSpeedPercentage = moveSpd;
            bonus.attackSpeedPercentage = atkSpd;
            bonus.critChanceFlat = crit;
            
            StatBonus addedBonus = hero.Status.AddStatBonus(bonus);
            _heroStatsSystemBonuses[heroNetId] = addedBonus;
            
            RPGLog.Info(string.Format(" SERVER: Successfully applied StatsSystem bonus for client hero {0}: ATK+{1}, HP+{2}, DEF+{3}, AP+{4}", 
                hero.name, (int)atk, (int)hp, (int)def, (int)ap));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing StatsSystem request: " + e.Message);
        }
    }
    
    // Track Mastery bonuses per hero
    private static Dictionary<uint, StatBonus> _heroMasteryBonuses = new Dictionary<uint, StatBonus>();
    
    /// <summary>
    /// Process WeaponMastery bonus request from client
    /// </summary>
    private static void ProcessMasteryBonusRequest(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            // Format: [RPGMASTERY]heroNetId|dmgBonus|critBonus|atkSpdBonus
            int idx = content.IndexOf("[RPGMASTERY]");
            if (idx < 0) return;
            string data = content.Substring(idx + 12);
            string[] parts = data.Split('|');
            if (parts.Length < 4) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            float dmgBonus = float.Parse(parts[1]);
            float critBonus = float.Parse(parts[2]);
            float atkSpdBonus = float.Parse(parts[3]);
            
            // Find the hero by netId
            Hero hero = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            if (hero == null) return;
            
            // Remove old bonus if exists (use TryGetValue for thread safety)
            StatBonus oldBonus;
            if (_heroMasteryBonuses.TryGetValue(heroNetId, out oldBonus))
            {
                if (oldBonus != null)
                {
                    try { hero.Status.RemoveStatBonus(oldBonus); } catch { }
                }
                _heroMasteryBonuses.Remove(heroNetId);
            }
            
            // Only apply if there are actual bonuses
            if (dmgBonus <= 0 && critBonus <= 0 && atkSpdBonus <= 0)
            {
                return;
            }
            
            // Create and apply new bonus
            StatBonus bonus = new StatBonus();
            bonus.attackDamagePercentage = dmgBonus;
            bonus.critChanceFlat = critBonus / 100f; // Convert to decimal
            bonus.attackSpeedPercentage = atkSpdBonus;
            
            StatBonus addedBonus = hero.Status.AddStatBonus(bonus);
            _heroMasteryBonuses[heroNetId] = addedBonus;
            
            RPGLog.Debug(string.Format(" Server applied Mastery bonus for client hero: +{0:F1}% DMG, +{1:F1}% Crit, +{2:F1}% AtkSpd", 
                dmgBonus, critBonus, atkSpdBonus));
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing Mastery bonus request: " + e.Message);
        }
    }
    
    // Track thorns subscriptions for client heroes
    private static Dictionary<uint, ThornsData> _thornsSubscriptions = new Dictionary<uint, ThornsData>();
    
    private class ThornsData
    {
        public Hero hero;
        public int thornsAmount;
        public Action<EventInfoDamage> handler;
    }
    
    /// <summary>
    /// Process thorns damage subscription request from client
    /// Format: [RPGTHORNSSUB]heroNetId|thorns|S/U (subscribe/unsubscribe)
    /// </summary>
    private static void ProcessThornsSubscription(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            int idx = content.IndexOf("[RPGTHORNSSUB]");
            if (idx < 0) return;
            string data = content.Substring(idx + 14);
            string[] parts = data.Split('|');
            if (parts.Length < 3) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            int thorns = int.Parse(parts[1]);
            string op = parts[2];
            
            // Find the hero
            Hero hero = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    break;
                }
            }
            
            if (hero == null) return;
            
            if (op == "S")
            {
                // Subscribe (use TryGetValue for thread safety)
                ThornsData existingData;
                if (_thornsSubscriptions.TryGetValue(heroNetId, out existingData))
                {
                    // Already subscribed, update thorns amount
                    existingData.thornsAmount = thorns;
                    return;
                }
                
                ThornsData thornsData = new ThornsData();
                thornsData.hero = hero;
                thornsData.thornsAmount = thorns;
                thornsData.handler = (EventInfoDamage info) =>
                {
                    if (thornsData.hero == null || thornsData.hero.IsNullInactiveDeadOrKnockedOut()) return;
                    if (info.actor == null || !info.actor.isActive) return;
                    if (thornsData.thornsAmount <= 0) return;
                    
                    // Deal thorns damage back to attacker (actor is the source of damage)
                    try
                    {
                        Entity attacker = info.actor.firstEntity;
                        if (attacker == null || attacker.IsNullInactiveDeadOrKnockedOut()) return;
                        
                        // CRITICAL: Don't reflect damage from self (e.g., Immolation)
                        // Also don't reflect damage from allies
                        if (attacker == thornsData.hero) return;
                        if (attacker is Hero) return; // Don't reflect damage from any hero (self or ally)
                        
                        // Only reflect damage from monsters (enemies)
                        if (!(attacker is Monster)) return;
                        
                        DamageData thornsDmg = thornsData.hero.PhysicalDamage(thornsData.thornsAmount, 0f);
                        thornsData.hero.DealDamage(thornsDmg, attacker);
                    }
                    catch { }
                };
                
                hero.EntityEvent_OnTakeDamage += thornsData.handler;
                _thornsSubscriptions[heroNetId] = thornsData;
                RPGLog.Debug(" Server subscribed thorns for client hero: " + thorns);
            }
            else if (op == "U")
            {
                // Unsubscribe (use TryGetValue for thread safety)
                ThornsData thornsData;
                if (_thornsSubscriptions.TryGetValue(heroNetId, out thornsData))
                {
                    if (thornsData.hero != null && thornsData.handler != null)
                    {
                        try { thornsData.hero.EntityEvent_OnTakeDamage -= thornsData.handler; } catch { }
                    }
                    _thornsSubscriptions.Remove(heroNetId);
                    RPGLog.Debug(" Server unsubscribed thorns for client hero");
                }
            }
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error processing Thorns subscription: " + e.Message);
        }
    }
    
    // Track on-hit subscriptions for client heroes
    private static Dictionary<uint, OnHitData> _onHitSubscriptions = new Dictionary<uint, OnHitData>();
    
    private class OnHitData
    {
        public Hero hero;
        public DewPlayer player;
        public int lifesteal;
        public Action<EventInfoDamage> handler;
    }
    
    /// <summary>
    /// Process on-hit effects subscription request from client
    /// Format: [RPGONHITSUB]heroNetId|gold|dust|lifesteal|S/U (subscribe/unsubscribe)
    /// Note: gold parameter is now ignored (gold on kill is handled by MonsterLootSystem)
    /// </summary>
    private static void ProcessOnHitSubscription(string content)
    {
        if (!NetworkServer.active) return;
        
        try
        {
            int idx = content.IndexOf("[RPGONHITSUB]");
            if (idx < 0) return;
            string data = content.Substring(idx + 13);
            string[] parts = data.Split('|');
            if (parts.Length < 5) return;
            
            uint heroNetId = uint.Parse(parts[0]);
            // parts[1] was goldOnHit, parts[2] was dustOnHit - both now on-kill handled elsewhere
            int lifesteal = int.Parse(parts[3]);
            string op = parts[4];
            
            // Find the hero and player
            Hero hero = null;
            DewPlayer targetPlayer = null;
            foreach (DewPlayer player in DewPlayer.allHumanPlayers)
            {
                if (player != null && player.hero != null && player.hero.netId == heroNetId)
                {
                    hero = player.hero;
                    targetPlayer = player;
                    break;
                }
            }
            
            if (hero == null || targetPlayer == null) return;
            
            if (op == "S")
            {
                // Subscribe - remove old subscription first (use TryGetValue for thread safety)
                OnHitData oldData;
                if (_onHitSubscriptions.TryGetValue(heroNetId, out oldData))
                {
                    if (oldData.hero != null && oldData.handler != null)
                    {
                        try { oldData.hero.ActorEvent_OnDealDamage -= oldData.handler; } catch { }
                    }
                    _onHitSubscriptions.Remove(heroNetId);
                }
                
                // Only subscribe if there are actual bonuses (lifesteal only now)
                if (lifesteal <= 0) return;
                
                OnHitData onHitData = new OnHitData();
                onHitData.hero = hero;
                onHitData.player = targetPlayer;
                onHitData.lifesteal = lifesteal;
                
                // Create handler for deal damage events
                onHitData.handler = new Action<EventInfoDamage>((EventInfoDamage dmgInfo) =>
                {
                    if (onHitData.hero == null || onHitData.hero.IsNullInactiveDeadOrKnockedOut()) return;
                    if (onHitData.player == null) return;
                    if (dmgInfo.damage.amount <= 0) return;
                    
                    // Don't trigger on self-damage
                    if (dmgInfo.victim == onHitData.hero) return;
                    
                    try
                    {
                        // Lifesteal (only on-hit effect remaining)
                        if (onHitData.lifesteal > 0 && dmgInfo.damage.amount > 0)
                        {
                            float healAmount = dmgInfo.damage.amount * (onHitData.lifesteal / 100f);
                            if (healAmount >= 1f)
                            {
                                HealData heal = onHitData.hero.Heal(healAmount);
                                onHitData.hero.DoHeal(heal, onHitData.hero);
                            }
                        }
                    }
                    catch { }
                });
                
                hero.ActorEvent_OnDealDamage += onHitData.handler;
                _onHitSubscriptions[heroNetId] = onHitData;
            }
            else if (op == "U")
            {
                // Unsubscribe (use TryGetValue for thread safety)
                OnHitData oldData;
                if (_onHitSubscriptions.TryGetValue(heroNetId, out oldData))
                {
                    if (oldData.hero != null && oldData.handler != null)
                    {
                        try { oldData.hero.ActorEvent_OnDealDamage -= oldData.handler; } catch { }
                    }
                    _onHitSubscriptions.Remove(heroNetId);
                }
            }
        }
        catch { }
    }
}

/// <summary>
/// Harmony patch to show tooltips for RPG items in chat
/// </summary>
public static class ChatTooltipPatch
{
    public static bool Prefix_ShowTooltip(UI_Common_ChatBox_Item __instance, UI_TooltipManager tooltip)
    {
        // Use reflection to get the private _msg field
        FieldInfo msgField = typeof(UI_Common_ChatBox_Item).GetField("_msg", BindingFlags.Instance | BindingFlags.NonPublic);
        if (msgField == null) return true;
        
        ChatManager.Message msg = (ChatManager.Message)msgField.GetValue(__instance);
        
        if (string.IsNullOrEmpty(msg.itemType)) return true;
        
        // Check if it's our RPG item
        if (msg.itemType.StartsWith("RPGItem_"))
        {
            // Show our custom tooltip
            ShowRPGItemTooltip(__instance, tooltip, msg);
            return false; // Skip original method
        }
        
        return true; // Let original handle it
    }
    
    private static void ShowRPGItemTooltip(UI_Common_ChatBox_Item chatItem, UI_TooltipManager tooltip, ChatManager.Message msg)
    {
        try
        {
            // Deserialize item from customData
            RPGItem item = NetworkedItemSystem.DeserializeItemFromTooltip(msg.itemCustomData);
            if (item == null) return;
            
            // Get position for tooltip
            RectTransform rect = chatItem.GetComponent<RectTransform>();
            Rect screenRect = rect.GetScreenSpaceRect();
            Vector2 pos = new Vector2(screenRect.center.x, screenRect.max.y);
            
            // Use centralized tooltip from RPGItem
            string tooltipContent = item.GetFullTooltip();
            
            // Show using the raw text tooltip
            TooltipSettings settings = new TooltipSettings();
            settings.mode = TooltipPositionMode.RawValue;
            settings.position = pos;
            tooltip.ShowRawTextTooltip(settings, tooltipContent);
        }
        catch (Exception e)
        {
            RPGLog.Warning(" Error showing tooltip: " + e.Message);
        }
    }
}



