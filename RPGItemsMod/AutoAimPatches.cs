using System;
using UnityEngine;
using HarmonyLib;

/// <summary>
/// Auto Aim patches - Makes memory abilities auto-aim at nearest enemy when auto-aim gear is equipped
/// </summary>
public static class AutoAimPatches
{
    private static AutoTargetSystem _autoTargetSystem;
    
    public static void SetAutoTargetSystem(AutoTargetSystem system)
    {
        _autoTargetSystem = system;
    }
    
    /// <summary>
    /// Apply Harmony patches for auto-aim
    /// </summary>
    public static void ApplyPatches(Harmony harmony)
    {
        try
        {
            // Patch ControlManager.CastAbilityAtCursor to use auto-aim target
            var originalMethod = typeof(ControlManager).GetMethod("CastAbilityAtCursor", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (originalMethod != null)
            {
                var prefixMethod = typeof(AutoAimPatches).GetMethod("CastAbilityAtCursorPrefix",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                
                harmony.Patch(originalMethod, 
                    prefix: new HarmonyMethod(prefixMethod));
                
                RPGLog.Debug(" Auto-aim patches applied successfully");
            }
            else
            {
                RPGLog.Warning(" Could not find ControlManager.CastAbilityAtCursor method for auto-aim patch");
            }
        }
        catch (Exception ex)
        {
            RPGLog.Error(" Failed to apply auto-aim patches: " + ex.Message);
        }
    }
    
    /// <summary>
    /// Prefix patch for CastAbilityAtCursor - intercepts ability casting to use auto-aim
    /// </summary>
    private static bool CastAbilityAtCursorPrefix(ControlManager __instance, AbilityTrigger trigger, ref bool __result)
    {
        try
        {
            // Check if auto-aim is active
            if (_autoTargetSystem == null || !_autoTargetSystem.HasAutoAim)
            {
                return true; // Let original method run
            }
            
            // Only apply to memory abilities (SkillTrigger that are not attack abilities)
            if (!(trigger is SkillTrigger))
            {
                return true; // Let original method run
            }
            
            // Don't apply to attack abilities
            if (trigger == __instance.controllingEntity.Ability.attackAbility)
            {
                return true; // Let original method run
            }
            
            Entity controllingEntity = __instance.controllingEntity;
            if (controllingEntity == null || trigger.currentConfig == null) return true;
            
            // Don't apply auto-aim to dodge abilities
            // But allow auto-aim for attack memories that use movement
            Hero hero = controllingEntity as Hero;
            if (hero != null)
            {
                SkillTrigger skillTrigger = trigger as SkillTrigger;
                if (skillTrigger != null && trigger.currentConfig != null)
                {
                    HeroSkillLocation skillLocation;
                    if (hero.Skill.TryGetSkillLocation(skillTrigger, out skillLocation) && 
                        skillLocation == HeroSkillLocation.Movement)
                    {
                        // Check if this is a dodge ability by checking if the ability instance inherits from Ai_GenericDodge
                        // Dodge abilities (like Nimble Dodge, Flash Step, etc.) use Ai_GenericDodge
                        // Attack memories that use movement (like Charge, Endure) use different ability instance types
                        if (trigger.currentConfig.spawnedInstance != null)
                        {
                            System.Type abilityInstanceType = trigger.currentConfig.spawnedInstance.GetType();
                            System.Type genericDodgeType = typeof(Ai_GenericDodge);
                            
                            // Check if the ability instance type is Ai_GenericDodge or inherits from it
                            if (genericDodgeType.IsAssignableFrom(abilityInstanceType))
                            {
                                // This is a dodge ability - let original method run (use cursor position)
                                return true;
                            }
                        }
                        // Otherwise, it's an attack memory that uses movement - allow auto-aim
                    }
                }
            }
            
            // Get auto-aim target
            float maxRange = trigger.currentConfig.effectiveRange > 0 ? trigger.currentConfig.effectiveRange : 20f;
            Vector3? autoAimPos = _autoTargetSystem.GetAutoAimTarget(controllingEntity, maxRange);
            
            if (autoAimPos.HasValue)
            {
                CastInfo info;
                
                // Create CastInfo based on cast method type, using auto-aim position
                switch (trigger.currentConfig.castMethod.type)
                {
                    case CastMethodType.None:
                        info = new CastInfo(controllingEntity);
                        break;
                        
                    case CastMethodType.Point:
                        // For point-targeted abilities, use auto-aim position
                        info = new CastInfo(controllingEntity, autoAimPos.Value);
                        break;
                        
                    case CastMethodType.Cone:
                    case CastMethodType.Arrow:
                        // For directional abilities, calculate angle to auto-aim target
                        Vector3 direction = (autoAimPos.Value - controllingEntity.transform.position).normalized;
                        float angle = CastInfo.GetAngle(direction);
                        info = new CastInfo(controllingEntity, angle);
                        break;
                        
                    case CastMethodType.Target:
                        // For target abilities, find entity at auto-aim position
                        Entity targetEntity = _autoTargetSystem.GetAutoAimTargetEntity(controllingEntity, maxRange);
                        if (targetEntity != null && trigger.currentConfig.targetValidator.Evaluate(controllingEntity, targetEntity))
                        {
                            info = new CastInfo(controllingEntity, targetEntity);
                        }
                        else
                        {
                            // No valid target found - let original method run
                            return true;
                        }
                        break;
                        
                    default:
                        // Unsupported cast method - let original method run
                        return true;
                }
                
                // Cast the ability with auto-aim target
                // Use reflection to access private fields for shouldMoveToCast calculation
                var isDoingDirectionalMovementField = typeof(ControlManager).GetField("_isDoingDirectionalMovement", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var isContinuousMoveField = typeof(ControlManager).GetField("_isContinuousMove", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                bool isDoingDirectionalMovement = isDoingDirectionalMovementField != null ? (bool)isDoingDirectionalMovementField.GetValue(__instance) : false;
                bool isContinuousMove = isContinuousMoveField != null ? (bool)isContinuousMoveField.GetValue(__instance) : false;
                bool shouldMoveToCast = !isDoingDirectionalMovement && !isContinuousMove;
                
                __instance.CastAbility(trigger, info, shouldMoveToCast);
                __result = true;
                return false; // Skip original method
            }
            
            // No auto-aim target found - let original method run
            return true;
        }
        catch (Exception ex)
        {
            RPGLog.Error(" Error in CastAbilityAtCursorPrefix: " + ex.Message);
            return true; // Let original method run on error
        }
    }
}

