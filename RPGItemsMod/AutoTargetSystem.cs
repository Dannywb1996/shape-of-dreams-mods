using UnityEngine;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// Auto Target System - Handles auto attack and auto aim gear effects
/// Auto Attack: Automatically basic attacks the nearest enemy (similar to Prismatic Vision)
/// Auto Aim: Automatically aims memory abilities at the nearest target
/// 
/// Based on how the game's Se_D_PrismaticEyes (Prismatic Vision) works:
/// - Uses DewPhysics.OverlapCircleAllEntities for target finding
/// - Checks attack charges before attacking
/// - Uses proper attack range detection
/// - Runs in Update loop with proper state checks
/// </summary>
public class AutoTargetSystem
{
    private EquipmentManager equipmentManager;
    private float lastAutoAttackTime = 0f;
    private float autoAttackInterval = 0.1f; // Check every 100ms (slightly faster than game's 200ms attack move)
    
    // Track if effects are active
    private bool hasAutoAttack = false;
    private bool hasAutoAim = false;
    
    // Cache for target validator
    private static AbilityTargetValidator _enemyValidator = new AbilityTargetValidator
    {
        targets = EntityRelation.Enemy
    };
    
    public bool HasAutoAttack { get { return hasAutoAttack; } }
    public bool HasAutoAim { get { return hasAutoAim; } }
    
    public void Initialize(EquipmentManager manager)
    {
        equipmentManager = manager;
    }
    
    /// <summary>
    /// Update the active effects based on equipped items
    /// </summary>
    public void UpdateActiveEffects()
    {
        hasAutoAttack = false;
        hasAutoAim = false;
        
        if (equipmentManager == null) return;
        
        // Check all equipment slots for auto effects
        foreach (EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentSlotType)))
        {
            RPGItem item = equipmentManager.GetEquipped(slotType);
            if (item != null)
            {
                if (item.autoAttack) hasAutoAttack = true;
                if (item.autoAim) hasAutoAim = true;
            }
        }
    }
    
    /// <summary>
    /// Called every frame to process auto targeting
    /// Similar to Se_D_PrismaticEyes.ActiveLogicUpdate
    /// </summary>
    public void Update()
    {
        if (!hasAutoAttack && !hasAutoAim) return;
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        
        // Safety checks similar to Prismatic Vision
        if (hero.IsNullInactiveDeadOrKnockedOut()) return;
        
        // Don't auto-attack during transitions or cutscenes
        if (NetworkedManagerBase<ZoneManager>.instance != null && NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition) return;
        if (ManagerBase<CameraManager>.instance != null && ManagerBase<CameraManager>.instance.isPlayingCutscene) return;
        if (ManagerBase<TransitionManager>.instance != null && ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading) return;
        
        // Auto Attack logic
        if (hasAutoAttack && Time.time - lastAutoAttackTime > autoAttackInterval)
        {
            lastAutoAttackTime = Time.time;
            TryAutoAttack(hero);
        }
    }
    
    /// <summary>
    /// Try to auto attack the nearest enemy
    /// Based on Se_D_PrismaticEyes.ActiveLogicUpdate approach
    /// </summary>
    private void TryAutoAttack(Hero hero)
    {
        // Check if attack is blocked
        if (hero.Control.IsActionBlocked(EntityControl.BlockableAction.Attack) != EntityControl.BlockStatus.Allowed)
            return;
        
        // Check if we have an attack ability
        AttackTrigger attackAbility = hero.Ability.attackAbility as AttackTrigger;
        if (attackAbility == null || !attackAbility.isActiveAndEnabled)
            return;
        
        // Check if attack has charges available (not on cooldown)
        // This is how Prismatic Vision checks: attackAbility.currentConfigCurrentCharge <= 0
        if (attackAbility.currentConfigCurrentCharge <= 0)
            return;
        
        // Don't auto-attack if attack ability is currently casting
        if (attackAbility.Network_isCasting) return;
        
        // Calculate attack range (similar to Prismatic Vision: attack range + 1f buffer)
        float attackRange = attackAbility.currentConfig.effectiveRange + 1f;
        
        // Always find the nearest enemy target (this will switch targets automatically)
        // This ensures we always attack the closest valid target
        Entity target = ActionAttackMove.FindAttackMoveTarget(hero, hero.agentPosition);
        
        if (target != null)
        {
            // Verify target is within attack range
            float distance = Vector3.Distance(hero.agentPosition, target.agentPosition);
            if (distance <= attackRange)
            {
                // Check if target is valid for attack
                if (attackAbility.currentConfig.CheckRange(hero, target))
                {
                    // Check if this is a different target than current
                    Entity currentTarget = hero.Control.attackTarget;
                    
                    // Always attack if target is different, or if we don't have a current target
                    // This allows switching to better/closer targets dynamically
                    if (target != currentTarget)
                    {
                        // Only attack if target is in range - use doChase=false to not interfere with player movement
                        // This allows the hero to attack while moving without stopping/chasing
                        hero.Control.CmdAttack(target, false);
                    }
                }
            }
            // If target is out of range, don't attack (don't chase - let player control movement)
        }
        else
        {
            // No target found - clear attack target if we had one
            // This prevents sticking to dead/invalid targets
            if (hero.Control.attackTarget != null)
            {
                hero.Control.CmdAttack(null, false);
            }
        }
    }
    
    /// <summary>
    /// Find the nearest valid enemy target using game's built-in method
    /// </summary>
    public Entity FindNearestEnemy(Entity source, float maxRange)
    {
        if (source == null) return null;
        
        // Use the game's attack move target finding which handles all validation
        Entity target = ActionAttackMove.FindAttackMoveTarget(source, source.agentPosition);
        
        if (target != null)
        {
            // Verify it's within our max range
            float distance = Vector3.Distance(source.agentPosition, target.agentPosition);
            if (distance <= maxRange)
            {
                return target;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the nearest enemy for auto-aim (used by memory abilities)
    /// Returns the target position for aiming
    /// </summary>
    public Vector3? GetAutoAimTarget(Entity source, float maxRange)
    {
        if (!hasAutoAim || source == null) return null;
        
        Entity target = FindNearestEnemy(source, maxRange);
        if (target != null)
        {
            return target.agentPosition;
        }
        
        return null;
    }
    
    /// <summary>
    /// Get the nearest enemy entity for auto-aim
    /// </summary>
    public Entity GetAutoAimTargetEntity(Entity source, float maxRange)
    {
        if (!hasAutoAim || source == null) return null;
        return FindNearestEnemy(source, maxRange);
    }
}
