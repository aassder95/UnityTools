# Reward Flyer Sample

Package Manager에서 이 샘플을 Import한 뒤 `RewardFlyerSample.unity`를 열어 Play합니다.

- **PLAY REWARD**: 12개 아이콘이 분산된 뒤 HUD로 이동합니다. 실행 중 중복 요청은 거절합니다.
- **MOVE HUD**: 이동하는 목표를 아이콘이 계속 추적하는지 확인합니다.
- **CANCEL**: 아이콘을 숨기고 완료 알림 없이 취소합니다. 다시 Play하면 생성했던 아이콘을 재사용합니다.

샘플은 Overlay Canvas를 사용합니다. Camera Canvas나 월드 시작점에는 해당 카메라를 `TryPlay` 인자로 전달하며, flyer 레이어의 카메라는 Inspector에 연결합니다. 기본 motion은 unscaled time입니다.

`RewardMotion.asset`에서 반경·시간·스케일을 조정할 수 있습니다. Icon prefab의 Image는 raycast를 차단하지 않습니다. 레이어에는 LayoutGroup/ContentSizeFitter를 붙이지 않습니다.

`Tools > UnityTools > Create Reward Flyer Sample`은 `Assets/RewardFlyerSample`에 새 장면과 연결된 asset을 생성합니다. 이 경로의 기존 샘플 장면·아이콘 prefab을 다시 생성하므로, 변경한 샘플은 다른 경로에 저장하세요. 기존 Motion asset은 유지합니다.

이 샘플의 알림은 시각적 연출 상태입니다. 실제 재화 지급은 게임 시스템에서 처리합니다.
