using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// Dropped Item - A world object that can be picked up by the player
/// Similar to how essences and memories work in the game
/// Press F to pick up, Hold G to dismantle (like game's memories/essences)
/// NOW NETWORKED: All players see dropped items and can interact!
/// Implements IInteractable to integrate with game's interact system!
/// </summary>
public class DroppedItem : MonoBehaviour, IInteractable
{
    // Track if being destroyed to prevent null reference errors
    private bool _isBeingDestroyed = false;
    
    // IInteractable implementation - return null if being destroyed to prevent CameraManager errors
    public Transform interactPivot { get { return _isBeingDestroyed ? null : transform; } }
    public bool canInteractWithMouse { get { return false; } }
    public float focusDistance { get { return _isBeingDestroyed ? 0f : 3.5f; } }
    public int priority { get { return 100; } } // Same as Gem
    
    public bool CanInteract(Entity entity)
    {
        if (_isBeingDestroyed) return false;
        if (item == null) return false;
        if (entity == null) return false;
        
        // Check if enough time has passed since spawn
        if (Time.time - spawnTime < pickupDelay) return false;
        
        // Check ownership (if not shared, only owner can interact)
        if (networkDropId > 0)
        {
            NetworkedItemSystem.NetworkedDropData netData = NetworkedItemSystem.GetDrop(networkDropId);
            if (netData != null && !netData.isShared)
            {
                // Only owner can interact with unshared items
                // NOTE: ownerNetId is the HERO's netId (from GetLocalPlayerNetId)
                Hero hero = entity as Hero;
                if (hero == null) return false;
                if (hero.netId != netData.ownerNetId) return false;
            }
        }
        
        return true;
    }
    
    public void OnInteract(Entity entity, bool alt)
    {
        if (item == null) return;
        
        if (alt)
        {
            // Alt interact = dismantle (hold G)
            HandleDismantleTapFromInteract();
        }
        else
        {
            // Normal interact = pickup (F)
            TryPickUp();
        }
    }
    
    private void HandleDismantleTapFromInteract()
    {
        // Prevent dismantling consumables, materials, and quest items
        if (item == null || 
            item.type == ItemType.Consumable || 
            item.type == ItemType.Material || 
            item.type == ItemType.Quest)
        {
            return;
        }
        
        float currentTime = Time.time;
        if (currentTime - _lastDismantleTapTime >= DismantleTapMinInterval)
        {
            _lastDismantleTapTime = currentTime;
            dismantleProgress += DismantleTapStrength;
            _isDismantling = true;
            
            // Play tap effect
            if (_fxDismantleTap != null)
            {
                DewEffect.PlayNew(_fxDismantleTap, transform.position, null, null);
            }
            
            // Check if complete
            if (dismantleProgress >= 1f)
            {
                CompleteDismantleFromInteract();
            }
        }
    }
    
