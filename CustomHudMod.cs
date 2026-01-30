using ModAPI.Attributes;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

class CustomHudMod : MonoBehaviour
{
    [ExecuteOnGameStart]
    static void AddMeToScene()
    {
        new GameObject("__CustomHudMod__").AddComponent<CustomHudMod>();
    }

    private const float BASE_HEIGHT = 1080f;

    private HudPosition currentPosition = HudPosition.TopLeft;
    private float baseOffsetX = 15f;
    private float baseOffsetY = 15f;

    private float baseBarWidth = 160f;
    private float baseBarHeight = 14f;
    private float baseBarSpacing = 5f;
    private float baseBorderWidth = 1.5f;
    private float baseLabelWidth = 55f;
    private float baseValueWidth = 50f;
    private float baseLabelFontSize = 10f;
    private float baseValueFontSize = 9f;

    private bool showLabels = true;
    private bool showValues = true;
    private bool showBorder = true;
    private bool showBackground = true;

    private float scale = 1f;
    private float barWidth;
    private float barHeight;
    private float barSpacing;
    private float borderWidth;
    private float offsetX;
    private float offsetY;
    private float labelWidth;
    private float valueWidth;
    private int labelFontSize;
    private int valueFontSize;

    private int lastScreenWidth;
    private int lastScreenHeight;

    public enum HudPosition
    {
        TopLeft, TopCenter, TopRight,
        BottomLeft, BottomCenter, BottomRight
    }

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

