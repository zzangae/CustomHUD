# Custom HUD Mod for The Forest

## Changelog: v1.0.0.2

- Increased gauge bar height
- Increased font size
- Dynamically adjust panel height based on armor presence

## Changelog: v1.0.0.1

- Automatic resolution scaling (720p ~ 4K)
- Multiple aspect ratio support (16:9, 21:9, 32:9)
- Original HUD always hidden
- Custom HUD displayed when player gains control
- HUD hidden during intro/ending cutscenes

## Changelog: v1.0.0.0

- Circular gauge → Square bar style
- Fixed position in the top left corner
- Existing HUD auto-hides
- Low value warning (blinking effect)
- Single/multiplayer support

---

## Features

- Circular gauges → Rectangular bar style
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
├── ModInfo.xml
└── CustomHudMod.cs
```

1. Run ModAPI.exe
2. Check "Custom HUD"
3. Build → Launch Game

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
┌─────────────────────────────────────────────┐
│  Health   ████████████████░░░░  80/100      │
│  Stamina  ██████████████████░░  95/100      │
│  Energy   ████████████░░░░░░░░  60/100      │
│  Food     ██████░░░░░░░░░░░░░░  30/100      │
│  Water    ████████████████░░░░  75/100      │
│  Armor    ████████░░░░░░░░░░░░  4/10        │
└─────────────────────────────────────────────┘
                                    (Rest of screen)
```

## Requirements

- The Forest v1.11b
- ModAPI

## Version

- v1.0.0.0 (2025-01-30)
