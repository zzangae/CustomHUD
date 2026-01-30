# Custom HUD Mod v1.0.0.0

Custom HUD Mod for The Forest

---

## Changelog: v1.0.0.1

- Automatic resolution scaling (720p ~ 4K)
- Multiple aspect ratio support (16:9, 21:9, 32:9)
- Original HUD always hidden
- Custom HUD displayed when player gains control
- HUD hidden during intro/ending cutscenes

---

## Features

- Circular gauges ⊥ Rectangular bar style
- Fixed position at top-left corner
- Automatic original HUD hiding
- Low value warning (blinking effect)

## Display Items

| Label | Color | Description |
|-------|-------|-------------|
| Health | Red | Health |
| Stamina | Green | Stamina |
| Energy | Yellow | Energy |
| Food | Orange | Hunger |
| Water | Cyan | Thirst |
| Armor | Gray | Armor (when equipped) |

## Installation

```
ModAPI/Mods/CustomHUD/
戍式式 ModInfo.xml
戌式式 CustomHudMod.cs
```

1. Run ModAPI.exe
2. Check "Custom HUD"
3. Build ⊥ Launch Game

## Complete Original HUD Removal

```csharp
// Hide in both Update() and LateUpdate()
void Update()
{
    HideOriginalHudComplete();  // Execute every frame
}

void LateUpdate()
{
    HideOriginalHudComplete();  // Double check
}
```

**Hidden Elements:**

- HealthBar, HealthBarTarget, HealthBarOutline
- StaminaBar, StaminaBarOutline
- EnergyBar, EnergyBarOutline
- Stomach, StomachOutline, StomachStarvation
- Hydration, ThirstOutline, ThirstDamageTimer
- ArmorBar, ColdArmorBar, ArmorNibbles[]
- Parent containers of each element are also hidden

---

### HUD Preview (Top-Left)

```
忙式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式忖
弛  Health   ????????????????????  80/100      弛
弛  Stamina  ????????????????????  95/100      弛
弛  Energy   ????????????????????  60/100      弛
弛  Food     ????????????????????  30/100      弛
弛  Water    ????????????????????  75/100      弛
弛  Armor    ????????????????????  4/10        弛
戌式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式戎
                                    (Rest of screen)
```

## Requirements

- The Forest v1.11b
- ModAPI

## Version

- v1.0.0.0 (2025-01-30)