using ModAPI.Attributes;
using TheForest.Utils;
using UnityEngine;

/// <summary>
/// The Forest Custom HUD Mod
/// Replaces circular gauges with rectangle bars
/// </summary>
class CustomHudMod : MonoBehaviour
{
    [ExecuteOnGameStart]
    static void AddMeToScene()
    {
        new GameObject("__CustomHudMod__").AddComponent<CustomHudMod>();
    }

    #region === Settings ===

    // Position - TopLeft
    private HudPosition currentPosition = HudPosition.TopLeft;
    private float offsetX = 20f;
    private float offsetY = 20f;

    // Size
    private float barWidth = 200f;
    private float barHeight = 18f;
    private float barSpacing = 6f;

    // Style
    private bool showLabels = true;
    private bool showValues = true;
    private bool showBorder = true;
    private bool showBackground = true;
    private float borderWidth = 2f;

    #endregion

    #region === Enums ===

    public enum HudPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    #endregion

    #region === Colors ===

    private Color healthFillColor = new Color(0.85f, 0.2f, 0.2f, 1f);
    private Color staminaFillColor = new Color(0.2f, 0.8f, 0.3f, 1f);
    private Color energyFillColor = new Color(0.95f, 0.85f, 0.2f, 1f);
    private Color foodFillColor = new Color(1f, 0.55f, 0.2f, 1f);
    private Color waterFillColor = new Color(0.3f, 0.7f, 1f, 1f);
    private Color armorFillColor = new Color(0.6f, 0.6f, 0.65f, 1f);

