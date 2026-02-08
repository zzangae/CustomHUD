[English](/doc/english.md)

---

# The Forest Custom HUD Mod

## 변경사항 : 1.0.0.3
- 좌측 상단: HP, Armor 바 (Armor 항상 표시)
- 우측 하단: 4등분 부채꼴 원형 (Energy, Stamina, Water, Food)
- 부채꼴에 라벨 + 점수 표시
- 부채꼴 간 각도 간격 4도
- 채움과 외곽선 간 간격 추가 (외부 4px, 내부 3px)
- 텍스처 미리 생성 방식으로 성능 최적화

## 변경사항 : 1.0.0.2
- 게이지 막대 높이 증가 (14 → 20)
- 폰트 크기 증가
- Armor 유무에 따라 패널 높이 동적 조절

## 변경사항 : 1.0.0.1
- 해상도 자동 스케일링 (720p ~ 4K)
- 다양한 화면 비율 지원 (16:9, 21:9, 32:9)
- 오리지널 HUD 항상 숨김
- 플레이어 조작 가능 시점에 커스텀 HUD 표시
- 인트로/엔딩 컷씬 중 HUD 숨김

## 변경사항 : 1.0.0.0
- 원형 게이지 → 사각형 막대 스타일
- 좌측 상단 고정 위치
- 기존 HUD 자동 숨김
- 낮은 수치 경고 (깜빡임 효과)
- 싱글/멀티플레이어 지원

---

## 표시 항목
| 라벨 | 색상 | 설명 |
|------|------|------|
| Health | 빨강 | 체력 |
| Stamina | 녹색 | 스태미나 |
| Energy | 노랑 | 에너지 |
| Food | 주황 | 배고픔 |
| Water | 하늘색 | 갈증 |
| Armor | 회색 | 방어구 (장착 시) |

---

## 설치
```
ModAPI/Mods/CustomHUD/
├── ModInfo.xml
└── CustomHudMod.cs
```
1. ModAPI.exe 실행
2. "Custom HUD" 체크
3. Build → Launch Game

---

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

---

**숨기는 요소들:**
```
- HealthBar, HealthBarTarget, HealthBarOutline
- StaminaBar, StaminaBarOutline
- EnergyBar, EnergyBarOutline
- Stomach, StomachOutline, StomachStarvation
- Hydration, ThirstOutline, ThirstDamageTimer
- ArmorBar, ColdArmorBar, ArmorNibbles[]
- 각 요소의 부모 컨테이너도 숨김
```

**PlayerStats 실제 구조 요약:**
```
// 실제 사용 가능한 속성들
LocalPlayer.Stats.Health        // 체력 (0~100)
LocalPlayer.Stats.HealthTarget  // 체력 회복 목표치
LocalPlayer.Stats.Stamina       // 현재 스태미나
LocalPlayer.Stats.Energy        // 에너지 (= 최대 스태미나)
LocalPlayer.Stats.Fullness      // 배고픔 (0~1, 1=배부름)
LocalPlayer.Stats.Thirst        // 갈증 (0~1, 1=목마름)
LocalPlayer.Stats.Armor         // 방어구 (정수)
LocalPlayer.Stats.ColdArmor     // 방한 방어구 (0~1)
```

**HUD 표시 로직:**
```
Health   = Health / 100
Stamina  = Stamina / Energy (Energy가 최대값)
Energy   = Energy / 100
Food     = Fullness * 100 (0~1을 0~100으로)
Water    = (1 - Thirst) * 100 (반전해서 표시)
Armor    = Armor / 10
```

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

---

## 요구사항
- The Forest v1.11b
- ModAPI