    private void CompleteDismantleFromInteract()
    {
        // Safety check: prevent dismantling consumables, materials, and quest items
        if (item == null || 
            item.type == ItemType.Consumable || 
            item.type == ItemType.Material || 
            item.type == ItemType.Quest)
        {
            dismantleProgress = 0f;
            _isDismantling = false;
            return;
        }
        
        // Play dismantle effect
        if (_fxDismantle != null)
        {
            DewEffect.PlayNew(_fxDismantle, transform.position, null, null);
        }
        
        // Award dust via network system
        if (networkDropId > 0)
        {
            int dustValue = InventoryMerchantHelper.GetDismantleValue(item);
            NetworkedItemSystem.CompleteDismantle(networkDropId, dustValue);
        }
        else
        {
            // Local only - award dust via network-safe method
            int dustValue = InventoryMerchantHelper.GetDismantleValue(item);
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                if (NetworkServer.active)
                {
                    // HOST: Directly add dust
                    DewPlayer.local.AddDreamDust(dustValue);
                }
                else
                {
                    // CLIENT: Request host to give us dust
                    ChatManager chatManager = NetworkedManagerBase<ChatManager>.instance;
                    if (chatManager != null)
                    {
                        string content = string.Format("[RPGDUST]{0}|{1}", DewPlayer.local.hero.netId, dustValue);
                        chatManager.CmdSendChatMessage(content, null);
                    }
                }
            }
            RPGLog.Debug(" Dismantled " + item.name + " for " + dustValue + " dust via game interact");
            CleanupAndDestroy();
        }
    }
    
    public RPGItem item;
    public uint networkDropId = 0; // Networked drop ID (0 = local only)
    
    private static List<DroppedItem> allDroppedItems = new List<DroppedItem>();
    private static DroppedItem nearestItem = null;
    private static Dictionary<uint, DroppedItem> droppedItemsByNetId = new Dictionary<uint, DroppedItem>();
    
    // Visual components (world space)
    private GameObject visualObject;
    private GameObject fxLoopObject;
    private GameObject fxPrepareObject;
    private Light itemLight;
    private Light pulseLight;
    
    // Game's actual effects (loaded from Resources)
    private static GameObject _cachedWorldModelPrefab = null;
    private GameObject _fxDismantleTap = null;
    private GameObject _fxDismantle = null;
    
    // Screen space UI (like game's interact UI)
    private GameObject screenUI;
    private Canvas screenCanvas;
    private GameObject uiPanel;
    private Image panelBackground;
    private Image rarityBorder;
    private Text nameText;
    private Text descriptionText;
    private GameObject equipAction;
    private GameObject shareAction;
    private GameObject dismantleAction;
    private Text dismantleKeyText;
    private Text dustAmountText;
    private Image dismantleProgressFill;
    private float _fillCv;
    
    // Pickup settings
    private float pickupRange = 3.5f;
    private bool isInRange = false;
    
    // Animation
    private float bobSpeed = 1.5f;      // Slower, smoother bobbing
    private float bobHeight = 0.15f;    // Gentle float
    private float rotateSpeed = 0f;     // No rotation - just float
    private float startY;
    private float spawnTime;
    private float pickupDelay = 0.5f;
    
    // Glow effect
    private float glowIntensity = 1.5f;
    private Color glowColor;
    
    // Highlight when in range
    private bool isHighlighted = false;
    
    // Dismantle progress system (like memories/essences)
    private float dismantleProgress = 0f;
    private float _lastDismantleTapTime = 0f;
    private float _lastInteractAltUnscaledTime = 0f;
    private bool _isDismantling = false;
    
    // Constants (same as game)
    private const float DismantleTapMinInterval = 0.075f;
    private const float DismantleDecayStartTime = 1f;
    private const float DismantleTapStrength = 0.4f;
    private const float DismantleHoldRepeatTime = 0.3f;
    
    /// <summary>
    /// Create a dropped item using the NETWORKED system (multiplayer compatible)
    /// </summary>
    public static void CreateNetworked(RPGItem itemToDrop, Vector3 position)
    {
        if (itemToDrop == null) return;
        
        // Use networked system - this will broadcast to all players
        NetworkedItemSystem.DropItem(itemToDrop, position);
    }
    
    /// <summary>
    /// Create a local visual for a networked drop (called by NetworkedItemSystem)
    /// </summary>
    public static DroppedItem CreateFromNetwork(uint dropId, RPGItem itemData, Vector3 position)
    {
        if (itemData == null) return null;
        
        // Check if we already have this drop
        if (droppedItemsByNetId.ContainsKey(dropId))
        {
            RPGLog.Warning(" Drop " + dropId + " already exists locally");
            return droppedItemsByNetId[dropId];
        }
        
        GameObject obj = new GameObject("DroppedItem_Net_" + dropId + "_" + itemData.name);
        obj.transform.position = position + Vector3.up * 0.5f;
        
        DroppedItem dropped = obj.AddComponent<DroppedItem>();
        dropped.networkDropId = dropId;
        dropped.Initialize(itemData);
        
        allDroppedItems.Add(dropped);
        droppedItemsByNetId[dropId] = dropped;
        
        // Link to NetworkedItemSystem
        NetworkedItemSystem.NetworkedDropData netData = NetworkedItemSystem.GetDrop(dropId);
        if (netData != null)
        {
            netData.visualInstance = dropped;
        }
        
        return dropped;
    }
    
    /// <summary>
    /// Legacy: Create a LOCAL-ONLY dropped item (for offline/singleplayer)
    /// For multiplayer, use CreateNetworked() instead!
    /// </summary>
    public static DroppedItem Create(RPGItem itemToDrop, Vector3 position)
    {
        if (itemToDrop == null) return null;
        
        // In multiplayer, redirect to networked system
        if (Mirror.NetworkClient.active || Mirror.NetworkServer.active)
        {
            CreateNetworked(itemToDrop, position);
            return null; // Visual will be created by network callback
        }
        
        // Offline/singleplayer - create directly
        GameObject obj = new GameObject("DroppedItem_" + itemToDrop.name);
        obj.transform.position = position + Vector3.up * 0.5f;
        
        DroppedItem dropped = obj.AddComponent<DroppedItem>();
        dropped.Initialize(itemToDrop);
        
        allDroppedItems.Add(dropped);
        RPGLog.Debug(" Dropped item: " + itemToDrop.name + " at " + position);
        
        return dropped;
    }
    
    public static void DropItemAtPlayer(RPGItem item)
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Vector3 playerPos = DewPlayer.local.hero.position;
        Vector3 dropPos = playerPos + new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        );
        
        Create(item, dropPos);
    }
    
    private void Initialize(RPGItem itemData)
    {
        item = itemData;
        spawnTime = Time.time;
        startY = transform.position.y;
        glowColor = item.GetRarityColor();
        
        // Set layer to "Interactable" so game's interact system can detect us
        int interactableLayer = LayerMask.NameToLayer("Interactable");
        if (interactableLayer >= 0)
        {
            gameObject.layer = interactableLayer;
        }
        else
        {
            RPGLog.Warning(" Could not find Interactable layer");
        }
        
        CreateVisuals();
        CreateScreenUI();
        
        // Start spawn animation
        StartCoroutine(SpawnAnimation());
    }
    
    private IEnumerator SpawnAnimation()
    {
        // Initial position above
        Vector3 targetPos = transform.position;
        
        // If using game world model, do the game's animation style
        if (_gameWorldModelInstance != null)
        {
            // Play prepare effect (like game does)
            if (fxPrepareObject != null)
            {
                DewEffect.Play(fxPrepareObject);
            }
            
            // Wait for prepare duration (same as game's 0.9f)
            yield return new WaitForSeconds(0.9f);
            
            // Play appear effect
            GameObject fxAppear = null;
            ItemWorldModel wmScript = _cachedWorldModelPrefab.GetComponent<ItemWorldModel>();
            if (wmScript != null)
            {
                fxAppear = wmScript.fxAppear;
            }
            if (fxAppear != null)
            {
                DewEffect.PlayNew(fxAppear, transform.position, null, null);
            }
            
            // Bounce animation on icon quad (like GemWorldModel does)
            if (_iconQuadRenderer != null)
            {
                Transform iconTransform = _iconQuadRenderer.transform;
                Vector3 originalLocalPos = iconTransform.localPosition;
                iconTransform.localPosition = originalLocalPos + Vector3.up * 2f;
                
                float elapsed = 0f;
                float duration = 0.8f;
                Vector3 startLocalPos = iconTransform.localPosition;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    
                    // OutBounce easing (like DOTween.Ease.OutBounce)
                    float bounce;
                    if (t < 1f / 2.75f)
                    {
                        bounce = 7.5625f * t * t;
                    }
                    else if (t < 2f / 2.75f)
                    {
                        t -= 1.5f / 2.75f;
                        bounce = 7.5625f * t * t + 0.75f;
                    }
                    else if (t < 2.5f / 2.75f)
                    {
                        t -= 2.25f / 2.75f;
                        bounce = 7.5625f * t * t + 0.9375f;
                    }
                    else
                    {
                        t -= 2.625f / 2.75f;
                        bounce = 7.5625f * t * t + 0.984375f;
                    }
                    
                    iconTransform.localPosition = Vector3.Lerp(startLocalPos, originalLocalPos, bounce);
                    yield return null;
                }
                
                iconTransform.localPosition = originalLocalPos;
            }
            
            // Start loop effect
            if (fxLoopObject != null)
            {
                DewEffect.Play(fxLoopObject);
            }
            
            startY = targetPos.y;
        }
        else
        {
            // Fallback animation for custom visuals
            transform.position = targetPos + Vector3.up * 2f;
            
            if (fxPrepareObject != null)
            {
                fxPrepareObject.SetActive(true);
            }
            
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 startPos = transform.position;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                float bounce = 1f - Mathf.Pow(1f - t, 2f) * Mathf.Cos(t * Mathf.PI * 2f) * (1f - t);
                transform.position = Vector3.Lerp(startPos, targetPos, bounce);
                
                yield return null;
            }
            
            transform.position = targetPos;
            startY = targetPos.y;
            
            if (fxPrepareObject != null)
            {
                fxPrepareObject.SetActive(false);
            }
            if (fxLoopObject != null)
            {
                fxLoopObject.SetActive(true);
            }
        }
    }
    
    // Reference to instantiated game world model
    private GameObject _gameWorldModelInstance = null;
    private MeshRenderer _iconQuadRenderer = null;
    
    private void CreateVisuals()
    {
        // Try to use the game's actual world model (like memories/gems)
        if (TryCreateGameWorldModel())
        {
            // Log removed
            return;
        }
        
        // Fallback to custom visuals if game model fails
        // Log removed
        CreateFallbackVisuals();
    }
    
    /// <summary>
    /// Try to instantiate the game's actual world model (like gems/skills use)
    /// This properly clones the game's VFX including the beam effect!
    /// </summary>
    private bool TryCreateGameWorldModel()
    {
        try
        {
            // Load the Gem World Model prefab (has the beam effect we want)
            if (_cachedWorldModelPrefab == null)
            {
                _cachedWorldModelPrefab = Resources.Load<GameObject>("WorldModels/Gem World Model");
                if (_cachedWorldModelPrefab == null)
                {
                    _cachedWorldModelPrefab = Resources.Load<GameObject>("WorldModels/Skill World Model");
                }
            }
            
            if (_cachedWorldModelPrefab == null)
            {
                RPGLog.Warning(" Could not load game world model prefab");
                return false;
            }
            
            // Get references from the prefab
            ItemWorldModel prefabWorldModel = _cachedWorldModelPrefab.GetComponent<ItemWorldModel>();
            if (prefabWorldModel == null)
            {
                RPGLog.Warning(" Prefab has no ItemWorldModel");
                return false;
            }
            
            // Cache effect references from prefab for dismantle effects
            _fxDismantleTap = prefabWorldModel.fxDismantleTap;
            _fxDismantle = prefabWorldModel.fxDismantle;
            
            // Get the rarity color (use game's system for consistency)
            Color rarityColor = GetGameRarityColor(item.rarity);
            
            // Create container for our world model
            _gameWorldModelInstance = new GameObject("GameWorldModel");
            _gameWorldModelInstance.transform.SetParent(transform);
            _gameWorldModelInstance.transform.localPosition = Vector3.zero;
            _gameWorldModelInstance.transform.localRotation = Quaternion.identity;
            
            // CLONE THE GAME'S FX LOOP (this contains the beam/pillar effect!)
            if (prefabWorldModel.fxLoop != null)
            {
                fxLoopObject = Object.Instantiate(prefabWorldModel.fxLoop, _gameWorldModelInstance.transform);
                fxLoopObject.name = "Fx Loop (Cloned)";
                fxLoopObject.transform.localPosition = Vector3.zero;
                fxLoopObject.SetActive(false); // Will be activated after spawn animation
                
                // Tint the particles to match our rarity color
                TintParticleSystemsRecursively(fxLoopObject, rarityColor);
            }
            
            // CLONE THE GAME'S FX PREPARE (spawn preparation effect)
            if (prefabWorldModel.fxPrepare != null)
            {
                fxPrepareObject = Object.Instantiate(prefabWorldModel.fxPrepare, _gameWorldModelInstance.transform);
                fxPrepareObject.name = "Fx Prepare (Cloned)";
                fxPrepareObject.transform.localPosition = Vector3.zero;
                fxPrepareObject.SetActive(false);
                
                TintParticleSystemsRecursively(fxPrepareObject, rarityColor);
            }
            
            // Create animated rarity background quad (like inventory slots)
            GameObject rarityBgQuad = CreateRarityBackgroundQuad(rarityColor);
            if (rarityBgQuad != null)
            {
                rarityBgQuad.transform.SetParent(_gameWorldModelInstance.transform);
                rarityBgQuad.transform.localPosition = new Vector3(0, 0.7f, 0.02f); // Slightly behind icon
                rarityBgQuad.transform.localRotation = Quaternion.identity;
                rarityBgQuad.transform.localScale = new Vector3(2.5f, 2.5f, 1f); // Large background
            }
            
            // Create our OWN icon quad with our item's texture (original size)
            GameObject iconQuadObj = CreateCustomIconQuad(rarityColor);
            if (iconQuadObj != null)
            {
                iconQuadObj.transform.SetParent(_gameWorldModelInstance.transform);
                iconQuadObj.transform.localPosition = new Vector3(0, 0.7f, 0);
                iconQuadObj.transform.localRotation = Quaternion.identity;
                iconQuadObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // Original size
            }
            
            // Add a point light for extra glow
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.SetParent(_gameWorldModelInstance.transform);
            lightObj.transform.localPosition = new Vector3(0, 0.7f, -0.5f); // In front of icon
            Light glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = rarityColor;
            glowLight.intensity = 2.0f;  // Brighter
            glowLight.range = 3f;        // Larger range
            
            // Add collider for interaction
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.radius = pickupRange;
            col.isTrigger = true;
            
            // Add ping handler for middle-click sharing (on 3D object, not UI)
            DroppedItemPingHandler pingHandler = gameObject.AddComponent<DroppedItemPingHandler>();
            pingHandler.droppedItem = this;
            
            visualObject = _gameWorldModelInstance;
            
            return true;
        }
        catch (System.Exception e)
        {
            RPGLog.Warning(" Failed to create game world model: " + e.Message + "\n" + e.StackTrace);
            return false;
        }
    }
    
    /// <summary>
    /// Tint all particle systems in a hierarchy to match our rarity color
    /// </summary>
    private void TintParticleSystemsRecursively(GameObject obj, Color color)
    {
        if (obj == null) return;
        
        // Tint particle systems
        ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particles)
        {
            var main = ps.main;
            // Preserve alpha, change color
            Color newColor = color;
            newColor.a = main.startColor.color.a;
            main.startColor = newColor;
        }
        
        // Tint renderers (for mesh-based effects)
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r.material != null)
            {
                // Try to tint the material
                if (r.material.HasProperty("_Color"))
                {
                    Color matColor = r.material.color;
                    matColor.r = color.r;
                    matColor.g = color.g;
                    matColor.b = color.b;
                    r.material.color = matColor;
                }
                if (r.material.HasProperty("_TintColor"))
                {
                    r.material.SetColor("_TintColor", color);
                }
                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.SetColor("_EmissionColor", color * 2f);
                }
            }
        }
        
        // Tint lights
        Light[] lights = obj.GetComponentsInChildren<Light>(true);
        foreach (Light l in lights)
        {
            l.color = color;
        }
    }
    
    /// <summary>
    /// Create a custom quad with our item's texture
    /// </summary>
    private GameObject CreateCustomIconQuad(Color rarityColor)
    {
        try
        {
            // Create a quad primitive
            GameObject quadObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadObj.name = "CustomIconQuad";
            
            // Remove the collider (we have our own)
            Collider col = quadObj.GetComponent<Collider>();
            if (col != null) Destroy(col);
            
            // Get the renderer
            MeshRenderer renderer = quadObj.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                RPGLog.Error(" Quad has no MeshRenderer!");
                Destroy(quadObj);
                return null;
            }
            
            // Create a simple unlit material
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (shader == null) shader = Shader.Find("UI/Default");
            if (shader == null) shader = Shader.Find("Standard");
            
            Material mat = new Material(shader);
            
            // Lazy load sprite if needed
            ItemDatabase.EnsureSprite(item);
            
            // Set the texture
            if (item.sprite != null && item.sprite.texture != null)
            {
                mat.mainTexture = item.sprite.texture;
                mat.color = Color.white;
            }
            else
            {
                // Create a simple colored texture
                Texture2D tex = new Texture2D(32, 32);
                Color[] colors = new Color[32 * 32];
                for (int i = 0; i < colors.Length; i++) colors[i] = rarityColor;
                tex.SetPixels(colors);
                tex.Apply();
                mat.mainTexture = tex;
                mat.color = Color.white;
                // Log removed
            }
            
            renderer.material = mat;
            _iconQuadRenderer = renderer;
            
            return quadObj;
        }
        catch (System.Exception e)
        {
            RPGLog.Error(" Error creating custom icon quad: " + e.Message);
            return null;
        }
    }
    
    private MeshRenderer _rarityBgRenderer = null;
    
    /// <summary>
    /// Create animated rarity background quad (like inventory slots use)
    /// This creates a 3D quad with the game's animated matBackdrop material
    /// </summary>
    private GameObject CreateRarityBackgroundQuad(Color rarityColor)
    {
        try
        {
            // Try to get the game's rarity material from RarityEffectHelper
            RarityEffectHelper.TryGetRarityMaterial();
            
            // Create a quad primitive
            GameObject bgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bgQuad.name = "RarityBackgroundQuad";
            
            // Remove the collider
            Collider col = bgQuad.GetComponent<Collider>();
            if (col != null) Destroy(col);
            
            // Get the renderer
            MeshRenderer renderer = bgQuad.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                Destroy(bgQuad);
                return null;
            }
            
            // Try to use the game's animated material
            Material rarityMat = null;
            
            if (RarityEffectHelper.SharedMaterial != null)
            {
                // Clone the game's animated material
                rarityMat = new Material(RarityEffectHelper.SharedMaterial);
                rarityMat.name = "RarityBg_" + item.rarity;
                
                // Set the color to match rarity
                if (rarityMat.HasProperty("_Color"))
                {
                    rarityMat.SetColor("_Color", rarityColor);
                }
                if (rarityMat.HasProperty("_TintColor"))
                {
                    rarityMat.SetColor("_TintColor", rarityColor);
                }
                
                // Try to set the sprite texture if available
                if (RarityEffectHelper.SharedSprite != null && RarityEffectHelper.SharedSprite.texture != null)
                {
                    rarityMat.mainTexture = RarityEffectHelper.SharedSprite.texture;
                }
                
            }
            else
            {
                // Fallback: Create a simple glowing material
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Unlit/Transparent");
                
                rarityMat = new Material(shader);
                
                // Create a radial gradient texture for the glow
                Texture2D glowTex = CreateRadialGlowTexture(64, rarityColor);
                rarityMat.mainTexture = glowTex;
                rarityMat.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.7f);
                
            }
            
            renderer.material = rarityMat;
            _rarityBgRenderer = renderer;
            
            return bgQuad;
        }
        catch (System.Exception e)
        {
            RPGLog.Warning(" Error creating rarity background quad: " + e.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Create a radial gradient glow texture for fallback rarity background
    /// </summary>
    private Texture2D CreateRadialGlowTexture(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        float center = size / 2f;
        float maxDist = center;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = 1f - Mathf.Clamp01(dist / maxDist);
                // Smooth falloff
                alpha = alpha * alpha;
                
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha * 0.8f));
            }
        }
        
        tex.Apply();
        return tex;
    }
    
    /// <summary>
    /// Get the game's rarity color to match memories/essences
    /// Game rarities: Common(0), Rare(1), Epic(2), Legendary(3)
    /// </summary>
    private Color GetGameRarityColor(ItemRarity rarity)
    {
        // Use the game's Dew.GetRarityColor if available
        try
        {
            Rarity gameRarity;
            switch (rarity)
            {
                case ItemRarity.Common:
                    gameRarity = Rarity.Common;
                    break;
                case ItemRarity.Rare:
                    // Our Rare maps to game's Rare
                    gameRarity = Rarity.Rare;
                    break;
                case ItemRarity.Epic:
                    gameRarity = Rarity.Epic;
                    break;
                case ItemRarity.Legendary:
                    gameRarity = Rarity.Legendary;
                    break;
                default:
                    gameRarity = Rarity.Common;
                    break;
            }
            return Dew.GetRarityColor(gameRarity);
        }
        catch
        {
            // Fallback to our own colors
            return item.GetRarityColor();
        }
    }
    
    /// <summary>
    /// Create a vertical beam of light effect
    /// </summary>
    private void CreateLightBeam(Transform parent, Color beamColor)
    {
        // Create a tall thin quad for the beam
        GameObject beamObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        beamObj.name = "LightBeam";
        beamObj.transform.SetParent(parent);
        beamObj.transform.localPosition = new Vector3(0, 2f, 0); // Center of beam at height 2
        beamObj.transform.localRotation = Quaternion.identity;
        beamObj.transform.localScale = new Vector3(0.3f, 4f, 1f); // Thin and tall
        
        // Remove collider
        Collider col = beamObj.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        // Create beam texture (vertical gradient, bright in center)
        Texture2D beamTex = CreateBeamTexture(32, 128, beamColor);
        
        // Set up material
        MeshRenderer renderer = beamObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            
            Material mat = new Material(shader);
            mat.mainTexture = beamTex;
            mat.color = new Color(1f, 1f, 1f, 0.6f); // Semi-transparent
            renderer.material = mat;
        }
        
        // Add a BillboardBeam component to make it always face camera
        beamObj.AddComponent<BillboardBeam>();
    }
    
    /// <summary>
    /// Create a texture for the light beam (vertical gradient)
    /// </summary>
    private Texture2D CreateBeamTexture(int width, int height, Color beamColor)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            // Vertical gradient - bright in middle, fading at top and bottom
            float verticalT = (float)y / height;
            float verticalAlpha;
            if (verticalT < 0.5f)
            {
                // Bottom half - fade in
                verticalAlpha = verticalT * 2f;
            }
            else
            {
                // Top half - fade out
                verticalAlpha = (1f - verticalT) * 2f;
            }
            verticalAlpha = Mathf.Pow(verticalAlpha, 0.5f); // Softer falloff
            
            for (int x = 0; x < width; x++)
            {
                // Horizontal gradient - bright in center, fading at edges
                float horizontalT = (float)x / width;
                float centerDist = Mathf.Abs(horizontalT - 0.5f) * 2f; // 0 at center, 1 at edges
                float horizontalAlpha = 1f - Mathf.Pow(centerDist, 2f); // Quadratic falloff
                
                float finalAlpha = verticalAlpha * horizontalAlpha;
                pixels[y * width + x] = new Color(beamColor.r, beamColor.g, beamColor.b, finalAlpha);
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
    
    /// <summary>
    /// Create a texture that looks like dust particles
    /// </summary>
    private Texture2D CreateDustTexture(int size, Color dustColor)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);
        
        // Fill with transparent
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = transparent;
        
        // Create dust particle pattern - multiple small circles/dots
        System.Random rand = new System.Random(42); // Fixed seed for consistency
        
        // Draw several dust particles at different positions
        int numParticles = 5;
        float[] particleX = { 0.3f, 0.7f, 0.5f, 0.25f, 0.75f };
        float[] particleY = { 0.3f, 0.35f, 0.65f, 0.6f, 0.55f };
        float[] particleSize = { 0.25f, 0.2f, 0.3f, 0.15f, 0.18f };
        
        for (int p = 0; p < numParticles; p++)
        {
            int centerX = (int)(particleX[p] * size);
            int centerY = (int)(particleY[p] * size);
            int radius = (int)(particleSize[p] * size / 2);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist <= radius)
                    {
                        // Soft edge
                        float alpha = 1f - (dist / radius);
                        alpha = alpha * alpha; // Quadratic falloff for softer look
                        
                        int idx = y * size + x;
                        Color existing = pixels[idx];
                        // Blend with existing
                        float newAlpha = Mathf.Clamp01(existing.a + alpha * dustColor.a);
                        pixels[idx] = new Color(dustColor.r, dustColor.g, dustColor.b, newAlpha);
                    }
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
    
    /// <summary>
    /// Fallback custom visuals if game model fails
    /// </summary>
    private void CreateFallbackVisuals()
    {
        // Create main visual container
        visualObject = new GameObject("ItemVisual");
        visualObject.transform.SetParent(transform);
        visualObject.transform.localPosition = Vector3.zero;
        
        // Create the icon quad
        GameObject iconQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        iconQuad.name = "IconQuad";
        iconQuad.transform.SetParent(visualObject.transform);
        iconQuad.transform.localPosition = Vector3.zero;
        iconQuad.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // Larger icon for better visibility
        
        // Remove collider from visual
        Collider visualCol = iconQuad.GetComponent<Collider>();
        if (visualCol != null) Destroy(visualCol);
        
        // Set up material
        MeshRenderer meshRenderer = iconQuad.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
            }
            
            // Lazy load sprite if needed
            ItemDatabase.EnsureSprite(item);
            
            if (item.sprite != null)
            {
                mat.mainTexture = item.sprite.texture;
                mat.color = Color.white;
            }
            else
            {
                mat.color = glowColor;
            }
            
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * 0.8f);
            
            meshRenderer.material = mat;
        }
        
        // Create glow ring effect
        CreateGlowRing();
        
        // Create particle effects
        CreateParticleEffects();
        
        // Create lights
        CreateLights();
        
        // Add collider for interaction
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.radius = pickupRange;
        col.isTrigger = true;
        
        // Add ping handler for middle-click sharing (on 3D object, not UI)
        DroppedItemPingHandler pingHandler = gameObject.AddComponent<DroppedItemPingHandler>();
        pingHandler.droppedItem = this;
    }
    
    private void CreateGlowRing()
    {
        // Create a ring/circle on the ground
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "GlowRing";
        ring.transform.SetParent(transform);
        ring.transform.localPosition = new Vector3(0, -0.4f, 0);
        ring.transform.localScale = new Vector3(1.2f, 0.02f, 1.2f);
        
        Collider ringCol = ring.GetComponent<Collider>();
        if (ringCol != null) Destroy(ringCol);
        
        MeshRenderer ringRenderer = ring.GetComponent<MeshRenderer>();
        if (ringRenderer != null)
        {
            Material ringMat = new Material(Shader.Find("Sprites/Default"));
            ringMat.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
            ringMat.EnableKeyword("_EMISSION");
            ringMat.SetColor("_EmissionColor", glowColor * 0.5f);
            ringRenderer.material = ringMat;
        }
    }
    
    private void CreateParticleEffects()
    {
        // Skip particle system creation - Unity particles need proper shaders
        // Instead, we'll use the glow ring and light effects for visual feedback
        // This avoids the pink/magenta shader error
        
        // Create prepare effect placeholder (for spawn animation)
        fxPrepareObject = new GameObject("FX_Prepare");
        fxPrepareObject.transform.SetParent(transform);
        fxPrepareObject.transform.localPosition = Vector3.up * 2f;
        fxPrepareObject.SetActive(false);
        
        // Create loop effect placeholder
        fxLoopObject = new GameObject("FX_Loop");
        fxLoopObject.transform.SetParent(transform);
        fxLoopObject.transform.localPosition = Vector3.zero;
        fxLoopObject.SetActive(true);
    }
    
    private void CreateLights()
    {
        // Main point light
        GameObject lightObj = new GameObject("ItemLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        itemLight = lightObj.AddComponent<Light>();
        itemLight.type = LightType.Point;
        itemLight.color = glowColor;
        itemLight.intensity = glowIntensity;
        itemLight.range = 3f;
        
        // Pulse light (for effects)
        GameObject pulseLightObj = new GameObject("PulseLight");
        pulseLightObj.transform.SetParent(transform);
        pulseLightObj.transform.localPosition = Vector3.zero;
        pulseLight = pulseLightObj.AddComponent<Light>();
        pulseLight.type = LightType.Point;
        pulseLight.color = glowColor;
        pulseLight.intensity = 0f;
        pulseLight.range = 5f;
    }
    
    private void CreateScreenUI()
    {
        // Create screen space UI (like game's interact tooltip)
        screenUI = new GameObject("DroppedItem_ScreenUI");
        DontDestroyOnLoad(screenUI);
        
        screenCanvas = screenUI.AddComponent<Canvas>();
        screenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        screenCanvas.sortingOrder = -1; // Minimum, below everything
        
        screenUI.AddComponent<CanvasScaler>();
        // No GraphicRaycaster - we handle middle-click manually in Update() to avoid blocking attacks
        
        // Main panel with dark background
        uiPanel = new GameObject("Panel");
        uiPanel.transform.SetParent(screenCanvas.transform, false);
        
        RectTransform panelRect = uiPanel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(360, 140); // Larger horizontally to prevent overflow
        
        // Dark background - raycastTarget false so clicks pass through for attacks
        panelBackground = uiPanel.AddComponent<Image>();
        panelBackground.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        panelBackground.raycastTarget = false;
        
        // Rarity colored border (left side accent)
        GameObject borderObj = new GameObject("RarityBorder");
        borderObj.transform.SetParent(uiPanel.transform, false);
        rarityBorder = borderObj.AddComponent<Image>();
        rarityBorder.color = glowColor;
        rarityBorder.raycastTarget = false;
        
        RectTransform borderRect = rarityBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0, 0);
        borderRect.anchorMax = new Vector2(0, 1);
        borderRect.pivot = new Vector2(0, 0.5f);
        borderRect.sizeDelta = new Vector2(4, 0);
        borderRect.anchoredPosition = Vector2.zero;
        
        // Top border line
        GameObject topBorderObj = new GameObject("TopBorder");
        topBorderObj.transform.SetParent(uiPanel.transform, false);
        Image topBorder = topBorderObj.AddComponent<Image>();
        topBorder.color = glowColor;
        topBorder.raycastTarget = false;
        
        RectTransform topBorderRect = topBorder.GetComponent<RectTransform>();
        topBorderRect.anchorMin = new Vector2(0, 1);
        topBorderRect.anchorMax = new Vector2(1, 1);
        topBorderRect.pivot = new Vector2(0.5f, 1);
        topBorderRect.sizeDelta = new Vector2(0, 2);
        topBorderRect.anchoredPosition = Vector2.zero;
        
        // Item name with rarity color (include stack count for stackable items)
        GameObject nameObj = new GameObject("ItemName");
        nameObj.transform.SetParent(uiPanel.transform, false);
        nameText = nameObj.AddComponent<Text>();
        // Show stack count if item is stackable (maxStack > 1 or unlimited stacks for consumables)
        bool isStackable = item.maxStack > 1 || item.maxStack < 0 || item.type == ItemType.Consumable;
        string displayName = item.GetDisplayName();
        if (isStackable && item.currentStack > 1)
        {
            nameText.text = displayName + " x" + item.currentStack;
        }
        else if (isStackable)
        {
            nameText.text = displayName + " x1";
        }
        else
        {
            nameText.text = displayName;
        }
        nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color = glowColor;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.raycastTarget = false;
        
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.7f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = new Vector2(12, 0);
        nameRect.offsetMax = new Vector2(-8, -8);
        
        // Description text
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(uiPanel.transform, false);
        descriptionText = descObj.AddComponent<Text>();
        descriptionText.text = GetItemDescription();
        descriptionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        descriptionText.fontSize = 11;
        descriptionText.color = new Color(0.85f, 0.85f, 0.85f, 1f); // Lighter for better contrast with colors
        descriptionText.alignment = TextAnchor.UpperLeft;
        descriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        descriptionText.verticalOverflow = VerticalWrapMode.Truncate;
        descriptionText.supportRichText = true; // Enable rich text for colored stats
        descriptionText.raycastTarget = false;
        
        RectTransform descRect = descriptionText.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0.35f);
        descRect.anchorMax = new Vector2(1, 0.7f);
        descRect.offsetMin = new Vector2(12, 0);
        descRect.offsetMax = new Vector2(-12, 0);
        
        // Actions container
        GameObject actionsObj = new GameObject("Actions");
        actionsObj.transform.SetParent(uiPanel.transform, false);
        RectTransform actionsRect = actionsObj.AddComponent<RectTransform>();
        actionsRect.anchorMin = new Vector2(0, 0);
        actionsRect.anchorMax = new Vector2(1, 0.35f);
        actionsRect.offsetMin = new Vector2(8, 6);
        actionsRect.offsetMax = new Vector2(-8, -4);
        
        HorizontalLayoutGroup actionsLayout = actionsObj.AddComponent<HorizontalLayoutGroup>();
        actionsLayout.spacing = 15f;
        actionsLayout.childAlignment = TextAnchor.MiddleLeft;
        actionsLayout.childControlWidth = false;
        actionsLayout.childControlHeight = false;
        actionsLayout.childForceExpandWidth = false;
        actionsLayout.childForceExpandHeight = false;
        
        // Equip action [F]
        equipAction = CreateActionButton(actionsObj.transform, "F", Localization.Pickup, Color.white);
        
        // Share action [MMB] - only show if owner and not already shared
        shareAction = CreateActionButton(actionsObj.transform, Localization.MMB, Localization.Share, new Color(0.57f, 0.98f, 1f)); // Cyan like game
        
        // Dismantle action [G] with dust amount
        dismantleAction = CreateDismantleButton(actionsObj.transform);
        
        // Dismantle progress bar (at bottom)
        CreateDismantleProgressBar();
        
        screenUI.SetActive(false);
    }
    
    private GameObject CreateActionButton(Transform parent, string key, string action, Color keyColor)
    {
        GameObject actionObj = new GameObject("Action_" + action);
        actionObj.transform.SetParent(parent, false);
        
        RectTransform actionRect = actionObj.AddComponent<RectTransform>();
        actionRect.sizeDelta = new Vector2(70, 24);
        
        HorizontalLayoutGroup layout = actionObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        
        // Key box [F]
        GameObject keyBoxObj = new GameObject("KeyBox");
        keyBoxObj.transform.SetParent(actionObj.transform, false);
        
        Image keyBoxBg = keyBoxObj.AddComponent<Image>();
        keyBoxBg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        keyBoxBg.raycastTarget = false;
        
        RectTransform keyBoxRect = keyBoxBg.GetComponent<RectTransform>();
        keyBoxRect.sizeDelta = new Vector2(22, 22);
        
        GameObject keyTextObj = new GameObject("KeyText");
        keyTextObj.transform.SetParent(keyBoxObj.transform, false);
        Text keyText = keyTextObj.AddComponent<Text>();
        keyText.text = key;
        keyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        keyText.fontSize = 14;
        keyText.fontStyle = FontStyle.Bold;
        keyText.color = keyColor;
        keyText.alignment = TextAnchor.MiddleCenter;
        keyText.raycastTarget = false;
        
        RectTransform keyTextRect = keyText.GetComponent<RectTransform>();
        keyTextRect.anchorMin = Vector2.zero;
        keyTextRect.anchorMax = Vector2.one;
        keyTextRect.sizeDelta = Vector2.zero;
        
        // Action text
        GameObject actionTextObj = new GameObject("ActionText");
        actionTextObj.transform.SetParent(actionObj.transform, false);
        Text actionText = actionTextObj.AddComponent<Text>();
        actionText.text = action;
        actionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        actionText.fontSize = 13;
        actionText.color = Color.white;
        actionText.alignment = TextAnchor.MiddleLeft;
        actionText.raycastTarget = false;
        
        RectTransform actionTextRect = actionText.GetComponent<RectTransform>();
        actionTextRect.sizeDelta = new Vector2(50, 22);
        
        return actionObj;
    }
    
    private GameObject CreateDismantleButton(Transform parent)
    {
        GameObject actionObj = new GameObject("Action_Dismantle");
        actionObj.transform.SetParent(parent, false);
        
        RectTransform actionRect = actionObj.AddComponent<RectTransform>();
        actionRect.sizeDelta = new Vector2(130, 24);
        
        HorizontalLayoutGroup layout = actionObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        
        // Key box [G]
        GameObject keyBoxObj = new GameObject("KeyBox");
        keyBoxObj.transform.SetParent(actionObj.transform, false);
        
        Image keyBoxBg = keyBoxObj.AddComponent<Image>();
        keyBoxBg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        keyBoxBg.raycastTarget = false;
        
        RectTransform keyBoxRect = keyBoxBg.GetComponent<RectTransform>();
        keyBoxRect.sizeDelta = new Vector2(22, 22);
        
        GameObject keyTextObj = new GameObject("KeyText");
        keyTextObj.transform.SetParent(keyBoxObj.transform, false);
        dismantleKeyText = keyTextObj.AddComponent<Text>();
        // Get the actual keybind from player's control settings
        dismantleKeyText.text = GetDismantleKeyText();
        dismantleKeyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        dismantleKeyText.fontSize = 14;
        dismantleKeyText.fontStyle = FontStyle.Bold;
        dismantleKeyText.color = Color.white;
        dismantleKeyText.alignment = TextAnchor.MiddleCenter;
        dismantleKeyText.raycastTarget = false;
        
        RectTransform keyTextRect = dismantleKeyText.GetComponent<RectTransform>();
        keyTextRect.anchorMin = Vector2.zero;
        keyTextRect.anchorMax = Vector2.one;
        keyTextRect.sizeDelta = Vector2.zero;
        
        // Action text "Dismantle"
        GameObject actionTextObj = new GameObject("ActionText");
        actionTextObj.transform.SetParent(actionObj.transform, false);
        Text actionText = actionTextObj.AddComponent<Text>();
        actionText.text = Localization.Dismantle;
        actionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        actionText.fontSize = 13;
        actionText.color = Color.white;
        actionText.alignment = TextAnchor.MiddleLeft;
        actionText.raycastTarget = false;
        
        RectTransform actionTextRect = actionText.GetComponent<RectTransform>();
        actionTextRect.sizeDelta = new Vector2(60, 22);
        
        // Dream dust color - cyan/blue like the game uses
        Color dreamDustColor = new Color(0.4f, 0.85f, 1f, 1f);
        
        // Create a dust icon using a generated texture that looks like dust particles
        GameObject dustIconObj = new GameObject("DustIcon");
        dustIconObj.transform.SetParent(actionObj.transform, false);
        Image dustIcon = dustIconObj.AddComponent<Image>();
        
        // Create a small dust particle texture
        Texture2D dustTex = CreateDustTexture(16, dreamDustColor);
        dustIcon.sprite = Sprite.Create(dustTex, new Rect(0, 0, dustTex.width, dustTex.height), new Vector2(0.5f, 0.5f));
        dustIcon.color = Color.white;
        dustIcon.raycastTarget = false;
        
        RectTransform dustIconRect = dustIcon.GetComponent<RectTransform>();
        dustIconRect.sizeDelta = new Vector2(16, 16);
        
        // Dust amount text (blue like dream dust)
        GameObject dustTextObj = new GameObject("DustAmount");
        dustTextObj.transform.SetParent(actionObj.transform, false);
        dustAmountText = dustTextObj.AddComponent<Text>();
        dustAmountText.text = "+" + GetDismantleAmount();
        dustAmountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        dustAmountText.fontSize = 14;
        dustAmountText.fontStyle = FontStyle.Bold;
        dustAmountText.color = dreamDustColor; // Blue like dream dust
        dustAmountText.alignment = TextAnchor.MiddleLeft;
        dustAmountText.raycastTarget = false;
        
        RectTransform dustTextRect = dustAmountText.GetComponent<RectTransform>();
        dustTextRect.sizeDelta = new Vector2(40, 22);
        
        return actionObj;
    }
    
    private void CreateDismantleProgressBar()
    {
        // Progress bar at bottom of panel
        GameObject progressObj = new GameObject("DismantleProgress");
        progressObj.transform.SetParent(uiPanel.transform, false);
        
        Image progressBg = progressObj.AddComponent<Image>();
        progressBg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        progressBg.raycastTarget = false;
        
        RectTransform progressRect = progressBg.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0, 0);
        progressRect.anchorMax = new Vector2(1, 0);
        progressRect.pivot = new Vector2(0.5f, 0);
        progressRect.sizeDelta = new Vector2(0, 4);
        progressRect.anchoredPosition = Vector2.zero;
        
        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(progressObj.transform, false);
        dismantleProgressFill = fillObj.AddComponent<Image>();
        dismantleProgressFill.color = new Color(1f, 0.4f, 0.4f, 1f); // Red
        dismantleProgressFill.raycastTarget = false;
        
        RectTransform fillRect = dismantleProgressFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
    }
    
    /// <summary>
    /// Get the keybind text for dismantling (reads from player's control settings)
    /// </summary>
    private string GetDismantleKeyText()
    {
        try
        {
            // Use the game's DewInput to get the readable text for the interactAlt binding
            // This respects the player's custom keybinds
            if (DewSave.profileMain != null && DewSave.profileMain.controls != null)
            {
                return DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.interactAlt);
            }
        }
        catch { }
        
        // Fallback to default "G" if we can't read the setting
        return "G";
    }
    
    private string GetItemDescription()
    {
        // Use localized rarity and type names
        string rarityName = Localization.GetRarityName(item.rarity);
        string desc = rarityName + " " + item.GetTypeName();
        
        // Add short stats
        string stats = item.GetShortStatsString();
        if (!string.IsNullOrEmpty(stats))
        {
            desc += ", " + stats;
        }
        
        // Add localized item description if available
        string localizedDesc = item.GetLocalizedDescription();
        if (!string.IsNullOrEmpty(localizedDesc))
        {
            desc += "\n" + localizedDesc;
        }
        
        return desc;
    }
    
    private void Update()
    {
        if (item == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Bob up and down
        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Rotate visual
        if (visualObject != null)
        {
            visualObject.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
        
        // Check distance to player AND mouse hover
        if (Time.time - spawnTime > pickupDelay)
        {
            CheckPlayerDistance();
            CheckMouseHover();
        }
        
        // Pulse light effect
        if (itemLight != null)
        {
            float baseIntensity = isInRange ? glowIntensity * 1.5f : glowIntensity;
            itemLight.intensity = baseIntensity + Mathf.Sin(Time.time * 3f) * 0.5f;
        }
        
        // Update screen UI position
        UpdateScreenUIPosition();
        
        // Handle pickup key press - use game's interact binding
        bool interactDown = DewInput.GetButtonDown(DewSave.profileMain.controls.interact, checkGameAreaForMouse: true);
        if (interactDown)
        {
            if (isInRange && nearestItem == this)
            {
                TryPickUp();
            }
        }
        
        // Share is now handled via IPingableCustom.OnPing() to prevent ground pings
        // The game's ping system will call HandlePingShare() when middle-clicking on our UI
        
        // Handle dismantle (hold G like game)
        HandleDismantleInput();
        
        // Update dismantle progress
        UpdateDismantleProgress();
    }
    
    private bool isMouseHovering = false;
    
    private void CheckMouseHover()
    {
        // Check if mouse is hovering over this item
        if (Camera.main == null) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 mousePos = Input.mousePosition;
        
        // Simple screen distance check - hover works at ANY distance (like game's memories)
        float screenDist = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), new Vector2(mousePos.x, mousePos.y));
        isMouseHovering = screenPos.z > 0 && screenDist < 60f; // 60 pixels radius on screen
        
        // If mouse hovering (regardless of range), this becomes the focused item for UI display
        // But interaction (pickup/dismantle) still requires proximity
        if (isMouseHovering)
        {
            if (nearestItem != this)
            {
                if (nearestItem != null)
                {
                    nearestItem.SetHighlighted(false);
                }
                nearestItem = this;
                SetHighlighted(true);
            }
        }
    }
    
    private void UpdateScreenUIPosition()
    {
        if (screenUI == null || uiPanel == null) return;
        
        // Show UI when:
        // 1. Mouse hovering (works at ANY distance, like game's memories - for info display)
        // 2. OR in range and this is the nearest item (proximity-based when not hovering)
        bool shouldShow = isMouseHovering || (isInRange && nearestItem == this);
        screenUI.SetActive(shouldShow);
        
        if (!shouldShow) return;
        
        // Update action buttons based on whether player is in range
        // Can only interact when in proximity
        if (equipAction != null)
        {
            // Dim the action if not in range
            CanvasGroup equipCG = equipAction.GetComponent<CanvasGroup>();
            if (equipCG == null) equipCG = equipAction.AddComponent<CanvasGroup>();
            equipCG.alpha = isInRange ? 1f : 0.4f;
        }
        
        // Share button - only show if we own the item and it's not already shared
        if (shareAction != null && networkDropId != 0)
        {
            NetworkedItemSystem.NetworkedDropData dropData = NetworkedItemSystem.GetDrop(networkDropId);
            bool canShare = dropData != null && 
                           !dropData.isShared && 
                           dropData.ownerNetId == NetworkedItemSystem.GetLocalPlayerNetId();
            shareAction.SetActive(canShare);
            
            if (canShare)
            {
                CanvasGroup shareCG = shareAction.GetComponent<CanvasGroup>();
                if (shareCG == null) shareCG = shareAction.AddComponent<CanvasGroup>();
                shareCG.alpha = isInRange ? 1f : 0.4f;
            }
        }
        else if (shareAction != null)
        {
            // No network ID = single player, hide share button
            shareAction.SetActive(false);
        }
        
        // Dismantle button - hide for consumables, materials, and quest items (same as inventory)
        bool canDismantle = item != null && 
                           item.type != ItemType.Consumable && 
                           item.type != ItemType.Material && 
                           item.type != ItemType.Quest;
        
        if (dismantleAction != null)
        {
            dismantleAction.SetActive(canDismantle);
            
            if (canDismantle)
            {
                CanvasGroup dismantleCG = dismantleAction.GetComponent<CanvasGroup>();
                if (dismantleCG == null) dismantleCG = dismantleAction.AddComponent<CanvasGroup>();
                dismantleCG.alpha = isInRange ? 1f : 0.4f;
            }
        }
        
        // Hide progress bar for consumables, materials, and quest items
        if (dismantleProgressFill != null && dismantleProgressFill.transform.parent != null)
        {
            dismantleProgressFill.transform.parent.gameObject.SetActive(canDismantle);
        }
        
        // Position UI above the item in screen space (like game's interact UI)
        if (Camera.main != null)
        {
            Vector3 worldPos = transform.position + Vector3.up * 1.2f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            
            // Only show if in front of camera
            if (screenPos.z > 0)
            {
                RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
                panelRect.position = screenPos;
            }
            else
            {
                screenUI.SetActive(false);
            }
        }
    }
    
    private void HandleDismantleInput()
    {
        if (!isInRange || nearestItem != this) return;
        
        // Check for interactAlt (G key by default) - same logic as game
        bool interactAltDown = DewInput.GetButtonDown(DewSave.profileMain.controls.interactAlt, checkGameAreaForMouse: true);
        bool interactAltHeld = DewInput.GetButton(DewSave.profileMain.controls.interactAlt, checkGameAreaForMouse: true);
        bool interactAltHoldRepeat = Time.unscaledTime - _lastInteractAltUnscaledTime > DismantleHoldRepeatTime && interactAltHeld;
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        // INSTANT dismantle with Ctrl + dismantle key - skip hold-to-confirm!
        if (interactAltDown && ctrlHeld)
        {
            DoInstantDismantle();
            return;
        }
        
        // Normal hold-to-confirm behavior
        if (interactAltDown || interactAltHoldRepeat)
        {
            if (interactAltDown || interactAltHoldRepeat)
            {
                _lastInteractAltUnscaledTime = Time.unscaledTime;
            }
            DoDismantleTap();
        }
    }
    
    /// <summary>
    /// Instantly dismantle the item (Ctrl+key shortcut)
    /// </summary>
    private void DoInstantDismantle()
    {
        RPGLog.Debug(" Instant dismantle (Ctrl+key): " + item.name);
        
        // Use networked system if we have a network ID
        if (networkDropId != 0)
        {
            // Force complete the dismantle
            NetworkedItemSystem.RequestInstantDismantle(networkDropId);
            PlayDismantleTapEffect();
            return;
        }
        
        // Local/offline instant dismantle
        CompleteDismantle();
    }
    
    private void DoDismantleTap()
    {
        if (Time.time - _lastDismantleTapTime < DismantleTapMinInterval) return;
        
        // Use networked system if we have a network ID
        if (networkDropId != 0)
        {
            NetworkedItemSystem.RequestDismantleTap(networkDropId);
            // Progress will be updated via network callback
            PlayDismantleTapEffect();
            _lastDismantleTapTime = Time.time;
            return;
        }
        
        // Local/offline dismantle
        _lastDismantleTapTime = Time.time;
        dismantleProgress += DismantleTapStrength;
        _isDismantling = true;
        
        // Play tap effect
        PlayDismantleTapEffect();
        
        RPGLog.Debug(" Dismantle tap! Progress: " + dismantleProgress.ToString("F2"));
    }
    
    /// <summary>
    /// Update dismantle progress from network (called by NetworkedItemSystem)
    /// </summary>
    public void SetDismantleProgress(float progress)
    {
        dismantleProgress = progress;
        _isDismantling = progress > 0;
    }
    
    private void PlayDismantleTapEffect()
    {
        // Play game's actual dismantle tap effect if available
        if (_fxDismantleTap != null)
        {
            DewEffect.PlayNew(_fxDismantleTap, transform.position, null, null);
        }
        
        // Flash the pulse light
        if (pulseLight != null)
        {
            StartCoroutine(PulseLightEffect());
        }
        
        // Scale punch effect on visual
        if (visualObject != null)
        {
            StartCoroutine(ScalePunchEffect());
        }
    }
    
    private IEnumerator PulseLightEffect()
    {
        pulseLight.intensity = 3f;
        float elapsed = 0f;
        float duration = 0.2f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            pulseLight.intensity = Mathf.Lerp(3f, 0f, elapsed / duration);
            yield return null;
        }
        
        pulseLight.intensity = 0f;
    }
    
    private IEnumerator ScalePunchEffect()
    {
        Vector3 originalScale = visualObject.transform.localScale;
        Vector3 punchScale = originalScale * 1.3f;
        
        // Punch up
        float elapsed = 0f;
        float duration = 0.1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            visualObject.transform.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / duration);
            yield return null;
        }
        
        // Return to normal
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            visualObject.transform.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / duration);
            yield return null;
        }
        
        visualObject.transform.localScale = originalScale;
    }
    
    private void UpdateDismantleProgress()
    {
        // Reset progress if too much time passed (like game does)
        if (dismantleProgress > 0f && Time.time - _lastDismantleTapTime > DismantleDecayStartTime)
        {
            dismantleProgress = 0f;
            _isDismantling = false;
        }
        
        // Update progress bar (smooth like game)
        if (dismantleProgressFill != null)
        {
            float targetFill = Mathf.Clamp01(dismantleProgress / 0.8f); // Same as game (fills at 0.8 = ~2-3 taps)
            float currentFill = dismantleProgressFill.GetComponent<RectTransform>().anchorMax.x;
            float smoothFill = Mathf.SmoothDamp(currentFill, targetFill, ref _fillCv, 0.1f);
            
            RectTransform fillRect = dismantleProgressFill.GetComponent<RectTransform>();
            fillRect.anchorMax = new Vector2(smoothFill, 1);
        }
        
        // Complete dismantle when progress >= 1.0
        if (dismantleProgress >= 1f)
        {
            CompleteDismantle();
        }
    }
    
    private void CompleteDismantle()
    {
        // Play game's actual dismantle complete effect if available
        if (_fxDismantle != null)
        {
            DewEffect.PlayNew(_fxDismantle, transform.position, null, null);
        }
        
        // Play completion effect
        StartCoroutine(DismantleCompleteEffect());
    }
    
    private IEnumerator DismantleCompleteEffect()
    {
        // Big flash
        if (pulseLight != null)
        {
            pulseLight.intensity = 5f;
            pulseLight.range = 8f;
        }
        
        // Scale up and fade
        if (visualObject != null)
        {
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 startScale = visualObject.transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                visualObject.transform.localScale = startScale * (1f + t * 0.5f);
                
                // Fade light
                if (itemLight != null)
                {
                    itemLight.intensity = Mathf.Lerp(glowIntensity * 2f, 0f, t);
                }
                if (pulseLight != null)
                {
                    pulseLight.intensity = Mathf.Lerp(5f, 0f, t);
                }
                
                yield return null;
            }
        }
        
        // Actually dismantle
        TryDismantle();
    }
    
    private void CheckPlayerDistance()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null)
        {
            SetInRange(false);
            return;
        }
        
        Hero hero = DewPlayer.local.hero;
        Vector3 itemPos = transform.position;
        Vector3 heroPos = hero.position;
        float distance = Vector2.Distance(new Vector2(itemPos.x, itemPos.z), new Vector2(heroPos.x, heroPos.z));
        
        bool wasInRange = isInRange;
        isInRange = distance < pickupRange;
        
        if (isInRange != wasInRange)
        {
            SetInRange(isInRange);
        }
        
        if (isInRange)
        {
            if (nearestItem == null)
            {
                nearestItem = this;
            }
            else if (nearestItem != this)
            {
                float nearestDist = Vector3.Distance(nearestItem.transform.position, hero.position);
                if (distance < nearestDist)
                {
                    if (nearestItem != null) nearestItem.SetHighlighted(false);
                    nearestItem = this;
                }
            }
            
            SetHighlighted(nearestItem == this);
        }
        else if (nearestItem == this)
        {
            nearestItem = null;
            SetHighlighted(false);
            FindNewNearestItem();
        }
    }
    
    private void SetInRange(bool inRange)
    {
        isInRange = inRange;
    }
    
    private void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
        
        // Update visual glow
        if (visualObject != null)
        {
            Transform iconQuad = visualObject.transform.Find("IconQuad");
            if (iconQuad != null)
            {
                MeshRenderer mr = iconQuad.GetComponent<MeshRenderer>();
                if (mr != null && mr.material != null)
                {
                    mr.material.SetColor("_EmissionColor", glowColor * (highlighted ? 1.5f : 0.8f));
                }
            }
        }
        
        // Increase light when highlighted
        if (itemLight != null)
        {
            itemLight.range = highlighted ? 4f : 3f;
        }
        
        // Reset dismantle if no longer highlighted
        if (!highlighted)
        {
            dismantleProgress = 0f;
            _isDismantling = false;
        }
    }
    
    private static void FindNewNearestItem()
    {
        if (DewPlayer.local == null || DewPlayer.local.hero == null) return;
        
        Hero hero = DewPlayer.local.hero;
        Vector3 heroPos = hero.position;
        DroppedItem closest = null;
        float closestDist = float.MaxValue;
        
        foreach (DroppedItem item in allDroppedItems)
        {
            if (item == null || !item.isInRange) continue;
            
            Vector3 itemPos = item.transform.position;
            float dist = Vector2.Distance(new Vector2(itemPos.x, itemPos.z), new Vector2(heroPos.x, heroPos.z));
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = item;
            }
        }
        
        if (closest != null)
        {
            nearestItem = closest;
            closest.SetHighlighted(true);
        }
    }
    
    private void TryPickUp()
    {
        if (!isInRange) return;
        
        // Use networked system if we have a network ID
        if (networkDropId != 0)
        {
            NetworkedItemSystem.RequestPickup(networkDropId);
            return;
        }
        
        // Local/offline pickup
        InventoryManager inventory = FindInventoryManager();
        if (inventory != null)
        {
            bool added = inventory.AddItem(item);
            if (added)
            {
                RPGLog.Debug(" Picked up: " + item.name);
                CleanupAndDestroy();
            }
            else
            {
                RPGLog.Debug(" Inventory full, cannot pick up: " + item.name);
            }
        }
    }
    
    /// <summary>
    /// Clean up this dropped item and destroy it
    /// </summary>
    public void CleanupAndDestroy()
    {
        // Mark as being destroyed FIRST to prevent CameraManager null reference errors
        _isBeingDestroyed = true;
        
        if (nearestItem == this)
        {
            nearestItem = null;
        }
        
        allDroppedItems.Remove(this);
        
        if (networkDropId != 0 && droppedItemsByNetId.ContainsKey(networkDropId))
        {
            droppedItemsByNetId.Remove(networkDropId);
        }
        
        Destroy(gameObject);
        FindNewNearestItem();
    }
    
    /// <summary>
    /// Show "Shared!" message above the item (like the game does)
    /// </summary>
    public void ShowSharedMessage()
    {
        // Create a floating "Shared!" text that fades out
        StartCoroutine(ShowSharedMessageCoroutine());
    }
    
    /// <summary>
    /// Try to share this item with other players (middle-click)
    /// </summary>
    private void TryShareItem()
    {
        if (networkDropId == 0)
        {
            // Log removed
            return;
        }
        
        // Use the networked system to share
        NetworkedItemSystem.ShareItem(networkDropId);
    }
    
    /// <summary>
    /// Called by the game's ping system via IPingableCustom when middle-clicking on our UI
    /// Returns true if we handled the ping (to prevent ground ping)
    /// </summary>
    public bool HandlePingShare()
    {
        // Only handle if this is the nearest/focused item and we're in range
        if (nearestItem != this) return false;
        if (!isInRange && !isMouseHovering) return false;
        
        // Only owner can share items that aren't already shared
        if (networkDropId != 0)
        {
            TryShareItem();
            return true; // We handled the ping, don't ping ground
        }
        
        return false;
    }
    
    private System.Collections.IEnumerator ShowSharedMessageCoroutine()
    {
        // Create floating text
        GameObject sharedObj = new GameObject("SharedMessage");
        sharedObj.transform.SetParent(screenCanvas.transform, false);
        
        Text sharedText = sharedObj.AddComponent<Text>();
        sharedText.text = "<size=130%><color=#91faff>" + Localization.Shared + "</color></size>";
        sharedText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        sharedText.fontSize = 18;
        sharedText.alignment = TextAnchor.MiddleCenter;
        sharedText.raycastTarget = false;
        
        Outline outline = sharedObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
        
        RectTransform rect = sharedText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(200, 50);
        
        // Animate: float up and fade out
        float duration = 1.5f;
        float elapsed = 0f;
        Vector2 startPos = new Vector2(0, 30);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Move up
            rect.anchoredPosition = startPos + new Vector2(0, t * 50);
            
            // Fade out
            Color c = sharedText.color;
            c.a = 1f - t;
            sharedText.color = c;
            
            yield return null;
        }
        
        Destroy(sharedObj);
    }
    
    /// <summary>
    /// Get a dropped item by its network ID
    /// </summary>
    public static DroppedItem GetByNetworkId(uint netId)
    {
        DroppedItem item;
        droppedItemsByNetId.TryGetValue(netId, out item);
        return item;
    }
    
    private void TryDismantle()
    {
        if (item == null) return;
        
        // Prevent dismantling consumables, materials, and quest items
        if (item.type == ItemType.Consumable || 
            item.type == ItemType.Material || 
            item.type == ItemType.Quest)
        {
            dismantleProgress = 0f;
            _isDismantling = false;
            return;
        }
        
        int dreamDustAmount = GetDismantleAmount();
        
        if (NetworkedManagerBase<PickupManager>.instance != null && DewPlayer.local != null && DewPlayer.local.hero != null)
        {
            NetworkedManagerBase<PickupManager>.instance.DropDreamDust(false, dreamDustAmount, transform.position, DewPlayer.local.hero);
            RPGLog.Debug(" Dismantled " + item.name + " for " + dreamDustAmount + " Dream Dust");
        }
        else
        {
            RPGLog.Warning(" PickupManager or Hero not found, cannot drop Dream Dust");
        }
        
        if (nearestItem == this)
        {
            nearestItem = null;
        }
        
        allDroppedItems.Remove(this);
        Destroy(gameObject);
        
        FindNewNearestItem();
    }
    
    private int GetDismantleAmount()
    {
        int baseAmount = 10;
        
        // Rarity multiplier - same as NetworkedItemSystem.CalculateDustAmount
        switch (item.rarity)
        {
            case ItemRarity.Common: baseAmount = 10; break;
            case ItemRarity.Rare: baseAmount = 25; break;
            case ItemRarity.Epic: baseAmount = 100; break;
            case ItemRarity.Legendary: baseAmount = 200; break;
        }
        
        // Add bonus for stats
        baseAmount += (item.attackBonus + item.defenseBonus + item.healthBonus / 10 + item.goldOnKill * 5) * 2;
        
        return Mathf.Max(1, baseAmount);
    }
    
    private InventoryManager FindInventoryManager()
    {
        RPGItemsMod mod = UnityEngine.Object.FindFirstObjectByType<RPGItemsMod>();
        if (mod != null)
        {
            return mod.GetInventoryManager();
        }
        return null;
    }
    
    private void OnDestroy()
    {
        // Mark as being destroyed to prevent CameraManager null reference errors
        _isBeingDestroyed = true;
        
        allDroppedItems.Remove(this);
        
        // Clean up network tracking
        if (networkDropId != 0 && droppedItemsByNetId.ContainsKey(networkDropId))
        {
            droppedItemsByNetId.Remove(networkDropId);
        }
        
        // Clean up screen UI
        if (screenUI != null)
        {
            Destroy(screenUI);
        }
    }
    
    public static List<DroppedItem> GetAllDroppedItems()
    {
        return new List<DroppedItem>(allDroppedItems);
    }
    
    /// <summary>
    /// Clear all dropped items (call when leaving game)
    /// </summary>
    public static void ClearAll()
    {
        foreach (DroppedItem item in allDroppedItems.ToArray())
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        allDroppedItems.Clear();
        droppedItemsByNetId.Clear();
        nearestItem = null;
    }
    
    public static void ClearAllDroppedItems()
    {
        foreach (DroppedItem item in allDroppedItems.ToArray())
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        allDroppedItems.Clear();
    }
    
    /// <summary>
    /// Check for middle-click to share items. Call this from RPGItemsMod.Update()
    /// NOTE: This is no longer needed - we use Harmony patch on SendPingPC instead
    /// </summary>
    public static void CheckMiddleClickShare()
    {
        // No longer used - Harmony patch handles this now
    }
    
    /// <summary>
    /// Try to share the nearest dropped item. Called from Harmony patch on SendPingPC.
    /// Returns true if we handled the ping (shared an item), false to let normal ping happen.
    /// Only intercepts ping if the cursor is actually over the dropped item's UI tooltip.
    /// Works for both host and clients.
    /// </summary>
    public static bool TryShareNearestItem()
    {
        // If there's a nearest item that we can share
        if (nearestItem != null && nearestItem.isInRange)
        {
            // Only intercept ping if cursor is actually over the item's UI panel
            if (nearestItem.IsCursorOverTooltip())
            {
                return nearestItem.HandlePingShare();
            }
        }
        return false;
    }
    
    /// <summary>
    /// Check if the mouse cursor is over this dropped item's tooltip UI
    /// </summary>
    public bool IsCursorOverTooltip()
    {
        if (uiPanel == null) return false;
        
        RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
        if (panelRect == null) return false;
        
        // Get the panel's screen position and size
        Vector3[] corners = new Vector3[4];
        panelRect.GetWorldCorners(corners);
        
        // Convert to screen space (corners are already in screen space for Screen Space - Overlay canvas)
        Vector2 mousePos = Input.mousePosition;
        
        // Check if mouse is within the panel bounds
        // corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right
        float minX = corners[0].x;
        float maxX = corners[2].x;
        float minY = corners[0].y;
        float maxY = corners[2].y;
        
        return mousePos.x >= minX && mousePos.x <= maxX && 
               mousePos.y >= minY && mousePos.y <= maxY;
    }
}

/// <summary>
/// Simple billboard component to make the beam always face the camera
/// </summary>
public class BillboardBeam : MonoBehaviour
{
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        // Make the beam face the camera (only rotate around Y axis to stay vertical)
        Vector3 lookDir = mainCamera.transform.position - transform.position;
        lookDir.y = 0; // Keep vertical
        if (lookDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(-lookDir) * Quaternion.Euler(0, 0, 0);
        }
    }
}

/// <summary>
/// Helper component that implements IPingableCustom to prevent ground pings when sharing items
/// This is added to the dropped item's UI panel so the game's ping system knows we handle the ping
/// </summary>
public class DroppedItemPingHandler : MonoBehaviour, IPingableCustom
{
    public DroppedItem droppedItem;
    
    public bool OnPing()
    {
        // If we have a valid dropped item and the user is trying to share it
        if (droppedItem != null)
        {
            // Try to share the item - this returns true if share was handled
            return droppedItem.HandlePingShare();
        }
        return false;
    }
}
