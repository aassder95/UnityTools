# Reward Flyer

`UiRewardFlyer`는 아이콘을 분산시킨 뒤 지정한 HUD로 이동시키는 uGUI 연출입니다. 전역 상태·씬 검색·Resources 경로·외부 tween 패키지를 사용하지 않습니다. 실제 재화 지급은 게임 시스템이 소유합니다.

## 연결

1. Canvas 아래 전용 RectTransform 레이어에 `UiRewardFlyer`를 추가합니다. 레이어에는 LayoutGroup/ContentSizeFitter를 붙이지 않습니다.
2. `_rtIconPrefab`에 아이콘 RectTransform prefab을 연결합니다. 비활성 prefab을 권장하며, Image/Text의 Raycast Target을 끕니다. 연출은 아이콘의 local position·rotation·scale을 소유합니다. 레이아웃·자체 애니메이션·활성화 콜백으로 이 상태를 변경하는 component는 붙이지 않습니다.
3. `Create > UnityTools > UI > Reward Motion`으로 생성한 asset을 `_motion`에 연결합니다. 두 참조는 필수입니다.
4. `_camLayer`는 **연출 레이어가 속한 Canvas의 카메라**입니다. Screen Space Overlay에서는 null, Screen Space Camera/World Space에서는 해당 Canvas 카메라를 명시적으로 연결합니다.
5. `_maxIconCnt`로 표시 상한을 정합니다. `Prewarm()`은 이 수까지 미리 생성합니다. 호출하지 않으면 첫 요청에서 필요한 수만 생성하고 이후 재사용합니다.

```csharp
// source와 HUD가 Overlay Canvas에 있을 때
bool isStarted = flyer.TryPlay(rtSource, null, rtHud, null, 12);

// 월드 오브젝트 → Overlay HUD
bool isWorldRewardStarted = flyer.TryPlay(trPickup, worldCamera, rtHud, null, 12);

// 서로 다른 Camera Canvas 사이에서도 각 좌표계의 카메라를 전달
bool isCameraRewardStarted = flyer.TryPlay(rtSource, sourceCanvasCamera, rtHud, hudCanvasCamera, 12);
```

camera 인자의 null은 Overlay 좌표를 뜻하며 자동으로 Camera.main을 찾지 않습니다. source는 시작 위치를 한 번만 읽고, target은 매 프레임 다시 투영합니다. source·target Transform의 위치가 아이콘의 중심점입니다. 레이어의 pivot·Canvas Scaler는 Unity 좌표 변환에 반영됩니다. 분산 반경은 레이어의 로컬 UI 단위입니다.

## 요청과 알림

- `TryPlay`는 비활성 host, 진행 중 중복 요청, null source/target, 비활성 target, 0 이하 또는 상한 초과 개수, 카메라 뒤의 시작/목표 위치를 거절합니다. 실패하면 기존 연출을 유지합니다. 필수 Inspector 참조 누락을 정상적인 false로 처리하지 않습니다.
- 동시 batch는 host 하나당 하나입니다. 교체하려면 `Cancel()` 후 새 요청을 시작합니다. 서로 다른 아이콘 prefab이나 동시에 진행할 연출에는 별도 host를 연결합니다.
- `OnArrived(int)`는 **이번 요청의 누적 도착 수**가 바뀐 프레임에 한 번 알립니다. 같은 프레임에 여러 아이콘이 도착할 수 있습니다. 도착 아이콘은 비활성화해 재사용합니다.
- 마지막 도착 알림 후 `IsPlaying`이 false가 되고 `OnCompleted`를 한 번 알립니다. 완료 handler에서는 새 요청을 시작할 수 있습니다. 마지막 `OnArrived` handler에서 취소·재시작하면 이전 요청의 `OnCompleted`는 발생하지 않습니다.
- `Cancel()`, host/부모 비활성화, target 파괴·비활성화, 사용 중이던 카메라 소멸, target이 카메라 뒤로 이동한 경우에는 아이콘을 숨기고 `OnCancelled`를 한 번 알립니다. 완료 알림은 발생하지 않습니다.
- `RequestedCnt`와 `ArrivedCnt`는 완료·취소 후에도 유지하고, 다음 요청 성공 시 다시 설정합니다. 알림 구독은 OnEnable/OnDisable 또는 소유자의 Init/Release에서 대칭으로 관리합니다.
- 모든 API는 Unity 메인 스레드에서 호출합니다. 알림은 연출 상태를 표시하기 위한 것으로, 구독 여부에 따라 실행 경로가 달라지지 않습니다.

## Motion과 수명

Motion asset에서 분산 반경, 분산/대기/이동 시간, 아이콘 간 시작 간격, 시작/분산/도착 스케일을 조정합니다. 반경·시간·스케일은 유한한 0 이상의 값으로 설정하고 실행 중 asset을 변경하지 않습니다. 0초 단계는 즉시 건너뛰며 모든 단계가 0초여도 다음 Update에서 완료됩니다. 이동 보간은 smoothstep입니다. 기본 unscaled time은 게임이 일시정지돼도 진행하며, 옵션을 끄면 Unity scaled time을 사용합니다.

아이콘은 host별로 소유·재사용하며 Update에서 생성·LINQ·씬 검색을 하지 않습니다. host 비활성화는 아이콘을 보관하고, component 파괴 시 소유 아이콘을 정리합니다. Prewarm/첫 요청의 생성 비용과 수신 handler의 비용은 별도입니다.

`Samples~/Reward Flyer Sample`에는 연결된 장면·아이콘 prefab·Motion asset·수동 재생/취소/목표 이동 버튼을 포함합니다. 카메라 조합·좌표 변환·취소·재사용은 자동 테스트 범위이며 실제 기기의 배치·터치·성능은 별도 검증이 필요합니다.

## 검증 기록

2026-10-06, Local UPM 소스로 별도의 빈 프로젝트에서 확인했습니다.

| Editor | Play Mode | Windows Development Build |
| --- | --- | --- |
| Unity 2022.3.62f3, UI + Input System | 58/58 | 성공, UI/Reward Flyer 샘플 포함 |
| Unity 6000.3.20f1, UI 단독 | 57/57 | 성공, Reward Flyer 샘플 포함 |

두 테스트 실행 모두 `UiRewardFlyerTests` 14건을 포함합니다. 2022.3에서는 Package Manager의 Sample Import와 장면의 누락 스크립트 검사를 확인했고, 6.3에서는 Editor 생성기로 만든 샘플의 연결과 빌드를 확인했습니다. 카메라 Canvas 테스트는 첫 화면 배치 이후 좌표를 비교합니다. 정적 UPM 검사·샘플 사본 일치 검사도 통과했습니다. Player 직접 조작·모바일·IL2CPP·기기 성능 검증은 포함하지 않습니다.
