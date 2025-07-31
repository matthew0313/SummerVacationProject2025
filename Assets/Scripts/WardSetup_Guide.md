# 와드 시스템 설정 가이드

## 1단계: WardManager 오브젝트 생성

### 1.1 WardManager 오브젝트 생성
1. **Hierarchy에서 우클릭** → **Create Empty**
2. **이름을 "WardManager"로 변경**
3. **WardManager 컴포넌트 추가** (Add Component → WardManager)

### 1.2 WardManager 설정
```
WardManager 컴포넌트에서:
- Ward Prefab: (나중에 설정)
- Max Wards Per Player: 3
- Ward Cooldown: 30
- Vision Layer Mask: Everything (-1)
- Global Vision Radius: 5
- Enable Fog Of War: true
```

## 2단계: Ward 프리팹 생성

### 2.1 기본 오브젝트 생성
1. **Hierarchy에서 우클릭** → **Create Empty**
2. **이름을 "Ward"로 변경**

### 2.2 컴포넌트 추가
```
Ward 오브젝트에 다음 컴포넌트들을 추가:

1. Ward (Script)
2. WardComponent (Script)
3. NetworkObject
4. SpriteRenderer
5. Animator (선택사항)
```

### 2.3 Ward 컴포넌트 설정
```
Ward 컴포넌트에서:
- Ward Component: (자동 할당됨)
- Ward Sprite: (스프라이트 할당)
- Ward Animator: (애니메이터 할당)
- Ward Type: Vision
- Placement Range: 3
- Can Be Destroyed: true
- Destroy Time: 2
```

### 2.4 WardComponent 설정
```
WardComponent에서:
- Ward Radius: 4
- Ward Duration: 60
- Ward Light Color: Cyan
- Light Intensity: 1
- Vision Layer Mask: Everything (-1)
- Ward Visual Prefab: (선택사항)
- Ward Particles: (선택사항)
- Ward Audio Source: (자동 생성됨)
- Ward Place Sound: (오디오 클립 할당)
- Ward Destroy Sound: (오디오 클립 할당)
```

### 2.5 NetworkObject 설정
```
NetworkObject 컴포넌트에서:
- Network Prefab: true
- Network Object Name: "Ward"
```

### 2.6 SpriteRenderer 설정
```
SpriteRenderer에서:
- Sprite: (와드 스프라이트 할당)
- Color: White
- Material: Default-Sprite
- Order in Layer: 1
```

## 3단계: 프리팹으로 저장

### 3.1 프리팹 생성
1. **Project 창에서 Assets/Prefabs 폴더 생성** (없다면)
2. **Ward 오브젝트를 Prefabs 폴더로 드래그**
3. **프리팹 이름을 "Ward"로 설정**

### 3.2 WardManager에 프리팹 할당
```
WardManager 컴포넌트에서:
- Ward Prefab: 방금 만든 Ward 프리팹 할당
```

## 4단계: WardUI 설정

### 4.1 UI 캔버스 생성
1. **Hierarchy에서 우클릭** → **UI** → **Canvas**
2. **Canvas 이름을 "WardUI"로 변경**

### 4.2 WardUI 패널 생성
1. **Canvas 하위에 우클릭** → **UI** → **Panel**
2. **이름을 "WardStatusPanel"로 변경**
3. **WardUI 컴포넌트 추가**

### 4.3 WardUI 하위 요소들 생성
```
WardStatusPanel 하위에 다음 요소들 생성:

1. Image (WardTypeIcon)
   - 이름: "WardTypeIcon"
   - Image 컴포넌트 추가
   - 스프라이트 할당

2. TextMeshPro - Text (UI) (WardCountText)
   - 이름: "WardCountText"
   - 텍스트: "Wards: 0/3"

3. TextMeshPro - Text (UI) (WardTypeText)
   - 이름: "WardTypeText"
   - 텍스트: "Type: Vision"

4. TextMeshPro - Text (UI) (CooldownText)
   - 이름: "CooldownText"
   - 텍스트: "Ready!"

5. Image (WardCooldownImage)
   - 이름: "WardCooldownImage"
   - Image Type: Filled
   - Fill Method: Radial 360
```

### 4.4 WardUI 컴포넌트 설정
```
WardUI 컴포넌트에서:
- Ward Status Panel: WardStatusPanel 할당
- Ward Cooldown Image: WardCooldownImage 할당
- Ward Count Text: WardCountText 할당
- Ward Type Text: WardTypeText 할당
- Cooldown Text: CooldownText 할당
- Ward Type Icon: WardTypeIcon 할당
- Vision Ward Sprite: (시야와드 스프라이트)
- Damage Ward Sprite: (데미지와드 스프라이트)
- Vision Color: Cyan
- Damage Color: Orange (1, 0.5, 0)
```

## 5단계: 네트워크 설정

### 5.1 NetworkManager 설정
```
NetworkManager에서:
- Network Prefabs에 Ward 프리팹 추가
- Spawn Method: Dynamic
```

### 5.2 NetworkPrefabList 설정
```
NetworkPrefabList에서:
- Ward 프리팹을 리스트에 추가
```

## 6단계: 테스트

### 6.1 게임 실행
1. **Play 버튼 클릭**
2. **W 키로 와드 배치 테스트**
3. **1, 2 키로 와드 타입 변경 테스트**

### 6.2 확인사항
- [ ] W 키로 와드 배치 가능
- [ ] 1, 2 키로 와드 타입 변경 가능
- [ ] UI에 와드 정보 표시
- [ ] 쿨다운 타이머 작동
- [ ] 불빛 효과 표시

## 문제 해결

### 와드가 배치되지 않는 경우
1. WardManager가 씬에 있는지 확인
2. Ward 프리팹이 WardManager에 할당되었는지 확인
3. NetworkObject 설정 확인

### 불빛이 보이지 않는 경우
1. URP 설정 확인
2. Light2D 컴포넌트 확인
3. 레이어 마스크 설정 확인

### UI가 표시되지 않는 경우
1. Canvas 설정 확인
2. WardUI 컴포넌트 설정 확인
3. UI 요소들이 올바르게 할당되었는지 확인 