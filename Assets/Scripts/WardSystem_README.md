# 와드 시스템 (Ward System)

롤 와드처럼 불빛을 이용한 시야 제공 시스템입니다.

## 주요 기능

### 1. 와드 타입
- **Vision Ward (시야 와드)**: 기본 시야 제공 (청록색)
- **Stealth Ward (은신 감지 와드)**: 은신한 적 감지 (빨간색)
- **Damage Ward (데미지 와드)**: 적에게 데미지 (주황색)
- **Healing Ward (힐링 와드)**: 아군 힐링 (초록색)

### 2. 와드 컴포넌트 (WardComponent)
- 불빛을 이용한 시야 제공
- 지속시간 관리 (기본 60초)
- 파티클 효과 및 사운드 지원
- 네트워크 동기화

### 3. 와드 매니저 (WardManager)
- 플레이어별 와드 관리
- 쿨다운 시스템 (기본 30초)
- 최대 와드 개수 제한 (기본 3개)
- 포그 오브 워 시스템

## 사용법

### 플레이어 컨트롤
- **W 키**: 와드 배치
- **1 키**: Vision Ward 선택
- **2 키**: Stealth Ward 선택
- **3 키**: Damage Ward 선택
- **4 키**: Healing Ward 선택

### 와드 배치
1. 원하는 와드 타입을 선택 (1-4 키)
2. 마우스로 배치 위치를 지정
3. W 키를 눌러 와드 배치
4. 와드는 플레이어로부터 일정 거리 내에만 배치 가능

### 와드 UI
- 현재 와드 타입 표시
- 남은 와드 개수 표시
- 쿨다운 타이머 표시
- 와드 배치 가능 여부 표시

## 설정

### WardComponent 설정
```csharp
[Header("Ward Settings")]
[SerializeField] private float wardRadius = 4f;        // 와드 범위
[SerializeField] private float wardDuration = 60f;     // 지속시간
[SerializeField] private Color wardLightColor = Color.cyan;  // 불빛 색상
[SerializeField] private float lightIntensity = 1f;    // 불빛 강도
```

### WardManager 설정
```csharp
[Header("Ward Management")]
[SerializeField] private int maxWardsPerPlayer = 3;    // 최대 와드 개수
[SerializeField] private float wardCooldown = 30f;     // 쿨다운 시간
[SerializeField] private bool enableFogOfWar = true;   // 포그 오브 워 활성화
```

## 프리팹 설정

### 와드 프리팹 구성
1. **Ward** 컴포넌트 추가
2. **WardComponent** 컴포넌트 추가
3. **NetworkObject** 컴포넌트 추가
4. **SpriteRenderer** 추가 (시각적 표현)
5. **Light2D** 컴포넌트 추가 (불빛 효과)

### WardManager 설정
1. WardManager 오브젝트 생성
2. WardManager 컴포넌트 추가
3. Ward Prefab 할당
4. 네트워크 설정

## 네트워크 동기화

모든 와드 관련 데이터는 네트워크를 통해 동기화됩니다:
- 와드 위치 및 상태
- 플레이어별 와드 개수
- 쿨다운 타이머
- 와드 생성/파괴 이벤트

## 확장 가능한 기능

### 추가 와드 타입
```csharp
public enum WardType
{
    Vision,     // 시야 제공
    Stealth,    // 은신 감지
    Damage,     // 데미지 트랩
    Healing,    // 힐링
    // 새로운 타입 추가 가능
    Teleport,   // 텔레포트
    Shield,     // 방어막
    // ...
}
```

### 커스텀 와드 효과
```csharp
// WardComponent에서 새로운 효과 추가
public void AddCustomEffect(WardEffect effect)
{
    // 커스텀 효과 구현
}
```

## 문제 해결

### 와드가 배치되지 않는 경우
1. 쿨다운 확인
2. 최대 와드 개수 확인
3. 배치 위치 유효성 확인
4. 네트워크 연결 상태 확인

### 불빛이 보이지 않는 경우
1. Light2D 컴포넌트 확인
2. URP 설정 확인
3. 레이어 마스크 설정 확인

### 네트워크 동기화 문제
1. NetworkObject 설정 확인
2. RPC 함수 호출 확인
3. 서버 권한 확인 