    private Texture2D whiteTexture;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private bool stylesInitialized = false;
    private bool isInitialized = false;
    private bool originalHudHidden = false;
    private bool playerAwake = false;
    private float pulseTime = 0f;

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
        CalculateScale();

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        pulseTime += Time.deltaTime * pulseSpeed;

        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            CalculateScale();
            stylesInitialized = false;
        }

        // 오리지널 HUD는 항상 숨김
        HideOriginalHudComplete();

        // 플레이어 조작 가능 여부 체크 (커스텀 HUD 표시용)
        if (!playerAwake && IsPlayerAwake())
        {
            playerAwake = true;
        }
    }

    void LateUpdate()
    {
        // 오리지널 HUD는 항상 숨김
        HideOriginalHudComplete();
    }

    void OnGUI()
    {
        if (!isInitialized) return;
        if (!playerAwake) return;
        if (!CanShowHud()) return;

        InitStyles();
        DrawCustomHud();
    }

    void OnDestroy()
    {
        ShowOriginalHud();
    }

    bool IsPlayerAwake()
    {
        if (LocalPlayer.Stats == null) return false;
        if (LocalPlayer.Inventory == null) return false;
        if (!LocalPlayer.Inventory.enabled) return false;
        if (LocalPlayer.AnimControl == null) return false;
        if (Clock.planecrash) return false;
        if (LocalPlayer.AnimControl.introCutScene) return false;
        if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Loading) return false;

        return true;
    }

    bool CanShowHud()
    {
        if (LocalPlayer.Stats == null) return false;
        if (LocalPlayer.Inventory == null) return false;
        if (!LocalPlayer.Inventory.enabled) return false;
        if (LocalPlayer.IsInPauseMenu) return false;

        if (LocalPlayer.AnimControl != null)
        {
            if (LocalPlayer.AnimControl.introCutScene) return false;
            if (LocalPlayer.AnimControl.endGameCutScene) return false;
        }

        return true;
    }

    void CalculateScale()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        scale = Screen.height / BASE_HEIGHT;
        scale = Mathf.Clamp(scale, 0.6f, 2.2f);

        barWidth = baseBarWidth * scale;
        barHeight = baseBarHeight * scale;
        barSpacing = baseBarSpacing * scale;
        borderWidth = Mathf.Max(1f, baseBorderWidth * scale);
        offsetX = baseOffsetX * scale;
        offsetY = baseOffsetY * scale;
        labelWidth = baseLabelWidth * scale;
        valueWidth = baseValueWidth * scale;

        labelFontSize = Mathf.Clamp(Mathf.RoundToInt(baseLabelFontSize * scale), 8, 20);
        valueFontSize = Mathf.Clamp(Mathf.RoundToInt(baseValueFontSize * scale), 7, 18);
    }

    void HideOriginalHudComplete()
    {
        if (Scene.HudGui == null) return;

        try
        {
            HudGui hud = Scene.HudGui;

            if (hud.HealthBar != null) { hud.HealthBar.gameObject.SetActive(false); hud.HealthBar.enabled = false; }
            if (hud.HealthBarTarget != null) { hud.HealthBarTarget.gameObject.SetActive(false); hud.HealthBarTarget.enabled = false; }
            SetGameObjectActive(hud.HealthBarOutline, false);

            if (hud.StaminaBar != null) { hud.StaminaBar.gameObject.SetActive(false); hud.StaminaBar.enabled = false; }
            SetGameObjectActive(hud.StaminaBarOutline, false);

            if (hud.EnergyBar != null) { hud.EnergyBar.gameObject.SetActive(false); hud.EnergyBar.enabled = false; }
            SetGameObjectActive(hud.EnergyBarOutline, false);

            if (hud.Stomach != null) { hud.Stomach.gameObject.SetActive(false); hud.Stomach.enabled = false; }
            SetGameObjectActive(hud.StomachOutline, false);
            if (hud.StomachStarvation != null) { hud.StomachStarvation.gameObject.SetActive(false); hud.StomachStarvation.enabled = false; }

            if (hud.Hydration != null) { hud.Hydration.gameObject.SetActive(false); hud.Hydration.enabled = false; }
            SetGameObjectActive(hud.ThirstOutline, false);
            if (hud.ThirstDamageTimer != null) { hud.ThirstDamageTimer.gameObject.SetActive(false); hud.ThirstDamageTimer.enabled = false; }

            if (hud.ArmorBar != null) { hud.ArmorBar.gameObject.SetActive(false); hud.ArmorBar.enabled = false; }
            if (hud.ColdArmorBar != null) { hud.ColdArmorBar.gameObject.SetActive(false); hud.ColdArmorBar.enabled = false; }

            if (hud.ArmorNibbles != null)
            {
                for (int i = 0; i < hud.ArmorNibbles.Length; i++)
                {
                    if (hud.ArmorNibbles[i] != null) { hud.ArmorNibbles[i].gameObject.SetActive(false); hud.ArmorNibbles[i].enabled = false; }
                }
            }

            if (hud.HealthBar != null && hud.HealthBar.transform.parent != null)
                hud.HealthBar.transform.parent.gameObject.SetActive(false);
            if (hud.Stomach != null && hud.Stomach.transform.parent != null)
                hud.Stomach.transform.parent.gameObject.SetActive(false);
            if (hud.Hydration != null && hud.Hydration.transform.parent != null)
                hud.Hydration.transform.parent.gameObject.SetActive(false);

            originalHudHidden = true;
        }
        catch { }
    }

    void ShowOriginalHud()
    {
        if (Scene.HudGui == null) return;

        try
        {
            HudGui hud = Scene.HudGui;

            if (hud.HealthBar != null) { hud.HealthBar.gameObject.SetActive(true); hud.HealthBar.enabled = true; if (hud.HealthBar.transform.parent != null) hud.HealthBar.transform.parent.gameObject.SetActive(true); }
            if (hud.HealthBarTarget != null) { hud.HealthBarTarget.gameObject.SetActive(true); hud.HealthBarTarget.enabled = true; }
            SetGameObjectActive(hud.HealthBarOutline, true);

            if (hud.StaminaBar != null) { hud.StaminaBar.gameObject.SetActive(true); hud.StaminaBar.enabled = true; }
            SetGameObjectActive(hud.StaminaBarOutline, true);

            if (hud.EnergyBar != null) { hud.EnergyBar.gameObject.SetActive(true); hud.EnergyBar.enabled = true; }
            SetGameObjectActive(hud.EnergyBarOutline, true);

            if (hud.Stomach != null) { hud.Stomach.gameObject.SetActive(true); hud.Stomach.enabled = true; if (hud.Stomach.transform.parent != null) hud.Stomach.transform.parent.gameObject.SetActive(true); }
            SetGameObjectActive(hud.StomachOutline, true);

            if (hud.Hydration != null) { hud.Hydration.gameObject.SetActive(true); hud.Hydration.enabled = true; if (hud.Hydration.transform.parent != null) hud.Hydration.transform.parent.gameObject.SetActive(true); }
            SetGameObjectActive(hud.ThirstOutline, true);

            if (hud.ArmorBar != null) { hud.ArmorBar.gameObject.SetActive(true); hud.ArmorBar.enabled = true; }
            if (hud.ColdArmorBar != null) { hud.ColdArmorBar.gameObject.SetActive(true); hud.ColdArmorBar.enabled = true; }
        }
        catch { }
    }

    void SetGameObjectActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    void InitStyles()
    {
        if (stylesInitialized) return;

        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = labelFontSize, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        labelStyle.normal.textColor = labelColor;

        valueStyle = new GUIStyle(GUI.skin.label) { fontSize = valueFontSize, fontStyle = FontStyle.Normal, alignment = TextAnchor.MiddleRight };
        valueStyle.normal.textColor = valueColor;

        stylesInitialized = true;
    }

    void DrawCustomHud()
    {
        Vector2 startPos = CalculateStartPosition();
        float currentY = startPos.y;

        if (showBackground)
        {
            float panelHeight = (barHeight + barSpacing) * 6 + (10 * scale);
            float panelWidth = barWidth + (20 * scale);
            if (showLabels) panelWidth += labelWidth;
            if (showValues) panelWidth += valueWidth;

            GUI.color = panelBgColor;
            GUI.DrawTexture(new Rect(startPos.x - (10 * scale), startPos.y - (5 * scale), panelWidth, panelHeight), whiteTexture);
            GUI.color = Color.white;
        }

        DrawStatBar(startPos.x, currentY, "Health", LocalPlayer.Stats.Health, 100f, healthFillColor, healthBgColor, true);
        currentY += barHeight + barSpacing;

        DrawStatBar(startPos.x, currentY, "Stamina", LocalPlayer.Stats.Stamina, LocalPlayer.Stats.Energy, staminaFillColor, staminaBgColor, true);
        currentY += barHeight + barSpacing;

        DrawStatBar(startPos.x, currentY, "Energy", LocalPlayer.Stats.Energy, 100f, energyFillColor, energyBgColor, true);
        currentY += barHeight + barSpacing;

        DrawStatBar(startPos.x, currentY, "Food", LocalPlayer.Stats.Fullness * 100f, 100f, foodFillColor, foodBgColor, true);
        currentY += barHeight + barSpacing;

        DrawStatBar(startPos.x, currentY, "Water", (1f - LocalPlayer.Stats.Thirst) * 100f, 100f, waterFillColor, waterBgColor, true);
        currentY += barHeight + barSpacing;

        if (LocalPlayer.Stats.Armor > 0)
        {
            DrawStatBar(startPos.x, currentY, "Armor", LocalPlayer.Stats.Armor, 10f, armorFillColor, armorBgColor, false);
        }
    }

    void DrawStatBar(float x, float y, string label, float current, float max, Color fillColor, Color bgColor, bool enableWarning)
    {
        float currentLabelWidth = showLabels ? labelWidth : 0f;
        float currentValueWidth = showValues ? valueWidth : 0f;
        float actualBarWidth = barWidth;

        float fillAmount = Mathf.Clamp01(current / max);
        bool isWarning = enableWarning && fillAmount < warningThreshold;
        float pulse = isWarning ? (Mathf.Sin(pulseTime) * 0.3f + 0.7f) : 1f;

        if (showLabels)
        {
            GUI.color = shadowColor;
            GUI.Label(new Rect(x + 1, y + 1, currentLabelWidth, barHeight), label, labelStyle);
            GUI.color = isWarning ? Color.Lerp(labelColor, warningColor, pulse) : labelColor;
            GUI.Label(new Rect(x, y, currentLabelWidth, barHeight), label, labelStyle);
        }

        float barX = x + currentLabelWidth;

        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(barX, y, actualBarWidth, barHeight), whiteTexture);

        Color currentFillColor = isWarning ? Color.Lerp(fillColor, warningColor, pulse) : fillColor;
        currentFillColor.a *= pulse;
        GUI.color = currentFillColor;
        GUI.DrawTexture(new Rect(barX, y, actualBarWidth * fillAmount, barHeight), whiteTexture);

        if (showBorder)
        {
            GUI.color = isWarning ? Color.Lerp(borderColor, warningColor, pulse * 0.5f) : borderColor;
            DrawBorder(new Rect(barX, y, actualBarWidth, barHeight), borderWidth);
        }

        if (showValues)
        {
            string valueText = string.Format("{0:F0}/{1:F0}", current, max);
            float valueX = barX + actualBarWidth + (5 * scale);
            GUI.color = shadowColor;
            GUI.Label(new Rect(valueX + 1, y + 1, currentValueWidth, barHeight), valueText, valueStyle);
            GUI.color = isWarning ? Color.Lerp(valueColor, warningColor, pulse) : valueColor;
            GUI.Label(new Rect(valueX, y, currentValueWidth, barHeight), valueText, valueStyle);
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
        if (showLabels) totalWidth += labelWidth;
        if (showValues) totalWidth += valueWidth;

        float x = offsetX;
        float y = offsetY;

        switch (currentPosition)
        {
            case HudPosition.TopLeft: x = offsetX; y = offsetY; break;
            case HudPosition.TopCenter: x = (Screen.width - totalWidth) / 2; y = offsetY; break;
            case HudPosition.TopRight: x = Screen.width - totalWidth - offsetX; y = offsetY; break;
            case HudPosition.BottomLeft: x = offsetX; y = Screen.height - totalHeight - offsetY; break;
            case HudPosition.BottomCenter: x = (Screen.width - totalWidth) / 2; y = Screen.height - totalHeight - offsetY; break;
            case HudPosition.BottomRight: x = Screen.width - totalWidth - offsetX; y = Screen.height - totalHeight - offsetY; break;
        }

        return new Vector2(x, y);
    }
}