# 🃏 Auto Battle Card Game Clone

인디게임 출품을 위해 참여한 프로젝트 [Afterschool Brawl Club](https://youtu.be/Piy6oe0A7r0?si=3vc8GGXlUzyonj3F)의 기여 부분만 클론한 리포지토리 입니다.  
카드 기반 덱 구성과 자동 전투 시스템을 중심으로, **확장 가능한 게임 구조와 클린한 클라이언트 로직 설계**에 초점을 맞췄습니다.

---

## 📌 프로젝트 개요

- **장르**: 오토 배틀러
- **플랫폼**: PC (Windows)
- **개발 목적**
  - 이하 설계 능력 검증
      - 엔진 레퍼런스에 의존하지 않는 C# (.Net Standard) 만으로 유연한 코어 시스템 설계
      - 기획자 친화적 데이터 직렬화 및 관리
      - 결정론적 시뮬레이션 기반 프레젠테이션 레이어 구현
      - 의존성과 게임 흐름을 고려한 에셋 구조 설계, 관리

> 프로젝트의 기여 부분만 클론하였기 때문에 비주얼적으로 많이 부족할 수 있으나 감안하여 봐주시기 바랍니다.

---

## 🎮 게임 소개

보드 게임 **오토배틀 챌린저스!** 를 참고하여 만든 오토배틀러 카드 게임입니다.  
플레이어는 카드로 덱을 구성하고, 전투가 시작되면 **플레이어 개입 없이 자동으로 전투가 진행**됩니다.

카드의 효과, 순서, 조합에 따라 전투 결과가 달라지며  
라운드를 반복하여 카드를 영입, 제외하며 **전략적인 덱 구성**이 핵심 플레이 요소입니다.

다만 **오토배틀 챌린저스!** 와 다르게 트로피 토큰을 없애고 승점으로 간략화했으며,  
독자적인 카드 효과를 고안해서 구현해보았습니다.

---

## 🔧 엔지니어링 기술 상세

### 1. 데이터 직렬화 및 관리

동기화 편의성, 정규화 가능 여부에 따라 Google SpreadSheet와 JSON을 채택하였습니다.  

<details>
<summary> 1-1. Google SpreadSheet로부터 데이터 직렬화</summary>

![01](ReadMeImages/01.png)

AutoBattleCardGameClone 프로젝트는 카드 및 게임의 환경 구성 및 로컬라이징을 위한 데이터를 관리하기 위한 도구로  
기획자가 쉽게 다룰 수 있고 동기화가 간편한 Google SpreadSheet를 선택하였습니다.

Google SpreadSheet를 직렬화된 데이터로 변환하여 Unity 프로젝트에 적용하기 위해서 Google Cloud와 Google Sheets API 및 [GoogleSheetsToUnity](https://assetstore.unity.com/packages/tools/utilities/google-sheets-to-unity-73410) 에셋 패키지를 활용합니다.

![02](ReadMeImages/02.png)

그러나 GoogleSheetsToUnity 에셋 패키지는 SpreadSheet를 불러오기 위해서  
Google Cloud OAuth 클라이언트 ID를 로컬에 저장하도록 설계되어 있으며,  
이는 Github 보안정책에 위배되어 프로젝트 관리에 지장을 받습니다.

![03](ReadMeImages/03.png)

이러한 이슈를 방지하기 위해서 [시크릿 로더](Assets/Scripts/Editor/GoogleClientSecretLoader.cs)를 작성하여 시크릿이 저장된 json 파일을 프로젝트 외부에서 읽어오고,  
시크릿을 저장하는 GoogleSheetsToUnity의 설정 파일은 .gitignore에 등록하여 프로젝트 내 시크릿을 저장하지 않는 방식으로  
보안을 유지하며 프로젝트 관리가 가능하도록 하였습니다.

![04](ReadMeImages/04.png)

또한 SpreadSheet를 직렬화한 정보를 저장하기 위해 ScriptableObject 기반 [DataAsset](Assets/Scripts/Data/DataAsset.cs) 클래스를 활용하며,  
DataAsset 클래스의 상속체는 참조할 SpreadSheet의 이름, 범위등을 지정하기 쉽도록 커스텀 인스펙터를 제공하고 있습니다.  
</details>

<details>

<summary>1-2. JSON을 활용한 카드 효과 데이터 직렬화</summary>

AutoBattleCardGameClone 프로젝트의 카드들은 다양한 카드 효과를 가지고 있습니다.  
각 카드의 효과 로직들이 최대한 코드 수정없이 밸런스를 조정할 수 있도록 데이터 주도 설계 방식을 채택하였습니다.

![05](ReadMeImages/05.png)

트리거 조건, 취소 조건, 적용 대상, 여러 수치 파라미터 등 효과마다 필요한 필드 구성이 다르기 때문에  
정규화된 테이블 구조로는 표현하기 어려운 관계로 Google SpreadSheet 대신 JSON을 채택했습니다.

아래 스크립트는 **공격할 때 자신이 필드의 유일한 카드일 경우 자신의 파워가 +2 증가**하는 효과를 가진 카드의 JSON 데이터 입니다.

```json
{
  "card_effect_id": "effect_power_up_self_if_attack_alone",
  "apply_triggers": [
    "OnEnterFieldAsAttacker"
  ],
  "cancel_triggers": [
    "OnSwitchToDefend",
    "OnRemainField",
    "OnLeaveField"
  ],
  "power_up_bonus": 2
}
```

![06](ReadMeImages/06.png)

JSON은 개발자에겐 친숙한 포멧이지만, Ide를 사용하지 않는 기획자에게 편집이 어려울 수 있으므로   
커스텀 에디터로 [JSON 편집 툴](Assets/Scripts/Editor/JsonEditor.cs)을 작성하여 프로젝트에서 직관적으로 편집할 수 있도록 돕고 있습니다.
</details>


### 2. 코어 시스템 설계

[ProjectABC.Core 네임스페이스](Assets/Scripts/Core)는 게임의 전반적인 규칙, 시뮬레이션, 카드 효과를 포함하고 있으며
추후 서버 애플리케이션으로 분리할 때를 대비해 순수 C#(.Net Standard)으로 작성되었습니다.

<details>
<summary>2-1. Phase 기반 파이프라인으로 관리되는 게임 진행 흐름</summary>

```pgsql
┌────────────┐
│  UI Layer  │
└─────┬──────┘
      │ Player Input
      ▼
┌───────────────┐
│    IPlayer    │  (Human / AI / Test)
└─────┬─────────┘
      │ PlayerAction
      ▼
┌──────────────────────────────┐
│          Simulation          │
│ ┌───────────┐  ┌───────────┐ │
│ │ GamePhase │→ │ GamePhase │ │
│ └───────────┘  └───────────┘ │
└─────┬────────────────────────┘
      │ change GameState, Publish ContextEvents
      ▼
┌─────────────────────────────┐
│   ContextEventBroadcaster   │
└─────┬───────────────────────┘
      │ ContextEvents
      ▼
┌───────────────────────────┐
│   ContextEventListener    │
└─────┬─────────────────────┘
      │ 
      ▼
   UI / Debug
```

게임 진행은 [Simulation.cs](Assets/Scripts/Core/Simulation.cs) 스크립트에 선언된 Simulation 객체가 IGamePhase를 순차 실행하는 구조로 설계되어 있습니다.  
Phase를 추상화해서 다루기 때문에 다음과 같은 이점을 얻을 수 있습니다.
- 게임의 Phase 단위로 기능 추가 / 교체가 용이
- 특정 Phase만 테스트 가능
- 게임 규칙 변경 시 영향 범위 최소화

</details>

<details>
<summary>2-2. IPlayer, IPlayerAction으로 추상화된 입력과 결과</summary>

```csharp
public interface IPlayer
{
    public string Name { get; }
    public bool IsLocalPlayer { get; }
    
    public Task<IPlayerAction> DeckConstructAsync();
    public Task<IPlayerAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound);
    public Task<IPlayerAction> DismissCardsAsync(PlayerState myState, DismissOnRound dismissOnRound);
    public Task WaitUntilConfirmToProceed(Type eventType);
}

public interface IPlayerAction
{
    public IPlayer Player { get; }
    public void ApplyState(GameState state);
    public void ApplyContextEvent(SimulationContextEvents events);
    public Task GetWaitConfirmTask();
}
```

[Player.cs](Assets/Scripts/Core/Player.cs) 스크립트에 선언된 **IPlayer**와 **IPlayerAction** 인터페이스 입니다.  

플레이어 입력은 **IPlayer** 인터페이스로 추상화되어 있어 입력 방식과 코어 로직이 완전히 분리될 수 있고,  
로컬 플레이어, AI, 테스트 플레이어 등 모두 동일 인터페이스로 처리할 수 있습니다.  
또한 플레이어 입력에 대한 결과를 모두 비동기로 반환하기 때문에 네트워크 지연이나 사용자 입력 대기를 자연스럽게 처리하고 있습니다.  

플레이어 입력에 대한 결과는 **IPlayerAction** 인터페이스를 사용하고 있으며 Command 패턴을 채용하여,  
이 역시 결합도를 낮추어 유연성과 확장성있는 구조를 유지할 수 있게 했습니다.

</details>

<details>
<summary>2-3. 스냅샷과 이벤트로 시뮬레이션 재현</summary>

MatchPhase에서 자동 전투의 로직은 즉시 완료되지만, 프레젠테이션 측에서는 변경점 단위로 연출을 재생해야 합니다.
이를 위해 시뮬레이션 실행과 프레젠테이션을 분리하고, **MatchSnapshot**과 **IMatchEvent**를 통해 전투 과정을 재구성할 수 있도록 설계했습니다.

#### 이벤트 로그

매치 중 발생하는 모든 상태 변화는 [IMatchEvent](Assets/Scripts/Core/MatchEvent.cs) 인터페이스를 구현한 이벤트 객체로 기록됩니다.

```csharp
public interface IMatchEvent
{
    public void RegisterEvent(IMatchContextEvent matchContextEvent);
}
```

카드 드로우, 공격 성공, 양호실 이동, 버프 적용/해제, 카드 효과 발동 등
매치 중 일어나는 모든 행위가 각각의 이벤트 클래스로 정의되어 `IMatchContextEvent.MatchEvents` 리스트에 순서대로 쌓입니다.

```csharp
public record CardMovementInfo
{
    public readonly CardLocation PreviousLocation;
    public readonly CardLocation CurrentLocation;
}
```

카드의 위치 변화는 [CardLocation](Assets/Scripts/Core/CardLocation.cs)객체가 매치 중 카드의 위치를 기록하여  
이전과 최근 위치를 담은 `CardMovementInfo`객체로 프레젠테이션 측에서 어떤 카드가 어디서 어디로 이동했는지 추적할 수 있습니다.

#### MatchSnapshot

[MatchSnapshot](Assets/Scripts/Core/MatchSnapshot.cs)은 매치의 특정 시점에서 양측의 전체 상태를 객체로 캡처합니다.

```csharp
public class MatchSnapshot
{
    public readonly IReadOnlyDictionary<IPlayer, MatchSideSnapshot> MatchSideSnapShots;
}
```

각 `MatchSideSnapshot`에는 해당 시점의 덱, 필드, 양호실의 카드 목록과 함께
각 카드에 적용된 버프 상태까지 `BuffSnapshot`으로 함께 기록됩니다.

스냅샷은 **매치 시작**(`MatchStartEvent`)과 **포지션 전환**(`SwitchPositionEvent`) 시점에 생성되어
UI가 턴 단위로 전투 상태를 복원하는 기준점 역할을 합니다.

이 구조를 통해 시뮬레이션 코어는 결과만 생성하고, 프레젠테이션 레이어는 기록된 스냅샷과 이벤트를 순차적으로 소비하며 연출을 재생하는 역할 분리가 가능해집니다.

</details>

### 3. 시뮬레이션 레이어 구현

[ProjectABC.Engine 네임스페이스](Assets/Scripts/Engine)는 Core에 대한 의존성을 가지며, 프레젠테이션을 위한 레이어를 구성합니다.

3-1. 이벤트 소싱 기반 매치 시각화
3-2. 시뮬레이션 타임스케일 시스템


### 4. 에셋 바인딩 구조 설계



---

## 📎 참고

본 리포지토리는 학습 및 포트폴리오 목적의 **클론 프로젝트**이며, 모든 스크립트는 모두 본인 [verdantray](https://github.com/verdantray)가 작성하였습니다.  
프로젝트에 포함된 리소스 중 교실, 책상 3d 모델 에셋은 [suiren22](https://github.com/suiren22)님이 제공해주셨습니다.