    private Color healthBgColor = new Color(0.3f, 0.1f, 0.1f, 0.85f);
    private Color staminaBgColor = new Color(0.1f, 0.25f, 0.1f, 0.85f);
    private Color energyBgColor = new Color(0.3f, 0.28f, 0.1f, 0.85f);
    private Color foodBgColor = new Color(0.3f, 0.18f, 0.1f, 0.85f);
    private Color waterBgColor = new Color(0.1f, 0.2f, 0.3f, 0.85f);
    private Color armorBgColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);

    private Color borderColor = new Color(1f, 1f, 1f, 0.6f);
    private Color labelColor = Color.white;
    private Color valueColor = Color.white;
    private Color shadowColor = new Color(0f, 0f, 0f, 0.7f);
    private Color panelBgColor = new Color(0f, 0f, 0f, 0.5f);

    private Color warningColor = new Color(1f, 0.3f, 0.3f, 1f);
    private float warningThreshold = 0.25f;
    private float pulseSpeed = 3f;

    #endregion

    #region === Internal Variables ===

    private Texture2D whiteTexture;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private bool stylesInitialized = false;
    private bool isInitialized = false;
    private bool originalHudHidden = false;
    private float pulseTime = 0f;

    #endregion

    #region === Unity Lifecycle ===

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();

        DontDestroyOnLoad(gameObject);

        isInitialized = true;
        ModAPI.Log.Write("[CustomHudMod] Initialized!");
    }

    void Update()
    {
        if (!isInitialized) return;

        pulseTime += Time.deltaTime * pulseSpeed;

        // Keep hiding original HUD every frame
        if (!originalHudHidden)
        {
            HideOriginalHudComplete();
        }
    }

    void LateUpdate()
    {
        // Double check - hide in LateUpdate too
        HideOriginalHudComplete();
    }

    void OnGUI()
    {
        if (!isInitialized) return;

        InitStyles();

        if (LocalPlayer.Stats == null) return;
        if (LocalPlayer.IsInPauseMenu) return;
        if (LocalPlayer.Inventory == null) return;
        if (!LocalPlayer.Inventory.enabled) return;

        DrawCustomHud();
    }

    void OnDestroy()
    {
        ShowOriginalHud();
    }

    #endregion

    #region === Original HUD Control ===

    void HideOriginalHudComplete()
    {
        if (Scene.HudGui == null) return;

        try
        {
            HudGui hud = Scene.HudGui;

            // === Health ===
            if (hud.HealthBar != null)
            {
                hud.HealthBar.gameObject.SetActive(false);
                hud.HealthBar.enabled = false;
            }
            if (hud.HealthBarTarget != null)
            {
                hud.HealthBarTarget.gameObject.SetActive(false);
                hud.HealthBarTarget.enabled = false;
            }
            SetGameObjectActive(hud.HealthBarOutline, false);

            // === Stamina ===
            if (hud.StaminaBar != null)
            {
                hud.StaminaBar.gameObject.SetActive(false);
                hud.StaminaBar.enabled = false;
            }
            SetGameObjectActive(hud.StaminaBarOutline, false);

            // === Energy ===
            if (hud.EnergyBar != null)
            {
                hud.EnergyBar.gameObject.SetActive(false);
                hud.EnergyBar.enabled = false;
            }
            SetGameObjectActive(hud.EnergyBarOutline, false);

            // === Stomach (Food) ===
            if (hud.Stomach != null)
            {
                hud.Stomach.gameObject.SetActive(false);
                hud.Stomach.enabled = false;
            }
            SetGameObjectActive(hud.StomachOutline, false);
            if (hud.StomachStarvation != null)
            {
                hud.StomachStarvation.gameObject.SetActive(false);
                hud.StomachStarvation.enabled = false;
            }

            // === Hydration (Water) ===
            if (hud.Hydration != null)
            {
                hud.Hydration.gameObject.SetActive(false);
                hud.Hydration.enabled = false;
            }
            SetGameObjectActive(hud.ThirstOutline, false);
            if (hud.ThirstDamageTimer != null)
            {
                hud.ThirstDamageTimer.gameObject.SetActive(false);
                hud.ThirstDamageTimer.enabled = false;
            }

            // === Armor ===
            if (hud.ArmorBar != null)
            {
                hud.ArmorBar.gameObject.SetActive(false);
                hud.ArmorBar.enabled = false;
            }
            if (hud.ColdArmorBar != null)
            {
                hud.ColdArmorBar.gameObject.SetActive(false);
                hud.ColdArmorBar.enabled = false;
            }

            // === Armor Nibbles ===
            if (hud.ArmorNibbles != null)
            {
                for (int i = 0; i < hud.ArmorNibbles.Length; i++)
                {
                    if (hud.ArmorNibbles[i] != null)
                    {
                        hud.ArmorNibbles[i].gameObject.SetActive(false);
                        hud.ArmorNibbles[i].enabled = false;
                    }
                }
            }

            // === Try to find and hide parent containers ===
            // Health group
            if (hud.HealthBar != null && hud.HealthBar.transform.parent != null)
            {
                hud.HealthBar.transform.parent.gameObject.SetActive(false);
            }

            // Stomach group
            if (hud.Stomach != null && hud.Stomach.transform.parent != null)
            {
                hud.Stomach.transform.parent.gameObject.SetActive(false);
            }

            // Hydration group  
            if (hud.Hydration != null && hud.Hydration.transform.parent != null)
            {
                hud.Hydration.transform.parent.gameObject.SetActive(false);
            }

            originalHudHidden = true;
        }
        catch (System.Exception e)
        {
            ModAPI.Log.Write("[CustomHudMod] HUD hide error: " + e.Message);
        }
    }

    void ShowOriginalHud()
    {
        if (Scene.HudGui == null) return;

        try
        {
            HudGui hud = Scene.HudGui;

            if (hud.HealthBar != null)
            {
                hud.HealthBar.gameObject.SetActive(true);
                hud.HealthBar.enabled = true;
                if (hud.HealthBar.transform.parent != null)
                    hud.HealthBar.transform.parent.gameObject.SetActive(true);
            }
            if (hud.HealthBarTarget != null)
            {
                hud.HealthBarTarget.gameObject.SetActive(true);
                hud.HealthBarTarget.enabled = true;
            }
            SetGameObjectActive(hud.HealthBarOutline, true);

            if (hud.StaminaBar != null)
            {
                hud.StaminaBar.gameObject.SetActive(true);
                hud.StaminaBar.enabled = true;
            }
            SetGameObjectActive(hud.StaminaBarOutline, true);

            if (hud.EnergyBar != null)
            {
                hud.EnergyBar.gameObject.SetActive(true);
                hud.EnergyBar.enabled = true;
            }
            SetGameObjectActive(hud.EnergyBarOutline, true);

            if (hud.Stomach != null)
            {
                hud.Stomach.gameObject.SetActive(true);
                hud.Stomach.enabled = true;
                if (hud.Stomach.transform.parent != null)
                    hud.Stomach.transform.parent.gameObject.SetActive(true);
            }
            SetGameObjectActive(hud.StomachOutline, true);

            if (hud.Hydration != null)
            {
                hud.Hydration.gameObject.SetActive(true);
                hud.Hydration.enabled = true;
                if (hud.Hydration.transform.parent != null)
                    hud.Hydration.transform.parent.gameObject.SetActive(true);
            }
            SetGameObjectActive(hud.ThirstOutline, true);

            if (hud.ArmorBar != null)
            {
                hud.ArmorBar.gameObject.SetActive(true);
                hud.ArmorBar.enabled = true;
            }
            if (hud.ColdArmorBar != null)
            {
                hud.ColdArmorBar.gameObject.SetActive(true);
                hud.ColdArmorBar.enabled = true;
            }
        }
        catch { }
    }

    void SetGameObjectActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    #endregion

    #region === Style Initialization ===

    void InitStyles()
    {
        if (stylesInitialized) return;

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        labelStyle.normal.textColor = labelColor;

        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleRight
        };
        valueStyle.normal.textColor = valueColor;

        stylesInitialized = true;
    }

    #endregion

    #region === HUD Drawing ===

    void DrawCustomHud()
    {
        Vector2 startPos = CalculateStartPosition();
        float currentY = startPos.y;

        if (showBackground)
        {
            float panelHeight = (barHeight + barSpacing) * 6 + 10;
            float panelWidth = barWidth + 20;

            if (showLabels) panelWidth += 70;
            if (showValues) panelWidth += 65;

            GUI.color = panelBgColor;
            GUI.DrawTexture(new Rect(startPos.x - 10, startPos.y - 5, panelWidth, panelHeight), whiteTexture);
            GUI.color = Color.white;
        }

        // Health
        DrawStatBar(startPos.x, currentY, "Health",
            LocalPlayer.Stats.Health, 100f,
            healthFillColor, healthBgColor, true);
        currentY += barHeight + barSpacing;

        // Stamina
        DrawStatBar(startPos.x, currentY, "Stamina",
            LocalPlayer.Stats.Stamina, LocalPlayer.Stats.Energy,
            staminaFillColor, staminaBgColor, true);
        currentY += barHeight + barSpacing;

        // Energy
        DrawStatBar(startPos.x, currentY, "Energy",
            LocalPlayer.Stats.Energy, 100f,
            energyFillColor, energyBgColor, true);
        currentY += barHeight + barSpacing;

        // Food
        DrawStatBar(startPos.x, currentY, "Food",
            LocalPlayer.Stats.Fullness * 100f, 100f,
            foodFillColor, foodBgColor, true);
        currentY += barHeight + barSpacing;

        // Water
        DrawStatBar(startPos.x, currentY, "Water",
            (1f - LocalPlayer.Stats.Thirst) * 100f, 100f,
            waterFillColor, waterBgColor, true);
        currentY += barHeight + barSpacing;

        // Armor
        if (LocalPlayer.Stats.Armor > 0)
        {
            DrawStatBar(startPos.x, currentY, "Armor",
                LocalPlayer.Stats.Armor, 10f,
                armorFillColor, armorBgColor, false);
        }
    }

    void DrawStatBar(float x, float y, string label, float current, float max,
                     Color fillColor, Color bgColor, bool enableWarning)
    {
        float labelWidth = showLabels ? 70f : 0f;
        float valueWidth = showValues ? 65f : 0f;
        float actualBarWidth = barWidth;

        float fillAmount = Mathf.Clamp01(current / max);

        bool isWarning = enableWarning && fillAmount < warningThreshold;
        float pulse = isWarning ? (Mathf.Sin(pulseTime) * 0.3f + 0.7f) : 1f;

        // Label
        if (showLabels)
        {
            GUI.color = shadowColor;
            GUI.Label(new Rect(x + 1, y + 1, labelWidth, barHeight), label, labelStyle);

            GUI.color = isWarning ? Color.Lerp(labelColor, warningColor, pulse) : labelColor;
            GUI.Label(new Rect(x, y, labelWidth, barHeight), label, labelStyle);
        }

        float barX = x + labelWidth;

        // Background
        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(barX, y, actualBarWidth, barHeight), whiteTexture);

        // Fill
        Color currentFillColor = isWarning ? Color.Lerp(fillColor, warningColor, pulse) : fillColor;
        currentFillColor.a *= pulse;
        GUI.color = currentFillColor;
        GUI.DrawTexture(new Rect(barX, y, actualBarWidth * fillAmount, barHeight), whiteTexture);

        // Border
        if (showBorder)
        {
            GUI.color = isWarning ? Color.Lerp(borderColor, warningColor, pulse * 0.5f) : borderColor;
            DrawBorder(new Rect(barX, y, actualBarWidth, barHeight), borderWidth);
        }

        // Value
        if (showValues)
        {
            string valueText = string.Format("{0:F0}/{1:F0}", current, max);
            float valueX = barX + actualBarWidth + 5;

            GUI.color = shadowColor;
            GUI.Label(new Rect(valueX + 1, y + 1, valueWidth, barHeight), valueText, valueStyle);

            GUI.color = isWarning ? Color.Lerp(valueColor, warningColor, pulse) : valueColor;
            GUI.Label(new Rect(valueX, y, valueWidth, barHeight), valueText, valueStyle);
        }

        GUI.color = Color.white;
    }

    void DrawBorder(Rect rect, float width)
    {
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, width), whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - width, rect.width, width), whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, width, rect.height), whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - width, rect.y, width, rect.height), whiteTexture);
    }

    Vector2 CalculateStartPosition()
    {
        float totalHeight = (barHeight + barSpacing) * 6;
        float totalWidth = barWidth;
        if (showLabels) totalWidth += 70;
        if (showValues) totalWidth += 65;

        float x = offsetX;
        float y = offsetY;

        switch (currentPosition)
        {
            case HudPosition.TopLeft:
                x = offsetX;
                y = offsetY;
                break;
            case HudPosition.TopCenter:
                x = (Screen.width - totalWidth) / 2;
                y = offsetY;
                break;
            case HudPosition.TopRight:
                x = Screen.width - totalWidth - offsetX;
                y = offsetY;
                break;
            case HudPosition.BottomLeft:
                x = offsetX;
                y = Screen.height - totalHeight - offsetY;
                break;
            case HudPosition.BottomCenter:
                x = (Screen.width - totalWidth) / 2;
                y = Screen.height - totalHeight - offsetY;
                break;
            case HudPosition.BottomRight:
                x = Screen.width - totalWidth - offsetX;
                y = Screen.height - totalHeight - offsetY;
                break;
        }

        return new Vector2(x, y);
    }

    #endregion
}