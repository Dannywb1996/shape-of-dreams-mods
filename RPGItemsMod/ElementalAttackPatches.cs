using System;
using UnityEngine;
using Mirror;

/// <summary>
/// Elemental weapon attack system
/// Uses EntityEvent_OnAttackFiredBeforePrepare to set elemental damage type on attacks
/// This makes the damage actually elemental (colored text, proper damage calculations)
/// </summary>
public static class ElementalAttackPatches
{
    private static EquipmentManager _equipmentManager;
    private static bool _isSubscribed = false;
    private static Action<EventInfoAttackFired> _attackHandler;
    private static Hero _subscribedHero = null; // Track which hero we're subscribed to
    
    public static void SetEquipmentManager(EquipmentManager manager)
    {
        _equipmentManager = manager;
    }
    
    /// <summary>
    /// Subscribe to the hero's attack events to add elemental damage
    /// Called when equipment changes or hero is available
    /// </summary>
    public static void SubscribeToHeroAttacks()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        if (!NetworkServer.active) return;
        
        Hero hero = DewPlayer.local.hero;
        
        // If already subscribed to THIS hero, don't re-subscribe
        if (_isSubscribed && _subscribedHero == hero) return;
        
        // If subscribed to a DIFFERENT hero (new run), unsubscribe from old first
        if (_isSubscribed && _subscribedHero != null && _subscribedHero != hero)
        {
            RPGLog.Debug(" Hero changed, re-subscribing to new hero...");
            ForceUnsubscribe();
        }
        
        // Create handler that adds elemental damage processor to attacks
        _attackHandler = new Action<EventInfoAttackFired>(OnAttackFired);
        hero.EntityEvent_OnAttackFiredBeforePrepare += _attackHandler;
        
        _subscribedHero = hero;
        _isSubscribed = true;
        RPGLog.Debug(" Subscribed to hero attack events for elemental weapons (Hero: " + hero.GetType().Name + ")");
    }
    
    /// <summary>
    /// Unsubscribe from hero attack events
    /// Called on cleanup or hero change
    /// </summary>
    public static void UnsubscribeFromHeroAttacks()
    {
        if (!_isSubscribed) return;
        
        // Try to unsubscribe from the stored hero reference
        if (_subscribedHero != null && _attackHandler != null)
        {
            try
            {
                _subscribedHero.EntityEvent_OnAttackFiredBeforePrepare -= _attackHandler;
                RPGLog.Debug(" Unsubscribed from hero attack events");
            }
            catch
            {
                // Hero might be destroyed, that's okay
                RPGLog.Debug(" Could not unsubscribe (hero may be destroyed)");
            }
        }
        
        _subscribedHero = null;
        _isSubscribed = false;
    }
    
    /// <summary>
    /// Force reset the subscription state (for when hero is destroyed/changed)
    /// </summary>
    public static void ForceUnsubscribe()
    {
        // Try to unsubscribe if possible
        if (_subscribedHero != null && _attackHandler != null)
        {
            try
            {
                _subscribedHero.EntityEvent_OnAttackFiredBeforePrepare -= _attackHandler;
            }
            catch { }
        }
        
        _subscribedHero = null;
        _isSubscribed = false;
        _attackHandler = null;
        RPGLog.Debug(" Force unsubscribed from hero attack events");
    }
    
    /// <summary>
    /// Handler for attack fired events
    /// Adds a damage processor that sets elemental type on the damage
    /// </summary>
    private static void OnAttackFired(EventInfoAttackFired obj)
    {
        if (_equipmentManager == null) return;
        
        ElementalType? elemType = _equipmentManager.GetActiveElementalType();
        if (!elemType.HasValue) return;
        
        ElementalType elementalType = elemType.Value;
        
        // Add damage processor to set elemental type on the damage data
        // Priority 70 matches Gem_E_Twilight
        obj.instance.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
        {
            data.SetElemental(elementalType);
        }, 70);
    }
}

