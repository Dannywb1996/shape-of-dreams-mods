using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteDungeonMod
{
    public partial class InfiniteDungeonMod
    {
        // ==================== INFECTION UI & OBLIVIAX ====================
        /// Create or update the infection status UI at the top of the screen
        /// </summary>
        private void UpdateInfectionUI()
        {
            try
            {
                // Check if we have any infection or curses to display
                bool hasInfection = _playerIsInfected && _playerInfectionCharges > 0;
                bool hasCurses = _playerCurses.Count > 0;
                
                if (!hasInfection && !hasCurses)
                {
                    // Hide UI if nothing to show
                    if (_infectionUI != null)
                    {
                        _infectionUI.SetActive(false);
                    }
                    return;
                }
                
                // Create UI if it doesn't exist
                if (_infectionUI == null)
                {
                    CreateInfectionUI();
                }
                
                if (_infectionUI == null || _infectionText == null) return;
                
                // Build the status text
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                
                // Infection status
                if (hasInfection)
                {
                    string infectionLabel = DungeonLocalization.IsChinese ? "猎人感染" : "Hunter Infection";
                    sb.Append("<color=#ff6600>" + infectionLabel + ": " + _playerInfectionCharges + "</color>");
                }
                
                // Curse statuses
                if (hasCurses)
                {
                    if (sb.Length > 0) sb.Append("  |  ");
                    
                    string cursesLabel = DungeonLocalization.IsChinese ? "诅咒" : "Curses";
                    sb.Append("<color=#ff4444>" + cursesLabel + ": </color>");
                    
                    bool first = true;
                    foreach (var kvp in _playerCurses)
                    {
                        if (!first) sb.Append(", ");
                        first = false;
                        
                        string curseName = DungeonLocalization.GetCurseName(kvp.Key);
                        sb.Append("<color=" + GetCurseColor(kvp.Key) + ">" + curseName + "(" + kvp.Value + ")</color>");
                    }
                }
                
                _infectionText.text = sb.ToString();
                _infectionUI.SetActive(true);
            }
            catch (Exception ex)
            {
                Debug.LogError("[InfiniteDungeon] Error in UpdateInfectionUI: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Create the infection status UI
        /// </summary>
        private void CreateInfectionUI()
        {
            try
            {
                // Find the main canvas
                Canvas mainCanvas = null;
                foreach (var canvas in UnityEngine.Object.FindObjectsOfType<Canvas>())
                {
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        mainCanvas = canvas;
                        break;
                    }
                }
                
                if (mainCanvas == null)
                {
                    Debug.LogWarning("[InfiniteDungeon] Could not find main canvas for infection UI");
                    return;
                }
                
                // Create the UI container
                _infectionUI = new GameObject("InfiniteDungeon_InfectionUI");
                _infectionUI.transform.SetParent(mainCanvas.transform, false);
                
                // Add RectTransform and position at top center
                RectTransform rect = _infectionUI.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -10f);
                rect.sizeDelta = new Vector2(800f, 50f);
                
                // Add background image
                Image bg = _infectionUI.AddComponent<Image>();
                bg.color = new Color(0f, 0f, 0f, 0.7f);
                
                // Add horizontal layout
                HorizontalLayoutGroup layout = _infectionUI.AddComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(20, 20, 5, 5);
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                
                // Add content size fitter
                ContentSizeFitter fitter = _infectionUI.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                // Create text object using game's widget for proper font support
                _infectionText = UnityEngine.Object.Instantiate(DewGUI.widgetTextBody, _infectionUI.transform);
                _infectionText.fontSize = 22;
                _infectionText.alignment = TextAlignmentOptions.Center;
                _infectionText.color = Color.white;
                _infectionText.richText = true;
                
                // Add LayoutElement to make it work with the layout group
                LayoutElement textLayout = _infectionText.gameObject.AddComponent<LayoutElement>();
                textLayout.preferredWidth = 800f;
            }
            catch (Exception)
            {
                // Silently ignore UI creation errors - not critical
            }
        }
        
        /// <summary>
        /// 5% chance to trigger Obliviax kidnap when entering a hunted node
        /// Instead of starting the quest (which requires racing to a boss node),
        /// we directly trigger the kidnap effect which takes the player to Obliviax's Nest
    }
}
