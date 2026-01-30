[English](/doc/english.md)

---

# Custom HUD Mod v1.0.0.0

The Forest용 커스텀 HUD 모드

---

## 변경사항 : 1.0.0.1
- 해상도 자동 스케일링 (720p ~ 4K)
- 다양한 화면 비율 지원 (16:9, 21:9, 32:9)
- 오리지널 HUD 항상 숨김
- 플레이어 조작 가능 시점에 커스텀 HUD 표시
- 인트로/엔딩 컷씬 중 HUD 숨김

---

## 기능
- 원형 게이지 → 사각형 막대 스타일
- 좌측 상단 고정 위치
- 기존 HUD 자동 숨김
- 낮은 수치 경고 (깜빡임 효과)

## 표시 항목
| 라벨 | 색상 | 설명 |
|------|------|------|
| Health | 빨강 | 체력 |
| Stamina | 녹색 | 스태미나 |
| Energy | 노랑 | 에너지 |
| Food | 주황 | 배고픔 |
| Water | 하늘색 | 갈증 |
| Armor | 회색 | 방어구 (장착 시) |

## 설치
```
ModAPI/Mods/CustomHUD/
├── ModInfo.xml
└── CustomHudMod.cs
```
1. ModAPI.exe 실행
2. "Custom HUD" 체크
3. Build → Launch Game

## 기존 HUD 완전 제거 방법
```
// Update()와 LateUpdate() 모두에서 숨김 처리
void Update()
{
    HideOriginalHudComplete();  // 매 프레임 실행
}

void LateUpdate()
{
    HideOriginalHudComplete();  // 한번 더 확인
}
```

**숨기는 요소들:**
- HealthBar, HealthBarTarget, HealthBarOutline
- StaminaBar, StaminaBarOutline
- EnergyBar, EnergyBarOutline
- Stomach, StomachOutline, StomachStarvation
- Hydration, ThirstOutline, ThirstDamageTimer
- ArmorBar, ColdArmorBar, ArmorNibbles[]
- 각 요소의 부모 컨테이너도 숨김

---

### HUD 미리보기 (좌측 상단)
```
┌─────────────────────────────────────────────┐
│  Health   ████████████████░░░░  80/100      │
│  Stamina  ██████████████████░░  95/100      │
│  Energy   ████████████░░░░░░░░  60/100      │
│  Food     ██████░░░░░░░░░░░░░░  30/100      │
│  Water    ████████████████░░░░  75/100      │
│  Armor    ████████░░░░░░░░░░░░  4/10        │
└─────────────────────────────────────────────┘
                                    (나머지 화면)
```

## 요구사항
- The Forest v1.11b
- ModAPI

## 버전
- v1.0.0.0 (2025-01-30)
