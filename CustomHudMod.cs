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

    // 좌측 상단 바 설정
    private float baseBarOffsetX = 15f;
    private float baseBarOffsetY = 15f;
    private float baseBarWidth = 180f;
    private float baseBarHeight = 22f;
    private float baseBarSpacing = 6f;
    private float baseLabelFontSize = 12f;
    private float baseValueFontSize = 11f;

    // 우측 하단 원형 설정
    private float baseCircleOffsetX = 15f;
    private float baseCircleOffsetY = 15f;
    private float baseCircleRadius = 80f;
    private float baseCircleInnerRadius = 25f;

    private float scale = 1f;
    private int lastScreenWidth;
    private int lastScreenHeight;

    // 스케일된 값
    private float barOffsetX, barOffsetY, barWidth, barHeight, barSpacing;
    private float circleOffsetX, circleOffsetY, circleRadius, circleInnerRadius;
    private int labelFontSize, valueFontSize;

    // 색상 - 바
    private Color healthFillColor = new Color(0.85f, 0.2f, 0.2f, 1f);
    private Color healthBgColor = new Color(0.3f, 0.1f, 0.1f, 0.85f);
    private Color armorFillColor = new Color(0.6f, 0.6f, 0.65f, 1f);
    private Color armorBgColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);

    // 색상 - 원형 부채꼴
    private Color energyColor = new Color(0.95f, 0.85f, 0.2f, 1f);
    private Color staminaColor = new Color(0.2f, 0.8f, 0.3f, 1f);
    private Color waterColor = new Color(0.3f, 0.7f, 1f, 1f);
    private Color foodColor = new Color(1f, 0.55f, 0.2f, 1f);
    private Color circleBgColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);

    // 공통 색상
    private Color borderColor = new Color(1f, 1f, 1f, 0.6f);
    private Color labelColor = Color.white;
    private Color shadowColor = new Color(0f, 0f, 0f, 0.7f);
    private Color panelBgColor = new Color(0f, 0f, 0f, 0.5f);
    private Color warningColor = new Color(1f, 0.3f, 0.3f, 1f);
    private float warningThreshold = 0.25f;
    private float pulseSpeed = 3f;

    // 텍스처
    private Texture2D whiteTexture;
    private Texture2D[] quadrantTextures;
    private Texture2D quadrantBgTexture;
    private Texture2D circleOutlineTexture;
    private int circleTextureSize = 256;

    // 스타일
    private GUIStyle labelStyle;
    private GUIStyle circleLabelStyle;
    private bool stylesInitialized = false;

    // 상태
    private bool isInitialized = false;
    private bool playerAwake = false;
    private float pulseTime = 0f;

    // 캐시된 값
    private float[] lastFillAmounts = new float[4];
    private Texture2D[] quadrantFillTextures;

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

        CreateQuadrantTextures();

        DontDestroyOnLoad(gameObject);
        CalculateScale();

        isInitialized = true;
    }

    void CreateQuadrantTextures()
    {
        quadrantTextures = new Texture2D[4];
        quadrantFillTextures = new Texture2D[4];

        Color[] quadrantColors = { energyColor, staminaColor, waterColor, foodColor };

        for (int q = 0; q < 4; q++)
        {
            quadrantTextures[q] = CreateQuadrantTexture(q, quadrantColors[q], 1f);
            quadrantFillTextures[q] = CreateQuadrantTexture(q, quadrantColors[q], 1f);
            lastFillAmounts[q] = 1f;
        }

        quadrantBgTexture = CreateFullCircleBgTexture();
        circleOutlineTexture = CreateCircleOutlineTexture();
    }

    float GetAngleFromCenter(float dx, float dy)
    {
        float angle = Mathf.Atan2(dx, -dy) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return angle;
    }

    Texture2D CreateQuadrantTexture(int quadrant, Color color, float fillAmount)
    {
        Texture2D tex = new Texture2D(circleTextureSize, circleTextureSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = circleTextureSize / 2f;
        float outerRadius = circleTextureSize / 2f - 2f;
        float innerRadius = outerRadius * 0.31f;

        float outerGap = 4f;
        float innerGap = 3f;
        float maxFillOuter = outerRadius - outerGap;
        float minFillInner = innerRadius + innerGap;
        float fillRadius = minFillInner + (maxFillOuter - minFillInner) * fillAmount;

        float gapAngle = 4f;
        float startAngle = quadrant * 90f - 45f + gapAngle / 2f;
        if (startAngle < 0) startAngle += 360f;
        float endAngle = startAngle + 90f - gapAngle;
        if (endAngle >= 360f) endAngle -= 360f;

        Color[] pixels = new Color[circleTextureSize * circleTextureSize];

        for (int y = 0; y < circleTextureSize; y++)
        {
            for (int x = 0; x < circleTextureSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = GetAngleFromCenter(dx, dy);

                int index = y * circleTextureSize + x;

                bool inAngleRange = false;
                if (startAngle > endAngle)
                {
                    inAngleRange = (angle >= startAngle || angle < endAngle);
                }
                else
                {
                    inAngleRange = (angle >= startAngle && angle < endAngle);
                }

                if (dist <= fillRadius && dist >= minFillInner && inAngleRange)
                {
                    float edge = Mathf.Clamp01((fillRadius - dist) * 2f);
                    float innerEdge = Mathf.Clamp01((dist - minFillInner) * 2f);
                    float alpha = Mathf.Min(edge, innerEdge);

                    pixels[index] = new Color(color.r, color.g, color.b, alpha);
                }
                else
                {
                    pixels[index] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    Texture2D CreateFullCircleBgTexture()
    {
        Texture2D tex = new Texture2D(circleTextureSize, circleTextureSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = circleTextureSize / 2f;
        float outerRadius = circleTextureSize / 2f - 2f;
        float innerRadius = outerRadius * 0.31f;

        Color[] pixels = new Color[circleTextureSize * circleTextureSize];

        for (int y = 0; y < circleTextureSize; y++)
        {
            for (int x = 0; x < circleTextureSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                int index = y * circleTextureSize + x;

                if (dist <= outerRadius && dist >= innerRadius)
                {
                    float edge = Mathf.Clamp01((outerRadius - dist) * 2f);
                    float innerEdge = Mathf.Clamp01((dist - innerRadius) * 2f);
                    float alpha = Mathf.Min(edge, innerEdge) * circleBgColor.a;

                    pixels[index] = new Color(circleBgColor.r, circleBgColor.g, circleBgColor.b, alpha);
                }
                else
                {
                    pixels[index] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    Texture2D CreateCircleOutlineTexture()
    {
        Texture2D tex = new Texture2D(circleTextureSize, circleTextureSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = circleTextureSize / 2f;
        float outerRadius = circleTextureSize / 2f - 2f;
        float innerRadius = outerRadius * 0.31f;
        float lineWidth = 2f;

        Color[] pixels = new Color[circleTextureSize * circleTextureSize];

        for (int y = 0; y < circleTextureSize; y++)
        {
            for (int x = 0; x < circleTextureSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = GetAngleFromCenter(dx, dy);

                int index = y * circleTextureSize + x;
                pixels[index] = Color.clear;

                bool onOuterRing = Mathf.Abs(dist - outerRadius) < lineWidth;
                bool onInnerRing = Mathf.Abs(dist - innerRadius) < lineWidth;

                if ((onOuterRing || onInnerRing) && dist <= outerRadius && dist >= innerRadius - lineWidth)
                {
                    float alpha = 1f - Mathf.Abs(dist - (onOuterRing ? outerRadius : innerRadius)) / lineWidth;
                    pixels[index] = new Color(borderColor.r, borderColor.g, borderColor.b, alpha * borderColor.a);
                }

                if (dist > innerRadius - lineWidth && dist < outerRadius + lineWidth)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float dividerAngle = i * 90f - 45f;
                        if (dividerAngle < 0) dividerAngle += 360f;

                        float angleDiff = Mathf.Abs(angle - dividerAngle);
                        if (angleDiff > 180f) angleDiff = 360f - angleDiff;

                        float angleWidth = (lineWidth / dist) * Mathf.Rad2Deg;
                        if (angleDiff < angleWidth && dist >= innerRadius && dist <= outerRadius)
                        {
                            float alpha = 1f - angleDiff / angleWidth;
                            Color existing = pixels[index];
                            pixels[index] = new Color(
                                Mathf.Max(existing.r, borderColor.r * 0.5f),
                                Mathf.Max(existing.g, borderColor.g * 0.5f),
                                Mathf.Max(existing.b, borderColor.b * 0.5f),
                                Mathf.Max(existing.a, alpha * borderColor.a * 0.5f)
                            );
                        }
                    }
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    void UpdateQuadrantFillTexture(int quadrant, float fillAmount, Color color)
    {
        if (Mathf.Abs(lastFillAmounts[quadrant] - fillAmount) < 0.01f) return;

        lastFillAmounts[quadrant] = fillAmount;

        if (quadrantFillTextures[quadrant] != null)
        {
            Destroy(quadrantFillTextures[quadrant]);
        }
        quadrantFillTextures[quadrant] = CreateQuadrantTexture(quadrant, color, fillAmount);
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

        HideOriginalHudComplete();

        if (!playerAwake && IsPlayerAwake())
        {
            playerAwake = true;
        }
    }

    void LateUpdate()
    {
        HideOriginalHudComplete();
    }

    void OnGUI()
    {
        if (!isInitialized) return;
        if (!playerAwake) return;
        if (!CanShowHud()) return;

        InitStyles();

        DrawTopLeftBars();
        DrawBottomRightCircle();
    }

    void OnDestroy()
    {
        ShowOriginalHud();

        if (quadrantBgTexture != null) Destroy(quadrantBgTexture);
        if (circleOutlineTexture != null) Destroy(circleOutlineTexture);
        if (whiteTexture != null) Destroy(whiteTexture);

        if (quadrantTextures != null)
        {
            for (int i = 0; i < quadrantTextures.Length; i++)
            {
                if (quadrantTextures[i] != null) Destroy(quadrantTextures[i]);
            }
        }

        if (quadrantFillTextures != null)
        {
            for (int i = 0; i < quadrantFillTextures.Length; i++)
            {
                if (quadrantFillTextures[i] != null) Destroy(quadrantFillTextures[i]);
            }
        }
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

        barOffsetX = baseBarOffsetX * scale;
        barOffsetY = baseBarOffsetY * scale;
        barWidth = baseBarWidth * scale;
        barHeight = baseBarHeight * scale;
        barSpacing = baseBarSpacing * scale;

        circleOffsetX = baseCircleOffsetX * scale;
        circleOffsetY = baseCircleOffsetY * scale;
        circleRadius = baseCircleRadius * scale;
        circleInnerRadius = baseCircleInnerRadius * scale;

        labelFontSize = Mathf.Clamp(Mathf.RoundToInt(baseLabelFontSize * scale), 9, 22);
        valueFontSize = Mathf.Clamp(Mathf.RoundToInt(baseValueFontSize * scale), 8, 20);
    }

    void InitStyles()
    {
        if (stylesInitialized) return;

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = labelFontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        labelStyle.normal.textColor = labelColor;

        circleLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(valueFontSize * 0.85f),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        circleLabelStyle.normal.textColor = labelColor;

        stylesInitialized = true;
    }

    // ========== 좌측 상단 바 ==========
    void DrawTopLeftBars()
    {
        float x = barOffsetX;
        float y = barOffsetY;

        int barCount = 2;
        float panelWidth = barWidth + 20 * scale;
        float panelHeight = (barHeight + barSpacing) * barCount - barSpacing + 12 * scale;

        GUI.color = panelBgColor;
        GUI.DrawTexture(new Rect(x - 6 * scale, y - 6 * scale, panelWidth, panelHeight), whiteTexture);
        GUI.color = Color.white;

        DrawBar(x, y, "HP", LocalPlayer.Stats.Health, 100f, healthFillColor, healthBgColor, true);

        y += barHeight + barSpacing;
        DrawBar(x, y, "Armor", LocalPlayer.Stats.Armor, 10f, armorFillColor, armorBgColor, false);
    }

    void DrawBar(float x, float y, string label, float current, float max, Color fillColor, Color bgColor, bool enableWarning)
    {
        float fillAmount = Mathf.Clamp01(current / max);
        bool isWarning = enableWarning && fillAmount < warningThreshold;
        float pulse = isWarning ? (Mathf.Sin(pulseTime) * 0.3f + 0.7f) : 1f;

        Rect barRect = new Rect(x, y, barWidth, barHeight);
        Rect fillRect = new Rect(x, y, barWidth * fillAmount, barHeight);

        GUI.color = bgColor;
        GUI.DrawTexture(barRect, whiteTexture);

        if (fillAmount > 0)
        {
            Color currentFillColor = isWarning ? Color.Lerp(fillColor, warningColor, pulse) : fillColor;
            currentFillColor.a *= pulse;
            GUI.color = currentFillColor;
            GUI.DrawTexture(fillRect, whiteTexture);
        }

        GUI.color = isWarning ? Color.Lerp(borderColor, warningColor, pulse * 0.5f) : borderColor;
        DrawBorder(barRect, Mathf.Max(1f, 1.5f * scale));

        string text = string.Format("{0} {1:F0}/{2:F0}", label, current, max);
        GUI.color = shadowColor;
        GUI.Label(new Rect(x + 5 * scale + 1, y + 1, barWidth - 10 * scale, barHeight), text, labelStyle);
        GUI.color = isWarning ? Color.Lerp(labelColor, warningColor, pulse) : labelColor;
        GUI.Label(new Rect(x + 5 * scale, y, barWidth - 10 * scale, barHeight), text, labelStyle);

        GUI.color = Color.white;
    }

    void DrawBorder(Rect rect, float width)
    {
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, width), whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - width, rect.width, width), whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, width, rect.height), whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - width, rect.y, width, rect.height), whiteTexture);
    }

    // ========== 우측 하단 원형 ==========
    void DrawBottomRightCircle()
    {
        float size = circleRadius * 2;
        float x = Screen.width - circleOffsetX - size;
        float y = Screen.height - circleOffsetY - size;

        Rect circleRect = new Rect(x, y, size, size);

        float energy = Mathf.Clamp01(LocalPlayer.Stats.Energy / 100f);
        float stamina = Mathf.Clamp01(LocalPlayer.Stats.Stamina / Mathf.Max(1f, LocalPlayer.Stats.Energy));
        float water = Mathf.Clamp01(1f - LocalPlayer.Stats.Thirst);
        float food = Mathf.Clamp01(LocalPlayer.Stats.Fullness);

        float[] fillAmounts = { energy, stamina, water, food };
        Color[] colors = { energyColor, staminaColor, waterColor, foodColor };
        string[] labels = { "Energy", "Stamina", "Water", "Food" };

        // 배경
        GUI.color = Color.white;
        GUI.DrawTexture(circleRect, quadrantBgTexture);

        // 각 부채꼴
        for (int i = 0; i < 4; i++)
        {
            bool isWarning = fillAmounts[i] < warningThreshold;
            float pulse = isWarning ? (Mathf.Sin(pulseTime) * 0.3f + 0.7f) : 1f;

            UpdateQuadrantFillTexture(i, fillAmounts[i], colors[i]);

            if (fillAmounts[i] > 0 && quadrantFillTextures[i] != null)
            {
                GUI.color = isWarning ? new Color(1f, pulse, pulse, pulse) : new Color(1f, 1f, 1f, 1f);
                GUI.DrawTexture(circleRect, quadrantFillTextures[i]);
            }
        }

        // 외곽선
        GUI.color = Color.white;
        GUI.DrawTexture(circleRect, circleOutlineTexture);

        // 라벨 및 점수
        float centerX = x + size / 2;
        float centerY = y + size / 2;
        float labelRadius = circleRadius * 0.65f;

        int[] scores = {
            Mathf.RoundToInt(fillAmounts[0] * 100),
            Mathf.RoundToInt(fillAmounts[1] * 100),
            Mathf.RoundToInt(fillAmounts[2] * 100),
            Mathf.RoundToInt(fillAmounts[3] * 100)
        };

        for (int i = 0; i < 4; i++)
        {
            float angle = Mathf.Deg2Rad * (i * 90f);
            float labelX = centerX + Mathf.Sin(angle) * labelRadius;
            float labelY = centerY - Mathf.Cos(angle) * labelRadius;

            bool isWarning = fillAmounts[i] < warningThreshold;
            float pulse = isWarning ? (Mathf.Sin(pulseTime) * 0.3f + 0.7f) : 1f;

            float labelWidth = 50 * scale;
            float labelHeight = 28 * scale;
            Rect labelRect = new Rect(labelX - labelWidth / 2, labelY - labelHeight / 2, labelWidth, labelHeight);

            string displayText = labels[i] + "\n" + scores[i];

            GUI.color = shadowColor;
            GUI.Label(new Rect(labelRect.x + 1, labelRect.y + 1, labelRect.width, labelRect.height), displayText, circleLabelStyle);
            GUI.color = isWarning ? Color.Lerp(labelColor, warningColor, pulse) : labelColor;
            GUI.Label(labelRect, displayText, circleLabelStyle);
        }

        GUI.color = Color.white;
    }

    // ========== 오리지널 HUD 제어 ==========
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
}